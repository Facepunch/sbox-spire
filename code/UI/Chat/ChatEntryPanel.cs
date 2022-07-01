using Sandbox.UI.Construct;

namespace Spire.UI;

public partial class ChatEntryPanel : Panel
{
	public Label MessageLabel { get; set; }
	public Label CategoryLabel { get; set; }

	public RealTimeSince TimeSinceCreated { get; init; } = 0;

	public static string GetCategoryName( ChatCategory category )
	{
		return category switch
		{
			ChatCategory.Chat => "Local",
			ChatCategory.System => "System",
			_ => "Server"
		};
	}

	public static Color GetCategoryColor( ChatCategory category )
	{
		return category switch
		{
			ChatCategory.Chat => Color.Green,
			ChatCategory.System => Color.Cyan,
			_ => Color.Red
		};
	}

	public void SetMessage( string message, ChatCategory category )
	{
		CategoryLabel = Add.Label( GetCategoryName( category ), "category" );
		CategoryLabel.Style.FontColor = GetCategoryColor( category );

		MessageLabel = Add.Label( message, "message" );
	}

	public override void Tick()
	{
		base.Tick();

		if ( TimeSinceCreated > 10f )
			Delete();
	}
}
