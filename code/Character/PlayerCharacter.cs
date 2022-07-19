using Spire.Abilities;
using Spire.Gamemodes;
using Spire.Gamemodes.Duel;
using Spire.UI;

namespace Spire;

public partial class PlayerCharacter : BaseCharacter
{
	public override string ToString()
	{
		return $"Character ({Client?.Name})";
	}

	[Net]
	public TimeSince TimeSinceDied { get; set; }

	[Net]
	public PawnController DevController { get; set; }

	[Net, Predicted]
	public PlayerHotbar Hotbar { get; set; }

	public override PawnController ActiveController => DevController ?? base.ActiveController;

	public Particles UnitCircle { get; set; }

	public PlayerCharacter()
	{
		if ( Host.IsClient )
			Nameplate = new PlayerNameplate( this );
	}

	[Net] 
	public string ClothingString { get; set; }

	public PlayerCharacter( Client cl ) : this()
	{
		// Load clothing from client data
		Clothing.LoadFromClient( cl );
		ClothingString = cl.GetClientData( "avatar" );
	}

	public override void Respawn()
	{
		base.Respawn();

		var camera = new PlayerCamera();
		CameraMode = camera;

		Controller = new CharacterController();
		Hotbar = new PlayerHotbar( this );

		Hotbar.Add( new SwordWeapon() );
		Hotbar.Add( new CrossbowWeapon(), false );
		Hotbar.Add( new MagicStaffWeapon(), false );

		// @TODO: Improve this. This is shit
		FirstAbility = new SelfHealAbility();
		FirstAbility.Entity = this;

		SecondAbility = new WorldPointTestAbility();
		SecondAbility.Entity = this;

		UltimateAbility = new BombThrowAbility();
		UltimateAbility.Entity = this;

		MovementAbility = new SpeedWalkAbility();
		MovementAbility.Entity = this;
	}

	public override void OnNewModel( Model model )
	{
		base.OnNewModel( model );

		if ( UnitCircle is not null )
			return;

		if ( IsLocalPawn )
			UnitCircle = Particles.Create( "particles/widgets/circle/player_circle_ground.vpcf", this, true );
		else
			UnitCircle = Particles.Create( "particles/widgets/circle/unit_circle_ground.vpcf", this, true );
	}

	public override void BuildInput( InputBuilder input )
	{
		BaseGamemode.Current?.BuildInput( input );

		if ( input.StopProcessing ) return;

		BuildInputAbilities( input );

		if ( input.StopProcessing ) return;

		ActiveChild?.BuildInput( input );

		if ( input.StopProcessing ) return;

		Controller?.BuildInput( input );

		if ( input.StopProcessing ) return;

		Animator?.BuildInput( input );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		TimeSinceDied = 0;
	}

	protected override void OnDestroy()
	{
		InteractingAbility = null;
		RpcKillInteractions( To.Single( Client ) );

		base.OnDestroy();
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		SimulateAbilities( cl );
		Hotbar?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		SimulateAbilities( cl );
	}

	[Event.Tick.Client]
	protected void UpdateColors()
	{
		if ( !IsLocalPawn )
			UnitCircle.SetPosition( 4, new Vector3( Client.GetTeam().GetColor() ) );
	}
}
