namespace Spire.ExtensionMethods;

public static class PanelExtensions
{
	public static async Task AsyncSetTimedClass( this Panel panel, string className, float timeInSeconds )
	{
		panel.SetClass( className, true );
		await GameTask.DelaySeconds( timeInSeconds );
		panel.SetClass( className, false );
	}

	public static void SetTimedClass( this Panel panel, string className, float timeInSeconds = 0.2f )
	{
		_ = panel.AsyncSetTimedClass( className, timeInSeconds );
	}
}
