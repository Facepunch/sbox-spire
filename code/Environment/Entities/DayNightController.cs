using SandboxEditor;

namespace Spire.DayNight;

[GameResource( "Time Stage Resource", "sptime", "Game resource to dictate lighting and fog information for a specific time stage." )]
public partial class TimeStageResource : GameResource
{
	public static List<TimeStageResource> All { get; protected set; } = new();
	public static TimeStageResource Get( string path ) => All.First( x => x.ResourcePath == path );

	[Category("Lighting")]
	public Color SkyColor { get; set; }
	[Category( "Lighting" )]
	public Color LightColor { get; set; }
	[Category( "Lighting" )]
	public Color AmbientLightColor { get; set; } = new Color( 0.1f, 0.1f, 0.1f );

	[Category( "Fog" )]
	public bool FogEnabled { get; set; } = true;
	[Category( "Fog" )]
	public float FogStartDistance { get; set; } = 0.0f;
	[Category( "Fog" )]
	public float FogEndDistance { get; set; } = 4000.0f;
	[Category( "Fog" )]
	public float FogStartHeight { get; set; } = 0.0f;
	[Category( "Fog" )]
	public float FogEndHeight { get; set; } = 200.0f;
	[Category( "Fog" )]
	public float FogMaximumOpacity { get; set; } = 0.5f;
	[Category( "Fog" )]
	public Color FogColor { get; set; } = Color.White;
	[Category( "Fog" )]
	public float FogStrength { get; set; } = 1.0f;
	[Category( "Fog" )]
	public float FogDistanceFalloffExponent { get; set; } = 2.0f;
	[Category( "Fog" )]
	public float FogVerticalFalloffExponent { get; set; } = 1.0f;

	protected override void PostLoad()
	{
		base.PostLoad();

		if ( !All.Contains( this ) )
			All.Add( this );
	}
}

public class DayNightGradient
{
	private struct GradientNode
	{
		public Color Color;
		public float Time;

		public GradientNode( Color color, float time )
		{
			Color = color;
			Time = time;
		}
	}

	private GradientNode[] _nodes;

	public DayNightGradient( Color dawnColor, Color dayColor, Color duskColor, Color nightColor )
	{
		_nodes = new GradientNode[7];
		_nodes[0] = new GradientNode( nightColor, 0f );
		_nodes[1] = new GradientNode( nightColor, 0.2f );
		_nodes[2] = new GradientNode( dawnColor, 0.3f );
		_nodes[3] = new GradientNode( dayColor, 0.5f );
		_nodes[4] = new GradientNode( dayColor, 0.7f );
		_nodes[5] = new GradientNode( duskColor, 0.85f );
		_nodes[6] = new GradientNode( nightColor, 1f );
	}

	public Color Evaluate( float fraction )
	{
		for ( var i = 0; i < _nodes.Length; i++ )
		{
			var node = _nodes[i];
			var nextIndex = i + 1;

			if ( _nodes.Length < nextIndex )
				nextIndex = 0;

			var nextNode = _nodes[nextIndex];

			if ( fraction >= node.Time && fraction <= nextNode.Time )
			{
				var duration = nextNode.Time - node.Time;
				var interpolate = 1f / duration * (fraction - node.Time);

				return Color.Lerp( node.Color, nextNode.Color, interpolate );
			}
		}

		return _nodes[0].Color;
	}
}

/// <summary>
/// A way to set the colour based on the time of day, it will smoothly blend between each colour when the time has changed. Also enables the day night cycle using a "light_environment"
/// </summary>
[HammerEntity]
[Library( "spire_daynight_controller" )]
[Title( "Time of Day Controller" ), Category( "Spire" )]
public partial class DayNightController : ModelEntity
{
	[Property, Category( "Lighting & Fog" ), ResourceType( "sptime" )]
	public string DawnData { get; set; }
	public TimeStageResource Dawn { get; set; }

	[Property, Category( "Lighting & Fog" ), ResourceType( "sptime" )]
	public string DayData { get; set; }
	public TimeStageResource Day { get; set; }

	[Property, Category( "Lighting & Fog" ), ResourceType( "sptime" )]
	public string DuskData { get; set; }
	public TimeStageResource Dusk { get; set; }

	[Property, Category( "Lighting & Fog" ), ResourceType( "sptime" )]
	public string NightData { get; set; }
	public TimeStageResource Night { get; set; }

	[ConVar.Replicated( "spire_daynight_debug" )]
	public static bool DayNightDebug { get; set; } = false;

	protected Output OnBecomeNight { get; set; }
	protected Output OnBecomeDusk { get; set; }
	protected Output OnBecomeDawn { get; set; }
	protected Output OnBecomeDay { get; set; }

	public EnvironmentLightEntity Environment
	{
		get
		{
			if ( _environment == null )
				_environment = All.OfType<EnvironmentLightEntity>().FirstOrDefault();
			return _environment;
		}
	}

	public GradientFogEntity GradientFog
	{
		get
		{
			if ( _gradientFog == null )
				_gradientFog = All.OfType<GradientFogEntity>().FirstOrDefault();
			return _gradientFog;
		}
	}

	private EnvironmentLightEntity _environment;
	private GradientFogEntity _gradientFog;
	private DayNightGradient _skyColorGradient;
	private DayNightGradient _colorGradient;

	protected bool ResourcesNotFound => Dawn is null || Day is null || Dusk is null || Night is null;

	public override void Spawn()
	{
		base.Spawn();

		Dawn = TimeStageResource.Get( DawnData );
		Day = TimeStageResource.Get( DayData );
		Dusk = TimeStageResource.Get( DuskData );
		Night = TimeStageResource.Get( NightData );

		if ( ResourcesNotFound )
		{
			Log.Warning( "DayNightController is not set up correctly" );
			return;
		}

		_colorGradient = new DayNightGradient( Dawn.LightColor, Day.LightColor, Dusk.LightColor, Night.LightColor );
		_skyColorGradient = new DayNightGradient( Dawn.SkyColor, Day.SkyColor, Dusk.SkyColor, Night.SkyColor );

		DayNightSystem.Instance.OnTimeStageChanged += OnTimeStageChanged;

		Transmit = TransmitType.Always;
	}

	private void OnTimeStageChanged( TimeStage stage )
	{
		if ( stage == TimeStage.Dawn )
			OnBecomeDawn.Fire( this );
		else if ( stage == TimeStage.Day )
			OnBecomeDay.Fire( this );
		else if ( stage == TimeStage.Dusk )
			OnBecomeDusk.Fire( this );
		else if ( stage == TimeStage.Night )
			OnBecomeNight.Fire( this );

		if ( GradientFog.IsValid() )
			UpdateFogState( stage );
	}

	[Net]
	private TimeStageResource CurrentFogState { get; set; }

	private TimeStageResource UpdateFogState( TimeStage stage )
	{
		CurrentFogState = stage switch
		{
			TimeStage.Dawn => Dawn,
			TimeStage.Day => Day,
			TimeStage.Dusk => Dusk,
			TimeStage.Night => Night,
			_ => Dawn
		};

		return CurrentFogState;
	}

	[Event.Tick]
	private void Tick()
	{
		if ( ResourcesNotFound )
			return;

		var lerpSpeed = Time.Delta * 0.5f;

		if ( Host.IsServer )
		{
			var environment = Environment;
			if ( !environment.IsValid() ) return;

			var sunAngle = DayNightSystem.Instance.TimeOfDay / 24f * 360f;
			var radius = 10000f;

			environment.Color = _colorGradient.Evaluate( 1f / 24f * DayNightSystem.Instance.TimeOfDay );
			environment.SkyColor = _skyColorGradient.Evaluate( 1f / 24f * DayNightSystem.Instance.TimeOfDay );

			environment.Position = Vector3.Zero + Rotation.From( 0, 0, sunAngle + 60f ) * (radius * Vector3.Right);
			environment.Position += Rotation.From( 0, sunAngle, 0 ) * (radius * Vector3.Forward);

			var direction = (Vector3.Zero - environment.Position).Normal;
			environment.Rotation = Rotation.LookAt( direction, Vector3.Up );

			var fog = GradientFog;
			if ( !fog.IsValid() ) return;

			fog.FogStartDistance = fog.FogStartDistance.LerpTo( CurrentFogState.FogStartDistance, lerpSpeed );
			fog.FogEndDistance = fog.FogEndDistance.LerpTo( CurrentFogState.FogEndDistance, lerpSpeed );
			fog.FogColor = Color.Lerp( fog.FogColor, CurrentFogState.FogColor, lerpSpeed );
			fog.FogStartHeight = fog.FogStartHeight.LerpTo( CurrentFogState.FogStartHeight, lerpSpeed );
			fog.FogEndHeight = fog.FogEndHeight.LerpTo( CurrentFogState.FogEndHeight, lerpSpeed );

			fog.UpdateFogState( true );

			if ( DayNightDebug )
			{
				DebugOverlay.ScreenText(
					 $"TimeOfDay: {DayNightSystem.Instance.TimeOfDay}\n" +
					 $"FogStartDistance: {fog.FogStartDistance}\n" +
					 $"FogEndDistance: {fog.FogEndDistance}\n" +
					 $"FogColor: {fog.FogColor}\n" +
					 $"AmbientLight: {Map.Scene.AmbientLightColor}\n" +
					 $"FogStartHeight: {fog.FogStartHeight}\n" +
					 $"FogEndHeight: {fog.FogEndHeight}\n"
				);
			}
		}

		Map.Scene.AmbientLightColor = Color.Lerp( Map.Scene.AmbientLightColor, CurrentFogState.AmbientLightColor, lerpSpeed );
	}
}
