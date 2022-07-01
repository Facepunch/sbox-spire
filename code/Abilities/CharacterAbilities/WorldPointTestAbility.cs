namespace Spire.Abilities;

public partial class WorldPointTestAbility : PlayerAbility
{
	// Configuration
	public override string Identifier => "world_point";
	public override PlayerAbilityType Type => PlayerAbilityType.Standard;

	public virtual float ProjectileSpeed => 800f;
	public virtual float ProjectileRadius => 10f;
	public virtual float ProjectileThrowStrength => 100f;
	public virtual bool ManualProjectile => false;

	protected virtual void CreateProjectile( Vector3 pos )
	{
		if ( Host.IsClient ) return;

		using ( Prediction.Off() )
		{
			Entity.PlaySound( "rust_crossbow.shoot" );
		}

		new DangerAreaEntity
		{
			Position = pos,
			ParticlePath = "particles/danger/danger_aoe.vpcf",
			MaxRadius = 100,
			DangerType = DangerType.In
		};

		var projectile = new ProjectileEntity()
		{
			FaceDirection = true,
			IgnoreEntity = Entity,
			Attacker = Entity,
			LifeTime = 2.5f,
			Gravity = 0f,
			ModelPath = "models/projectiles/rust_crossbow_bolt_fixed.vmdl"
		};

		var position = pos + Vector3.Up * 512f;

		var forward = Vector3.Down;
		var endPosition = position + forward * 100000f;
		var trace = Trace.Ray( position, endPosition )
			.Ignore( Entity )
			.Run();

		var direction = (trace.EndPosition - position).Normal;
		direction = direction.Normal;

		var velocity = (direction * ProjectileSpeed) + (forward * ProjectileThrowStrength);
		projectile.Initialize( position, velocity, ProjectileRadius, OnProjectileHit );
	}

	protected override void PostRun()
	{
		base.PostRun();

		if ( Host.IsClient )
			return;

		var interaction = Interaction as WorldPointAbilityInteraction;

		CreateProjectile( interaction.WorldCursorPosition );
	}

	protected virtual void OnProjectileHit( ProjectileEntity projectile, Entity hitEntity )
	{
		if ( !hitEntity.IsValid() ) return;

		CreateParticles( "projectile_hit" );

		new ExplosionEntity
		{
			Position = projectile.Position,
			Radius = 256f,
			Damage = 20f,
			ForceScale = 1f,
		}.Explode( projectile );
	}
}
