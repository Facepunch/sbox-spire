namespace Spire.Abilities;

public partial class SwordAttack : BaseMeleeAttackAbility
{
	// Configuration
	public override string Identifier => "sword_attack";
	public override WeaponAbilityType Type => WeaponAbilityType.Attack;
}
