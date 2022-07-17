namespace Spire.UI;

[UseTemplate]
public partial class ChatPanel : Panel
{
	public static ChatPanel Current;

	// @ref
	public Panel Canvas { get; set; }
	// @ref
	public TextEntry Input { get; set; }

	public ChatPanel()
	{
		Current = this;
	}

	protected override void PostTemplateApplied()
	{
		base.PostTemplateApplied();

		Input.AddEventListener( "onsubmit", () => Submit() );
		Input.AddEventListener( "onblur", () => Close() );
		Input.AcceptsFocus = true;
		Input.AllowEmojiReplace = true;

		ChatHooks.OnOpenChat += Open;
	}

	void Open()
	{
		AddClass( "open" );
		Input.Focus();
	}

	void Close()
	{
		RemoveClass( "open" );
		Input.Blur();
	}

	void Submit()
	{
		Close();

		var msg = Input.Text.Trim();
		Input.Text = "";

		if ( string.IsNullOrWhiteSpace( msg ) )
			return;

		Say( msg );
	}

	public void AddMessage( string message, ChatCategory category )
	{
		var pnl = Canvas.AddChild<ChatEntryPanel>();
		pnl.SetMessage( message, category );
	}

	public static void Announce( string message, ChatCategory category = ChatCategory.System )
	{
		if ( !Host.IsServer ) 
			return;

		AddChatEntry( To.Everyone, message, (int)category );
	}

	[ConCmd.Client( "spire_chat_system", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string message, int categoryNum )
	{
		var category = (ChatCategory)categoryNum;

		Current?.AddMessage( message, category );

		Log.Info( $"[{category}] {message}" );
	}

	[ConCmd.Server( "spire_say" )]
	public static void Say( string message )
	{
		Assert.NotNull( ConsoleSystem.Caller );

		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		AddChatEntry( To.Everyone, $"{ConsoleSystem.Caller.Name} says \"{message}\"", (int)ChatCategory.Chat );
	}
}

public static partial class ChatHooks
{
	public static event Action OnOpenChat;

	[ConCmd.Client( "openchat" )]
	internal static void MessageMode()
	{
		OnOpenChat?.Invoke();
	}
}
