namespace Spire.Abilities;

public partial class PlayerAbility : Ability
{
	public virtual PlayerAbilityType Type { get; set; } = PlayerAbilityType.Standard;
	public BaseCharacter Character => Entity as BaseCharacter;

	public static InputButton GetInputButtonFromSlot( int slotIndex )
	{
		return slotIndex switch
		{
			0 => InputButton.Jump, // Movement Ability
			1 => InputButton.Reload, // First Ability
			2 => InputButton.View, // Second Ability 
			3 => InputButton.Flashlight, // Ultimate Ability
			_ => InputButton.Jump // Fallback
		};
	}
}
