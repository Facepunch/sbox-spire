namespace Spire.Abilities;

/// <summary>
/// Configurable Game Data for Spire Abilities
/// </summary>
[GameResource( "Spire Ability Definition", "ability", "An ability", Icon = "star" )]
public partial class AbilityGameResource : GameResource
{
	public struct SoundEntry
	{
		public string Tag { get; set; }

//		I want to be able to do this, but I can't. This is on purpose.
//		[ResourceType( "sound" )]
		public string Sound { get; set; }

		public string Attachment { get; set; }

		public override string ToString() => $"Sound";
	}

	public struct ParticleEntry
	{
		public string Tag { get; set; }

		[ResourceType( "vpcf" )]
		public string Particle { get; set; }

		public bool FromCharacter { get; set; }

		public string Attachment { get; set; }

		public float Lifetime { get; set; }

		public override string ToString() => $"Particle";
	}

	public static List<AbilityGameResource> All { get; protected set; } = new();

	public static AbilityGameResource TryGet( string id ) => All.Where( x => x.AbilityID == id ).FirstOrDefault();

	/// <summary>
	/// An identifier for this ability. Used for Ability GameResources
	/// </summary>
	[Category( "Meta" )]
	public string AbilityID { get; set; } = "ability";

	/// <summary>
	/// A friendly name for the ability
	/// </summary>
	[Category( "Meta" )]
	public string AbilityName { get; set; } = "Ability";

	/// <summary>
	/// A short description of an ability
	/// </summary>
	[Category( "Meta" )]
	public string Description { get; set; } = "This ability does nothing.";

	/// <summary>
	/// The ability's cooldown until you can run it again. This is assigned after Ability.PostRun
	/// </summary>
	[Category( "Meta" )]
	public float Cooldown { get; set; } = 5f;

	/// <summary>
	/// The ability's icon used in the game's user interface
	/// </summary>
	[Category( "Meta" )]
	[ResourceType( "jpg" )]
	public string Icon { get; set; } = "";

	[Category( "Interaction" )]
	public AbilityInteractionType InteractionType { get; set; }

	[Category( "Interaction" ), Title( "Ability Range" ), ShowIf( nameof( InteractionType ), AbilityInteractionType.WorldPoint )]
	public Vector2 _AbilityRange { get; set; } = 0f;

	[HideInEditor]
	public Range AbilityRange => new( _AbilityRange.x, _AbilityRange.y );

	[Category( "Stats" )]
	public float AbilityEffectRadius { get; set; } = 60f;

	/// <summary>
	/// Apply a speed modifier to the player while the ability is in progress
	/// </summary>
	[Category( "Stats" )]
	public float CharacterSpeedMod { get; set; } = 1f;

	/// <summary>
	/// The duration of an ability.
	/// </summary>
	[Category( "Stats" )]
	public float Duration { get; set; } = 0f;

	[Category( "Character" )]
	public bool RunDefaultPlayerAnimation { get; set; } = true;

	[Title( "Sound List" ), Category( "Effects" )]
	public List<SoundEntry> Sounds { get; set; }

	public List<SoundEntry> SoundsWithTag( string tag )
	{
		var list = new List<SoundEntry>();

		if ( Sounds is not null )
			list.AddRange( Sounds.Where( x => x.Tag == tag ) );

		list.ForEach( x => Log.Info( x.Sound ) );

		return list;
	}

	[Title( "Particle List" ), Category( "Effects" )]
	public List<ParticleEntry> Particles { get; set; }

	public List<ParticleEntry> ParticlesWithTag( string tag )
	{
		var list = new List<ParticleEntry>();

		if ( Particles is not null )
			list.AddRange( Particles.Where( x => x.Tag == tag ) );

		return list;
	}

	protected void PrecacheAssets()
	{
		if ( Particles is not null )
		{
			foreach ( var particle in Particles )
			{
				Precache.Add( particle.Particle );
			}
		}
	}

	protected override void PostLoad()
	{
		base.PostLoad();

		if ( All.Contains( this ) )
			return;

		All.Add( this );

		PrecacheAssets();
	}
}
