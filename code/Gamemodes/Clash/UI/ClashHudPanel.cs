namespace Spire.Gamemodes.Clash;

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

		foreach ( var client in Client.All )
		{
			var panel = TeamMembers.AddChild<Image>( "avatar" );

			panel.SetTexture( $"avatar:{client.PlayerId}" );
			panel.SetClass( "dead", client.Pawn is not PlayerCharacter );
		}
	}
}
