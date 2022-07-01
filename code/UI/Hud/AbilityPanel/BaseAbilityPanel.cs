using Spire.Abilities;

namespace Spire.UI;

public partial class BaseAbilityPanel : Panel
{
	public Ability Ability { get; set; }

	// @ref
	public InputHint InputHint { get; set; }

	public string CooldownString
	{
		get
		{
			if ( Ability is null || Ability.InProgress )
				return "";

			var nextUse = (float)Ability.TimeUntilNextUse;

			if ( nextUse <= 2 && nextUse >= 0 )
				return $"{nextUse:F1}";

			if ( nextUse.CeilToInt() <= 0 )
				return "";

			return $"{nextUse.CeilToInt()}";
		}
	}

	public BaseAbilityPanel()
	{
		AddClass( "abilitypanel" );
	}

	protected PlayerCharacter GetCharacter()
	{
		return Local.Pawn as PlayerCharacter;
	}

	protected BaseWeapon GetWeapon()
	{
		return GetCharacter()?.ActiveChild as BaseWeapon;
	}

	protected virtual void UpdateAbility( Ability ability )
	{
		Ability = ability;

		if ( Ability is not null )
		{
			SetClass( "in-use", Ability.InProgress );
			Style.SetBackgroundImage( Ability.GetIcon() );
		}
		else
		{
			SetClass( "in-use", false );
			Style.SetBackgroundImage( "" );
		}

		var character = GetCharacter();
		if ( character.IsValid() )
		{
			SetClass( "character-cooldown", !character.CanUseAbility() || !character.CanUseAbilityInteract() );
			SetClass( "interacting-self", character.InteractingAbility is not null && character.InteractingAbility == Ability );
			SetClass( "interacting", character.InteractingAbility is not null && character.InteractingAbility != Ability );
		}

		SetClass( "no-ability", Ability is null );
		SetClass( "on-cooldown", Ability?.TimeUntilNextUse > 0f );

		InputHint.SetButton( GetButton() );
	}

	protected virtual InputButton GetButton()
	{
		return InputButton.PrimaryAttack;
	}

	protected virtual Ability GetAbility()
	{
		return null;
	}

	public virtual void Update()
	{
		var ability = GetAbility();
		
		UpdateAbility( ability );
	}
}
