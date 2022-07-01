namespace Spire;

public static class LoggerExtension
{
	public static void State( this Logger log, object obj )
	{
		log.Info( $"[{(Host.IsClient ? "Client" : "Server")}] {obj}" );
	}
}
