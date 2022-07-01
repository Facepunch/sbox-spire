namespace Spire.Abilities;

public partial class ExplosiveArrowAttack : BasicArrowAttack
{

	// Configuration
	public override string Identifier => "explosive_arrow_attack";
	public override WeaponAbilityType Type => WeaponAbilityType.Ultimate;

	protected async Task DelayedExplosion( ProjectileEntity projectile )
	{
		new DangerAreaEntity
		{
			Position = projectile.Position + Vector3.Down * 42f,
			Parent = projectile,
			ParticlePath = "particles/danger/danger_aoe.vpcf"
		};

		await GameTask.DelaySeconds( 0.5f );

		new ExplosionEntity
		{
			Position = projectile.Position,
			Radius = Data.AbilityEffectRadius,
			Damage = 50f,
			ForceScale = 1f,
		}.Explode( Owner );

		projectile?.Delete();
	}

	protected override void OnProjectileHit( ProjectileEntity projectile, Entity hitEntity )
	{
		base.OnProjectileHit( projectile, hitEntity );

		_ = DelayedExplosion( projectile );
	}
}
