namespace Spire.Abilities;

public partial class WeaponAbility : Ability
{
	// Configuration
	public virtual WeaponAbilityType Type => WeaponAbilityType.Attack;

	/// <summary>
	/// The weapon this ability belongs to.
	/// </summary>
	public BaseWeapon Weapon => Entity as BaseWeapon;
}
