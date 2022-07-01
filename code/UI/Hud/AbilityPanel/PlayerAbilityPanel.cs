using Spire.Abilities;

namespace Spire.UI;

[UseTemplate]
public partial class PlayerAbilityPanel : BaseAbilityPanel
{
	public int Slot { get; set; } = 0;

	protected override InputButton GetButton()
	{
		return PlayerAbility.GetInputButtonFromSlot( Slot );
	}

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "type" )
			Slot = value.ToInt( 0 );
	}

	protected override Ability GetAbility()
	{
		return GetCharacter().GetAbilityFromSlot( Slot );
	}
}
