namespace Spire.Abilities;

public partial class SwordHeavyAttack : BaseMeleeAttackAbility
{
	// Configuration
	public override string Identifier => "sword_heavy_attack";
	public override WeaponAbilityType Type => WeaponAbilityType.Special;

	public override float BaseDamage => 60f;

	protected override void OnTargetDamaged( Entity entity, DamageInfo damageInfo )
	{
		CreateParticles( "hit" );
		Particles.Create( "particles/explosion/barrel_explosion/explosion_fire_ring.vpcf", entity.Position );
	}
}
