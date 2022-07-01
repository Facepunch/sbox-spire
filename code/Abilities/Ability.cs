namespace Spire.Abilities;

// @TODO: These really shouldn't be entities, they were BaseNetworkable
// but they suck on hotload
public abstract partial class Ability : Entity
{
	public Ability()
	{
		Transmit = TransmitType.Always;

		Data = AbilityGameResource.TryGet( Identifier );

		if ( Data is null )
		{
			Log.Warning( $"Couldn't find AbilityGameResource for Ability {Identifier} " );
		}

		Interaction = AbilityInteraction.From( this );
	}

	public virtual string Identifier => "ability";

	public AbilityGameResource Data { get; set; }

	[Net, Predicted]
	public AbilityInteraction Interaction { get; set; }

	// Quick Accessors
	public string GetIcon() => Data.Icon.Replace( "jpg", "png" );

	// Network Variables
	[Net, Predicted]
	public TimeSince TimeSinceLastUse { get; set; }

	[Net, Predicted]
	public TimeUntil TimeUntilFinish { get; set; }

	[Net, Predicted]
	public TimeUntil TimeUntilNextUse { get; set; }

	[Net, Predicted]
	public bool InProgress { get; set; }

	[Net, Predicted]
	public Entity Entity { get; set; }

	// Implementation

	public virtual PlayerCharacter GetCharacter()
	{
		if ( Entity is PlayerCharacter character )
			return character;
		else
			return Entity.Owner as PlayerCharacter;
	}

	public virtual void DoPlayerAnimation()
	{
		GetCharacter()?.SetAnimParameter( "b_attack", true );
	}

	/// <summary>
	/// Called just before an ability is ran.
	/// </summary>
	protected virtual void PreRun()
	{
		PlaySound( "pre" );
		CreateParticles( "pre" );

		var character = GetCharacter();
		if ( character.IsValid() )
			character.Controller.SpeedMultiplier = Data.CharacterSpeedMod;
	}

	/// <summary>
	/// Called after the ability has finished. If specified, this will be executed in <paramref name="Duration"/> seconds.
	/// </summary>
	protected virtual void PostRun()
	{
		PlaySound( "post" );
		CreateParticles( "post" );

		var character = GetCharacter();
		if ( character.IsValid() )
			character.Controller.SpeedMultiplier = 1f;

		if ( Data.RunDefaultPlayerAnimation )
			DoPlayerAnimation();
	}

	/// <summary>
	/// Returns whether or not an ability can be executed.
	/// </summary>
	/// <returns></returns>
	public virtual bool CanRun()
	{
		return TimeUntilNextUse <= 0 && !InProgress;
	}

	/// <summary>
	/// Start an ability
	/// </summary>
	public void Run()
	{
		if ( !CanRun() )
			return;

		GetCharacter().LastAbilityUsed = 0f;

		TimeUntilFinish = Data.Duration;
		InProgress = true;
		TimeSinceLastUse = 0f;

		if ( Data.Duration > 0f )
		{
			PreRun();
		}
		else
		{
			PreRun();
			PostRun();
		}
	}

	public void Interact()
	{
		Interaction.Start();
	}

	/// <summary>
	/// Called every tick (shared) for each ability
	/// </summary>
	public virtual void OnTick()
	{

	}

	/// <summary>
	/// Allows abilities to define their own widget when a player is interacting with it
	/// </summary>
	/// <returns></returns>
	public virtual bool TickGuide( AbilityInteraction interaction )
	{
		return false;
	}

	public override void Simulate( Client cl )
	{
		OnTick();

		if ( !InProgress )
			return;

		if ( TimeUntilFinish )
		{
			InProgress = false;
			TimeUntilNextUse = Data.Cooldown;

			PostRun();
		}
	}

	public override void FrameSimulate( Client cl )
	{
		OnTick();
	}

	public void PlaySound( string tag, Entity entity )
	{
		var soundEntry = Rand.FromList( Data.SoundsWithTag( tag ) );

		if ( string.IsNullOrEmpty( soundEntry.Sound ) )
			return;

		entity.PlaySound( soundEntry.Sound, soundEntry.Attachment );
	}

	public new void PlaySound( string tag ) => PlaySound( tag, Entity );

	public void CreateParticle( AbilityGameResource.ParticleEntry entry, Entity entity )
	{
		if ( string.IsNullOrEmpty( entry.Particle ) )
			return;

		Util.CreateParticle(
			entry.FromCharacter ? GetCharacter() : entity,
			entry.Particle,
			true,
			entry.Attachment,
			entry.Lifetime
		);
	}

	public void CreateParticles( string tag, Entity entity )
	{
		Data.ParticlesWithTag( tag )
			.ForEach( x => CreateParticle( x, entity ) );
	}

	public void CreateParticles( string tag ) => CreateParticles( tag, Entity );
}
