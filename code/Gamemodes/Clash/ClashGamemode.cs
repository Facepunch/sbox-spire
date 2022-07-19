using SandboxEditor;

namespace Spire.Gamemodes.Clash;

[HammerEntity]
[Title( "Clash" ), Category( "Spire - Game Modes" ), Icon( "sports_kabaddi" )]
public partial class ClashGamemode : BaseGamemode
{
	public override Panel GetGamemodePanel() => new ClashHudPanel();

	[ConVar.Server( "spire_clash_minplayers" )]
	public static int MinPlayers { get; set; } = 2;

	[ConVar.Server( "spire_clash_round_start_countdown" )]
	public static int RoundStartCountdownTime = 5;

	[ConVar.Server( "spire_clash_round_length" )]
	public static int RoundLength = 120;

	public float RespawnTime => 5f;

	[Net]
	public ClashGameState CurrentState { get; set; } = ClashGameState.WaitingForPlayers;

	public override BasePawn GetPawn( Client cl )
	{
		return new PlayerCharacter( cl );
	}

	public override void Spawn()
	{
		base.Spawn();
	}

	[Net]
	public Dictionary<Client, int> Scores { get; set; }

	public void IncrementScore( Client cl )
	{
		if ( CurrentState != ClashGameState.RoundActive )
			return;

		Scores[cl]++;
	}

	public int GetScore( Client cl )
	{
		if ( Scores.TryGetValue( cl, out int score ) )
		{
			return score;
		}

		return 0;
	}

	public void ResetScores()
	{
		// Nulling out a [Net] variable will reset it.
		Scores.Clear();

		foreach ( var cl in Client.All )
		{
			Scores.Add( cl, 0 );
		}
	}


	[Net, Predicted]
	public TimeUntil TimeUntilRoundStart { get; set; }
	[Net, Predicted]
	public TimeUntil TimeUntilRoundEnd { get; set; }

	[Net, Predicted]
	public TimeUntil TimeUntilRoundRestart { get; set; }

	protected void InitializeClient( Client cl )
	{
		if ( PlayerCount >= MinPlayers && CurrentState == ClashGameState.WaitingForPlayers )
		{
			TryStartCountdown();
		}

		cl.SetTeam( Team.None );

		Scores.Add( cl, 0 );
	}

	public override void OnClientJoined( Client cl )
	{
		base.OnClientJoined( cl );

		InitializeClient( cl );
	}

	protected void ResetGameMode()
	{
		ChatPanel.Announce( $"Waiting for players since there are not enough people on.", ChatCategory.System );

		CurrentState = ClashGameState.WaitingForPlayers;
	}

	protected void TryStartCountdown()
	{
		CleanupMap();

		Game.Current?.RespawnEveryone();

		ChatPanel.Announce( $"The round will start in {RoundStartCountdownTime} seconds.", ChatCategory.System );

		CurrentState = ClashGameState.RoundCountdown;
		TimeUntilRoundStart = RoundStartCountdownTime;
	}

	protected void BeginRound()
	{
		CleanupMap();
		ResetScores();

		Game.Current?.RespawnDeadPlayers();

		ChatPanel.Announce( $"The fight begins.", ChatCategory.System );

		CurrentState = ClashGameState.RoundActive;
		TimeUntilRoundEnd = RoundLength;

		ResetScores();
		PlaySound( "duel.round_begin" );
	}

	protected void DecideRoundWinner()
	{
		var sortedScores = Scores.OrderByDescending( kvp => kvp.Value ).ToList();
		var winner = sortedScores.FirstOrDefault();

		if ( !winner.Key.IsValid() )
		{
			ChatPanel.Announce( $"No one wins the round.", ChatCategory.System );
			PlaySound( "duel.round_lose" );
		}
		else
		{
			ChatPanel.Announce( $"{winner.Key.Name} wins the round!", ChatCategory.System );
			PlaySound( "duel.round_win" );
		}

		CurrentState = ClashGameState.RoundWinnerDecided;
	}

	public string GetGameStateText()
	{
		return CurrentState switch
		{
			ClashGameState.WaitingForPlayers => "Waiting",
			ClashGameState.RoundCountdown => TimeSpan.FromSeconds( TimeUntilRoundStart ).ToString( @"mm\:ss" ),
			ClashGameState.RoundActive => TimeSpan.FromSeconds( TimeUntilRoundEnd ).ToString( @"mm\:ss" ),
			_ => "-"
		};
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( CurrentState == ClashGameState.RoundCountdown )
		{
			if ( TimeUntilRoundStart <= 0 )
			{
				BeginRound();
			}
		}
		//
		else if ( CurrentState == ClashGameState.RoundActive )
		{
			if ( TimeUntilRoundEnd <= 0 )
			{
				DecideRoundWinner();
			}
		}
		//
		else if ( CurrentState == ClashGameState.RoundWinnerDecided )
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
		await GameTask.DelaySeconds( RespawnTime );

		cl.Components.RemoveAny<DeathCameraMode>();

		if ( AllowRespawning() )
		{
			Game.Current?.RespawnPlayer( cl );
			return;
		}

		var pawn = new SpectatorPawn();
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

			if ( damageInfo.Attacker is PlayerCharacter attacker )
			{
				IncrementScore( attacker.Client );
			}
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
		return CurrentState != ClashGameState.RoundCountdown;
	}

	public override bool AllowRespawning()
	{
		return true;
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
