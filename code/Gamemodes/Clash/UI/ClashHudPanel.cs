namespace Spire.Gamemodes.Clash;

partial class ScoreAvatar : Panel
{
	public Image Avatar { get; set; }
	public Label Label { get; set; }

	public ScoreAvatar() : base()
	{ 
	}

	public ScoreAvatar( Client cl, int score = 0, int position = 0 )
	{
		Avatar = AddChild<Image>( "avatar" );
		Avatar.SetTexture( $"avatar:{cl.PlayerId}" );

		Label = AddChild<Label>( "score" );

		if ( score > 0 )
		Label.Text = $"{score}";

		SetClass( "dead", cl.Pawn is not PlayerCharacter );
		
		ApplyPosition( position );
	}

	void ApplyPosition( int position )
	{
		Avatar.SetClass( "gold", position == 0 );
		Avatar.SetClass( "silver", position == 1 );
		Avatar.SetClass( "bronze", position == 2 );
	}
}

[UseTemplate]
public partial class ClashHudPanel : Panel
{
	public ClashGamemode Gamemode => BaseGamemode.Current as ClashGamemode;

	// @ref
	public Panel TeamMembers { get; set; }

	public string GameState => $"{Gamemode?.GetGameStateText()}";

	private TimeSince LastUpdatedAvatars = 1f;
	private float AvatarUpdateRate = 1f;

	public override void Tick()
	{
		base.Tick();

		UpdateAvatars();
	}

	public void UpdateAvatars()
	{
		if ( LastUpdatedAvatars < AvatarUpdateRate )
			return;

		TeamMembers.DeleteChildren( true );
		LastUpdatedAvatars = 0;
		
		var game = BaseGamemode.Current as ClashGamemode;
		var sortedScores = game.Scores.OrderByDescending( kvp => kvp.Value ).ToList();

		int position = 0;
		foreach ( var kvp in sortedScores )
		{
			var client = kvp.Key;
			var score = kvp.Value;
			var avatar = new ScoreAvatar( client, score, position );

			TeamMembers.AddChild( avatar );

			position++;
		}
	}
}
