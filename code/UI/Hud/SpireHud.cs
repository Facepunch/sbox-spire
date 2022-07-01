namespace Spire.UI;

[UseTemplate]
public partial class SpireHud : RootPanel
{
	[Event.BuildInput]
	protected void BuildInput( InputBuilder input )
	{
		var devCam = Local.Client.Components.Get<DevCamera>();

		// @TODO: handle this logic elsewhere 
		SetClass( "camera-movement", Input.UsingController || input.Down( InputButton.SecondaryAttack ) || devCam is not null );

		SetClass( "invisible", devCam is not null );
	}
}
