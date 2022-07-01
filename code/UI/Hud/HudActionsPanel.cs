namespace Spire.UI;

[UseTemplate]
public partial class HudActionsPanel : Panel
{
	// PLAYER ABILITIES 

	// @ref
	public PlayerAbilityPanel MovementPlayerAbility { get; set; }
	// @ref
	public PlayerAbilityPanel FirstPlayerAbility { get; set; }
	// @ref
	public PlayerAbilityPanel SecondPlayerAbility { get; set; }
	// @ref
	public PlayerAbilityPanel UltimatePlayerAbility { get; set; }

	/// WEAPON ABILITIES

	// @ref
	public WeaponAbilityPanel WeaponAttackAbility { get; set; }
	// @ref
	public WeaponAbilityPanel SpecialAttackAbility { get; set; }
	// @ref
	public WeaponAbilityPanel UltimateAttackAbility { get; set; }

	public override void Tick()
	{
		base.Tick();

		var character = Local.Pawn as PlayerCharacter;
		if ( !character.IsValid() )
			return;

		MovementPlayerAbility.Update();
		FirstPlayerAbility.Update();
		SecondPlayerAbility.Update();
		UltimatePlayerAbility.Update();

		WeaponAttackAbility.Update();
		SpecialAttackAbility.Update();
		UltimateAttackAbility.Update();
	}
}
