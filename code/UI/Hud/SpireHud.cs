namespace Spire.UI;

[UseTemplate]
public partial class SpireHud : RootPanel
{
	public Panel CursorPanel { get; set; }

	[Event.BuildInput]
	protected void BuildInput()
	{
		var devCam = Local.Client.Components.Get<DevCamera>();

		bool disableMouseInput = Input.UsingController || Input.Down( InputButton.SecondaryAttack ) || devCam is not null;
		CursorPanel.SetClass( "enabled", !disableMouseInput );

		SetClass( "invisible", devCam is not null );
	}
}
