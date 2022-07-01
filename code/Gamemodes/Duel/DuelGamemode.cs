using SandboxEditor;

namespace Spire.Gamemodes.Duel;

[HammerEntity]
[Title( "Duel" ), Category( "Spire - Game Modes" ), Icon( "sports_kabaddi" )]
public partial class DuelGamemode : BaseGamemode
{
	public override Panel GetGamemodePanel() => new DuelHudPanel();

	[ConVar.Server( "spire_duel_minplayers" )]
	public static int MinPlayers { get; set; } = 2;

	[ConVar.Server( "spire_duel_round_start_countdown" )]
	public static int RoundStartCountdownTime = 5;

	[ConVar.Server( "spire_duel_round_length" )]
	public static int RoundLength = 120;

	[Net]
	public DuelGameState CurrentState { get; set; } = DuelGameState.WaitingForPlayers;

	[Net]
	public IList<int> TeamScores { get; set; }

	public override BasePawn GetPawn( Client cl )
	{
		if ( CurrentState == DuelGameState.RoundActive )
			return new DuelSpectatorPawn();

		return new PlayerCharacter( cl );
	}

	public int GetTeamScore( DuelTeam team )
	{
		return TeamScores[(int)team];
	}

	public void SetupScores()
	{
		TeamScores = null;

		foreach ( var team in Enum.GetValues( typeof( DuelTeam ) ) )
			TeamScores.Add( 0 );
	}

	public void IncrementScore( DuelTeam team )
	{
		TeamScores[(int)team]++;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetupScores();
	}

	[Net, Predicted]
	public TimeUntil TimeUntilRoundStart { get; set; }
	[Net, Predicted]
	public TimeUntil TimeUntilRoundEnd { get; set; }

	[Net, Predicted]
	public TimeUntil TimeUntilRoundRestart { get; set; }

	protected void AddToSuitableTeam( Client cl )
	{
		var lowest = DuelTeamHelpers.GetLowestTeam();
		cl.SetTeam( lowest );

		if ( PlayerCount >= MinPlayers && CurrentState == DuelGameState.WaitingForPlayers )
		{
			TryStartCountdown();
		}
	}

	protected bool AreTeamsBalanced()
	{
		int blueCount = DuelTeam.Blue.Count();
		int redCount = DuelTeam.Red.Count();

		return Math.Abs( blueCount - redCount ) <= 1;
	}

	protected void ShuffleTeamMembers()
	{
		var playersPerTeam = ( PlayerCount / 2f ).FloorToInt();
		int selectionIndex = 0;
		foreach( var client in Client.All )
		{
			client.SetTeam( selectionIndex < playersPerTeam ? DuelTeam.Blue : DuelTeam.Red );

			selectionIndex++;
		}

		ChatPanel.Announce( "The teams were shuffled.", ChatCategory.System );
	}

	public override void OnClientJoined( Client cl )
	{
		base.OnClientJoined( cl );

		AddToSuitableTeam( cl );
	}

	protected void ResetGameMode()
	{
		ChatPanel.Announce( $"Waiting for players since there are not enough people on.", ChatCategory.System );

		SetupScores();
		CurrentState = DuelGameState.WaitingForPlayers;
	}

	protected void TryStartCountdown()
	{
		CleanupMap();

		if ( !AreTeamsBalanced() )
			ShuffleTeamMembers();
		else
			Game.Current?.RespawnEveryone();

		ChatPanel.Announce( $"The round will start in {RoundStartCountdownTime} seconds.", ChatCategory.System );

		CurrentState = DuelGameState.RoundCountdown;
		TimeUntilRoundStart = RoundStartCountdownTime;
	}

	protected void BeginRound()
	{
		CleanupMap();

		Game.Current?.RespawnDeadPlayers();

		ChatPanel.Announce( $"The fight begins.", ChatCategory.System );

		CurrentState = DuelGameState.RoundActive;
		TimeUntilRoundEnd = RoundLength;

		PlaySound( "duel.round_begin" );
	}

	protected void DecideRoundWinner()
	{
		var teamOneCount = DuelTeam.Blue.GetAliveMembers().Count();
		var teamTwoCount = DuelTeam.Red.GetAliveMembers().Count();

		if ( teamOneCount > teamTwoCount )
		{
			IncrementScore( DuelTeam.Blue );
			ChatPanel.Announce( $"{DuelTeam.Blue.GetName()} wins the round!", ChatCategory.System );
		}
		else if ( teamTwoCount > teamOneCount )
		{
			IncrementScore( DuelTeam.Red );
			ChatPanel.Announce( $"{DuelTeam.Red.GetName()} wins the round!", ChatCategory.System );
		}
		else
		{
			ChatPanel.Announce( $"Stalemate!", ChatCategory.System );
		}

		CurrentState = DuelGameState.RoundWinnerDecided;
		TimeUntilRoundRestart = RoundStartCountdownTime;
	}

	public string GetGameStateText()
	{
		return CurrentState switch
		{
			DuelGameState.WaitingForPlayers => "Waiting",
			DuelGameState.RoundCountdown => TimeSpan.FromSeconds( TimeUntilRoundStart ).ToString( @"mm\:ss" ),
			DuelGameState.RoundActive => TimeSpan.FromSeconds( TimeUntilRoundEnd ).ToString( @"mm\:ss" ),
			DuelGameState.RoundWinnerDecided => "Round Over",
			DuelGameState.GameWinnerDecided => "Game Over",
			_ => ""
		};
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( CurrentState == DuelGameState.RoundCountdown )
		{
			if ( TimeUntilRoundStart <= 0 )
			{
				BeginRound();
			}
		}
		//
		else if ( CurrentState == DuelGameState.RoundActive )
		{
			if ( TimeUntilRoundEnd <= 0 )
			{
				DecideRoundWinner();
			}
		}
		//
		else if ( CurrentState == DuelGameState.RoundWinnerDecided )
		{
			if ( TimeUntilRoundRestart <= 0 )
			{
				if ( PlayerCount >= MinPlayers )
				{
					TryStartCountdown();
				}
				else
				{
					ResetGameMode();
				}
			}
		}
	}

	protected async Task BecomeSpectator( Client cl )
	{
		await GameTask.DelaySeconds( 3 );

		cl.Components.RemoveAny<DeathCameraMode>();

		if ( AllowRespawning() )
		{
			Game.Current?.RespawnPlayer( cl );
			return;
		}

		var pawn = new DuelSpectatorPawn();
		cl.Pawn = pawn;
		pawn.Respawn();
	}

	public override void OnCharacterKilled( BaseCharacter character, DamageInfo damageInfo )
	{
		base.OnCharacterKilled( character, damageInfo );

		var cl = character.Client;
		if ( cl.IsValid() )
		{
			var deathCam = new DeathCameraMode();
			deathCam.FocusPoint = character.Position;

			cl.Pawn?.Delete();

			cl.Components.Add( deathCam );

			_ = BecomeSpectator( cl );
		}

		var blueCount = DuelTeam.Blue.AliveCount();
		var redCount = DuelTeam.Red.AliveCount();

		if ( CurrentState == DuelGameState.RoundActive )
		{
			bool anyIsZero = blueCount == 0 || redCount == 0;

			if ( !anyIsZero )
				return;

			DecideRoundWinner();
		}
	}

	protected bool IsSpawnPointOccupied( SpawnPoint spawnPoint )
	{
		const float occupiedRadius = 30f;

		var sphere = Entity.FindInSphere( spawnPoint.Position, occupiedRadius );

		foreach ( var entity in sphere )
		{
			if ( entity is BaseCharacter )
				return true;
		}

		return false;
	}

	public override Transform? GetSpawn( BaseCharacter character )
	{
		var teamName = character.Client.GetTeam()
			.ToString()
			.ToLower();

		var allSpawns = All.OfType<SpawnPoint>()
			.Where( x => x.Tags.Has( teamName ) )
			.OrderBy( x => Guid.NewGuid() );

		int attempts = 0;
		const int maxAttempts = 10;

		while ( attempts < maxAttempts )
		{
			attempts++;

			var spawnPoint = allSpawns.FirstOrDefault( x => !IsSpawnPointOccupied( x ) );
			if ( spawnPoint.IsValid() )
				return spawnPoint.Transform;
		}

		return allSpawns.FirstOrDefault()?.Transform ?? Transform.Zero;
	}

	public override bool AllowMovement()
	{
		return CurrentState != DuelGameState.RoundCountdown;
	}

	public override bool AllowRespawning()
	{
		return CurrentState != DuelGameState.RoundActive && CurrentState != DuelGameState.RoundWinnerDecided;
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( !AllowMovement() )
		{
			input.Clear();
			input.StopProcessing = true;
		}
	}
}
