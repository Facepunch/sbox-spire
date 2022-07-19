namespace Spire.Gamemodes.Clash;

public class ScoreAvatar : Panel
{
	public CitizenPanel Avatar { get; set; }
	public Label Score { get; set; }
	public Label Name { get; set; }

	public Client Client { get; set; }

	public ScoreAvatar() : base()
	{
	}

	public ScoreAvatar( Client cl, int score = 0 )
	{
		Client = cl;

		Avatar = new CitizenPanel( cl );
		AddChild( Avatar );
		
		Score = AddChild<Label>( "score" );

		Name = AddChild<Label>( "name" );
		Name.Text = $"{cl.Name}";

		Update( score );
		SetClass( "dead", cl.Pawn is not PlayerCharacter );
	}
		
	public void Update( int score = 0 )
	{
		if ( score > 0 )
			Score.Text = $"{score}";
	}
}

[UseTemplate]
public class ClashHudPanel : Panel
{
	public ClashGamemode Gamemode => BaseGamemode.Current as ClashGamemode;

	public Dictionary<Client, ScoreAvatar> Entries = new();

	// @ref
	public Panel TeamMembers { get; set; }

	public string GameState => $"{Gamemode?.GetGameStateText()}";

	public override void Tick()
	{
		base.Tick();
		UpdateAvatars();
	}

	ScoreAvatar AddClient( Client cl, int score = 0 )
	{
		var p = new ScoreAvatar( cl, score );
		TeamMembers.AddChild( p );

		return p;
	}

	public void UpdateAvatars()
	{
		foreach ( var client in Client.All.Except( Entries.Keys ) )
		{
			var entry = AddClient( client );
			Entries[client] = entry;
		}

		foreach ( var client in Entries.Keys.Except( Client.All ) )
		{
			if ( Entries.TryGetValue( client, out var entry ) )
			{
				entry?.Delete();
				Entries.Remove( client );
			}
		}
	}

	[Event( "spire.clash.updatescore" )]
	void UpdateScores()
	{
		foreach ( var kv in Entries )
		{
			var client = kv.Key;
			var entry = kv.Value;
			var score = Gamemode?.GetScore( client ) ?? 0;
			entry.Update( score );
		}
	}
}
