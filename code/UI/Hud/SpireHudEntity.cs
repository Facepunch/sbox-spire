using Spire.Gamemodes;

namespace Spire;

public partial class SpireHudEntity : HudEntity<SpireHud>
{
	public static Panel CurrentHudPanel { get; protected set; }

	[Event.Tick.Client]
	protected void DoTick()
	{
		if ( !BaseGamemode.Current.IsValid() ) return;
		if ( CurrentHudPanel is not null ) return;

		var gamemode = BaseGamemode.Current;
		CurrentHudPanel = gamemode.GetGamemodePanel();

		if ( CurrentHudPanel is not null )
			CurrentHudPanel.Parent = this.RootPanel;
	}
}
