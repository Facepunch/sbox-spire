using Spire.Buffs;

namespace Spire.Abilities;

public partial class FireballAbility : BaseMeleeAttackAbility
{
	// Configuration
	public override string Identifier => "fireball";
	public override WeaponAbilityType Type => WeaponAbilityType.Attack;


	public virtual float ProjectileSpeed => 500f;
	public virtual float ProjectileRadius => 10f;
	public virtual float ProjectileThrowStrength => 100f;
	public virtual bool ManualProjectile => false;

	public virtual float MaxRangeFalloff => 400f;
	public virtual float Damage => 34f;

	protected virtual void CreateProjectile( float yawOffset = 0f )
	{
		if ( Host.IsClient ) return;

		PlaySound( "projectile" );

		var projectile = new ProjectileEntity()
		{
			FaceDirection = true,
			IgnoreEntity = Weapon.Owner,
			Attacker = Weapon.Owner,
			LifeTime = 2.5f,
			Gravity = 0f,
			TrailEffect = "particles/abilities/staff/fireball.vpcf"
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
		return Damage - Damage.Remap( 0, MathF.Min( hitEntity.Position.Distance( StartProjectilePos ), MaxRangeFalloff ), 0f, Damage );
	}

	protected virtual void OnProjectileHit( ProjectileEntity projectile, Entity hitEntity )
	{
		if ( !hitEntity.IsValid() ) return;

		CreateParticles( "projectile_hit", projectile );

		hitEntity.TakeDamage( DamageInfo.FromBullet( hitEntity.Position, Vector3.Zero, CalculateDamage( hitEntity ) ) );

		if ( hitEntity is BaseCharacter character )
		{
			character.AddBuff<BaseDamageBuff>();
		}
	}
}
