namespace Spire.Abilities;

public partial class BasicArrowAttack : WeaponAbility
{
	// Configuration
	public override string Identifier => "arrow_attack";
	public override WeaponAbilityType Type => WeaponAbilityType.Attack;

	public virtual float ProjectileSpeed => 800f;
	public virtual float ProjectileRadius => 10f;
	public virtual float ProjectileThrowStrength => 100f;
	public virtual bool ManualProjectile => false;

	public virtual float MaxRangeFalloff => 400f;
	public virtual float ArrowDamage => 34f;

	protected virtual void CreateProjectile( float yawOffset = 0f )
	{
		if ( Host.IsClient ) return;

		using ( Prediction.Off() )
		{
			Entity.PlaySound( "rust_crossbow.shoot" );
		}

		var projectile = new StickyProjectileEntity()
		{
			FaceDirection = true,
			IgnoreEntity = Weapon.Owner,
			Attacker = Weapon.Owner,
			LifeTime = 2.5f,
			Gravity = 0f,
			ModelPath = "models/projectiles/rust_crossbow_bolt_fixed.vmdl",
			TrailEffect = "particles/weapons/crossbow/crossbow_trail.vpcf"
		};

		var position = Weapon.Owner.EyePosition + Vector3.Down * 20f + Weapon.Owner.EyeRotation.Forward * 40f + Weapon.Owner.EyeRotation.Left * yawOffset; 

		var spread = new Angles().WithYaw( yawOffset );
		var rotation = Rotation.From( spread ) * Weapon.Owner.EyeRotation;

		var forward = rotation.Forward;
		var endPosition = position + forward * 100000f;
		var trace = Trace.Ray( position, endPosition )
			.Ignore( Weapon.Owner )
			.Run();

		var direction = (trace.EndPosition - position).Normal;
		direction = direction.Normal;

		var velocity = (direction * ProjectileSpeed) + (forward * ProjectileThrowStrength);
		projectile.Initialize( position, velocity, ProjectileRadius, OnProjectileHit );

		StartProjectilePos = position;
	}

	protected override void PostRun()
	{
		base.PostRun();

		if ( Host.IsClient )
			return;

		if ( !ManualProjectile )
			CreateProjectile();
	}

	protected Vector3 StartProjectilePos;

	protected virtual float CalculateDamage( Entity hitEntity )
	{
		return ArrowDamage - ArrowDamage.Remap( 0, MathF.Min( hitEntity.Position.Distance( StartProjectilePos ), MaxRangeFalloff ), 0f, ArrowDamage );
	}

	protected virtual void OnProjectileHit( ProjectileEntity projectile, Entity hitEntity )
	{
		if ( !hitEntity.IsValid() ) return;

		CreateParticles( "projectile_hit" );

		hitEntity.TakeDamage( DamageInfo.FromBullet( hitEntity.Position, Vector3.Zero, CalculateDamage( hitEntity ) ) );
	}
}
