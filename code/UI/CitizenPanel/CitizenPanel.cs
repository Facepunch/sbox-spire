namespace Spire.UI;

public class CitizenPanelScene : ScenePanel
{
	static Vector3 EyePosition = new( 0, 0, 64 );
	static Color LightColor = new Color( 1f, 0.85f, 0.71f ) * 30f;

	private SceneModel PlayerModel { get; set; }
	private ClothingContainer Clothing { get; set; }
	public Client Client { get; set; }
	private bool NeedsUpdating { get; set; } = true;

	public CitizenPanelScene()
	{
		AddClass( "avatar" );
	}


	[Event.Hotload]
	private void Update()
	{
		World?.Delete();
		World = new();

		RenderOnce = true;
		NeedsUpdating = false;

		// Create a light
		_ = new SceneLight( World, Vector3.Up * 128 + Vector3.Forward * 60, 386, LightColor );

		PlayerModel = new SceneModel( World, "models/citizen/citizen.vmdl", Transform.Zero );
		PlayerModel.Update( RealTime.Delta );

		SetupClothing();

		// Set up camera shit
		CameraPosition = EyePosition + Vector3.Forward * 64;
		CameraRotation = Rotation.LookAt( (EyePosition - CameraPosition).Normal, Vector3.Up );
		FieldOfView = 20f;
		ZNear = 0.1f;
		ZFar = 512;
	}

	private void SetupClothing()
	{
		var player = Client.Pawn as PlayerCharacter;

		Clothing = new ClothingContainer();
		Clothing.Deserialize( player.ClothingString );

		var models = Clothing.DressSceneObject( PlayerModel );
		var delta = RealTime.Delta;

		models.ForEach( x => x.Update( delta ) );
	}

	public override void Tick()
	{
		base.Tick();

		if ( NeedsUpdating )
		{
			Update();
		}
	}
}

/// <summary>
/// Panel that displays a player, preferably a Citizen
/// </summary>
public class CitizenPanel : Panel
{
	public CitizenPanelScene Scene { get; set; }

	public CitizenPanel( Client cl )
	{
		StyleSheet.Load( "/UI/CitizenPanel/CitizenPanel.scss" );
		SetClass( "avatar", true );
		
		Scene = AddChild<CitizenPanelScene>();
		Scene.Client = cl;
	}
}
