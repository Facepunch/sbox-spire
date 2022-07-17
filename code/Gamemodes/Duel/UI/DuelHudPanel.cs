namespace Spire.Gamemodes.Duel;

[UseTemplate]
public partial class DuelHudPanel : Panel
{
	public DuelGamemode Gamemode => BaseGamemode.Current as DuelGamemode;

	// @ref
	public Panel BlueTeamMembers { get; set; }
	// @ref
	public Panel RedTeamMembers { get; set; }

	public string GameState => $"{Gamemode?.GetGameStateText()}";
	public string RedScore => $"{Gamemode?.GetTeamScore( Team.Red )}";
	public string BlueScore => $"{Gamemode?.GetTeamScore( Team.Blue )}";

	private TimeSince LastUpdatedAvatars = 1f;
	private float AvatarUpdateRate = 1f;

	public override void Tick()
	{
		base.Tick();

		UpdateAvatars();
	}

	public Panel GetTeamPanel( Team team )
	{
		if ( team == Team.Blue )
			return BlueTeamMembers;
		else
			return RedTeamMembers;
	}

	public void UpdateAvatars()
	{
		if ( LastUpdatedAvatars < AvatarUpdateRate )
			return;

		BlueTeamMembers.DeleteChildren( true );
		RedTeamMembers.DeleteChildren( true );

		LastUpdatedAvatars = 0;

		foreach ( var client in Client.All )
		{
			var panel = GetTeamPanel( client.GetTeam() )
				.AddChild<Image>( "avatar" );

			panel.SetTexture( $"avatar:{client.PlayerId}" );
			panel.SetClass( "dead", client.Pawn is not PlayerCharacter );
		}
	}
}
