namespace Spire.Abilities;

public abstract partial class AbilityInteraction : BaseNetworkable
{
	public AbilityInteraction()
	{
	}

	public AbilityInteraction( Ability ability )
	{
		Ability = ability;
	}

	[Net]
	public Ability Ability { get; set; }

	public static AbilityInteraction From( Ability ability )
	{
		var type = ability.Data.InteractionType;

		AbilityInteraction interaction = type switch
		{
			AbilityInteractionType.Generic => new GenericAbilityInteraction(),
			AbilityInteractionType.WorldPoint => new WorldPointAbilityInteraction(),
			AbilityInteractionType.Direction => new DirectionAbilityInteraction(),
			_ => new GenericAbilityInteraction()
		};

		interaction.Ability = ability;

		return interaction;
	}

	[Net, Predicted]
	public Vector3 WorldCursorPosition { get; set; }

	public Vector3 GetWorldCursor()
	{
		var trace = Trace.Ray( CurrentView.Position, CurrentView.Position + Screen.GetDirection( Mouse.Position ) * 100000f )
			.WithoutTags( "player" )
			.Radius( 5f )
			.Run();

		return trace.HitPosition;
	}

	/// <summary>
	/// Called every tick on the client
	/// </summary>
	public virtual void OnTick()
	{
		if ( Host.IsClient )
			InternalTickGuide();
	}

	public void Start()
	{
		Ability.GetCharacter().InteractingAbility = Ability;

		OnStart();
	}

	public static AbilityGuideEntity GuideEntity { get; protected set; }
	public static AbilityGuideEntity AltGuideEntity { get; protected set; }

	protected void InternalTickGuide()
	{
		Host.AssertClient();

		var character = Ability.GetCharacter();
		if ( !character.IsValid() || !character.CanUseAbilityInteract() )
			return;

		if ( !GuideEntity.IsValid() )
			GuideEntity = new();

		if ( !AltGuideEntity.IsValid() )
			AltGuideEntity = new();

		var shouldOverride = Ability.TickGuide( this );

		if ( !shouldOverride )
			TickGuide();
	}

	protected virtual void TickGuide()
	{
	}

	protected virtual void OnStart()
	{
	}

	public static void KillGuides()
	{
		GuideEntity?.Delete();
		AltGuideEntity?.Delete();
	}

	public void Cancel()
	{
		if ( Host.IsClient )
			KillGuides();

		Ability.GetCharacter().InteractingAbility = null;

		OnCancel();
	}

	protected virtual void OnCancel()
	{
	}

	public void End()
	{
		if ( Host.IsClient )
			KillGuides();

		Ability.GetCharacter().InteractingAbility = null;

		OnEnd();
	}

	protected virtual void OnEnd()
	{
	}
}
