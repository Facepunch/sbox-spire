namespace Spire.Abilities;

public partial class BombThrowAbility : PlayerAbility
{
	// Configuration
	public override string Identifier => "bomb_throw";
	public override PlayerAbilityType Type => PlayerAbilityType.Ultimate;

	public virtual float ProjectileSpeed => 500f;
	public virtual float ProjectileRadius => 20f;
	public virtual float ProjectileThrowStrength => 100f;

	public virtual float BombExplosionRadius => 128f;

	protected override void PreRun()
	{
		base.PreRun();

		if ( Host.IsClient )
			return;

		var projectile = new BouncyProjectileEntity()
		{
			Bounciness = 0.7f,
			ReflectionScale = 0.5f,
			IgnoreEntity = Character,
			Attacker = Character,
			LifeTime = 2.5f,
			Gravity = 30f,
			ModelPath = "models/projectiles/small_bomb.vmdl",
			TrailEffect = "particles/weapons/crossbow/crossbow_trail.vpcf"
		};

		projectile.OnBounce += OnProjectileBounce;


		var position = Character.EyePosition + Vector3.Down * 25f;
		var forward = Character.EyeRotation.Forward;
		var endPosition = position + Vector3.Up * 256f + forward * 200f;
		var trace = Trace.Ray( position, endPosition )
			.Ignore( Character )
			.Run();

		var direction = (trace.EndPosition - position).Normal;
		direction = direction.Normal;

		var velocity = (direction * ProjectileSpeed) + (Character.EyeRotation.Forward * ProjectileThrowStrength);
		projectile.Initialize( position, velocity, ProjectileRadius, OnProjectileHit );
	}


	protected DangerAreaEntity DangerEntity;
	protected void OnProjectileBounce( ProjectileEntity projectile, Entity hitEntity )
	{
		if ( !DangerEntity.IsValid() )
		{
			DangerEntity = new DangerAreaEntity
			{
				Position = projectile.Position + Vector3.Down * 5f,
				ParticlePath = "particles/danger/danger_aoe.vpcf",
				MaxRadius = BombExplosionRadius,
				UntilFilled = 2.0f,
				UntilRemoved = 2.5f,
				DangerType = DangerType.Out,
				Parent = projectile
			};
		}
	}

	protected void OnProjectileHit( ProjectileEntity projectile, Entity hitEntity )
	{
		new ExplosionEntity
		{
			Position = projectile.Position,
			Radius = BombExplosionRadius,
			Damage = 50f,
			ForceScale = 1f,
		}.Explode( projectile );
	}
}
