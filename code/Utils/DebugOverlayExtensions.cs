namespace Spire.ExtensionMethods;

public static class DebugOverlayExtensions
{
	public static void TraceResultWithRealm( this Sandbox.Internal.Globals.DebugOverlay _, TraceResult tr )
	{
		var color = (Host.IsServer ? Color.Blue : Color.Yellow).WithAlpha( 0.5f );

		DebugOverlay.Line( tr.StartPosition, tr.StartPosition + tr.Direction * tr.Distance, color, 1f );
		DebugOverlay.Sphere( tr.EndPosition, 8f, color, 1f );

		if ( tr.Hit )
			DebugOverlay.Text( $"Hit: {tr.Entity}", tr.EndPosition, color, 1f );
	}
}
