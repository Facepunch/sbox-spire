using Spire.Abilities;

namespace Spire.UI;

[UseTemplate]
public partial class WeaponAbilityPanel : BaseAbilityPanel
{
	public WeaponAbilityType Type { get; set; } = WeaponAbilityType.Attack;

	public override void SetProperty( string name, string value )
	{
		base.SetProperty( name, value );

		if ( name == "type" )
		{
			var enumType = Enum.Parse<WeaponAbilityType>( value, true );
			Type = enumType;
		}
	}

	protected override InputButton GetButton()
	{
		return Type.GetButton();
	}

	protected override Ability GetAbility()
	{
		return GetWeapon()?.GetAbilities().FirstOrDefault( x => x.IsValid() && x.Type == Type );
	}
}
