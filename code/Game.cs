global using Sandbox;
global using Sandbox.UI;
global using System;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Collections.Generic;

// Spire
global using Spire.ExtensionMethods;
global using Spire.UI;

using Spire.Gamemodes;
using static Sandbox.Internal.MapAccessor;

namespace Spire;

public partial class Game : Sandbox.Game
{
	public static new Game Current => Sandbox.Game.Current as Game;

	public DayNight.DayNightSystem DayNightSystem { get; protected set; }

	public SpireHudEntity Hud { get; set; }

	StandardPostProcess _PostProcess;

	public Game()
	{
		if ( Host.IsClient )
		{
			_PostProcess = new StandardPostProcess();
			PostProcess.Add( _PostProcess );
		}
		else
		{
			DayNightSystem = new();

			Hud = new();
		}

		Global.TickRate = 30;
	}

	public void RespawnPlayer( Client cl )
	{
		var gamemode = BaseGamemode.Current;
		if ( gamemode.IsValid() )
		{
			gamemode.CreatePawn( cl );
		}
		else
		{
			cl.Pawn?.Delete();

			var pawn = new PlayerCharacter( cl );
			cl.Pawn = pawn;
			pawn.Respawn();
		}
	}

	public override void ClientJoined( Client cl )
	{
		Log.Info( $"{cl.Name} has joined the world" );

		RespawnPlayer( cl );

		BaseGamemode.Current?.OnClientJoined( cl );
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		BaseGamemode.Current?.OnClientLeft( cl, reason );
	}

	[ConCmd.Server( "spire_respawn" )]
	public static void ForceRespawn()
	{
		var cl = ConsoleSystem.Caller;

		Current.RespawnPlayer( cl );
	}

	public void RespawnDeadPlayers()
	{
		if ( !Host.IsServer )
			return;

		Client.All.Where( x => x.Pawn is not BaseCharacter )
			.ToList()
			.ForEach( x => Game.Current.RespawnPlayer( x ) );
	}

	public void RespawnEveryone()
	{
		Client.All.ToList().ForEach( x => Game.Current.RespawnPlayer( x ) );
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		// Simulate active gamemode
		BaseGamemode.Current?.Simulate( cl );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		// Simulate active gamemode
		BaseGamemode.Current?.FrameSimulate( cl );

		PostProcessTick();
	}	

	protected void PostProcessTick()
	{
		_PostProcess.Vignette.Enabled = true;
		_PostProcess.Vignette.Intensity = 1.0f;
		_PostProcess.Vignette.Roundness = 1.5f;
		_PostProcess.Vignette.Smoothness = 0.5f;
		_PostProcess.Vignette.Color = Color.Black;

		_PostProcess.FilmGrain.Enabled = true;
		_PostProcess.FilmGrain.Intensity = 0f;
		_PostProcess.FilmGrain.Response = 1;

		if ( Local.Pawn is BaseCharacter localPlayer )
		{
			var timeSinceDamage = localPlayer.TimeSinceDamage.Relative;
			var damageUi = timeSinceDamage.LerpInverse( 0.25f, 0.0f, true ) * 0.3f;
			if ( damageUi > 0 )
			{
				_PostProcess.Saturate.Amount -= damageUi;
				_PostProcess.Vignette.Color = Color.Lerp( _PostProcess.Vignette.Color, new Color( 1f, 0.5f, 0.5f ), damageUi );
				_PostProcess.Vignette.Intensity += damageUi;
				_PostProcess.Vignette.Smoothness += damageUi;
				_PostProcess.Vignette.Roundness += damageUi;

				_PostProcess.Blur.Enabled = true;
				_PostProcess.Blur.Strength = damageUi * 1f;
			}
			else
			{
				_PostProcess.Blur.Enabled = false;
			}
		}
		else
		{
			_PostProcess.Blur.Enabled = false;
		}

		BaseGamemode.Current?.PostProcessTick( _PostProcess );
	}
}
