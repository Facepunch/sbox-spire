namespace Spire.AI.Navigation;

public class Path 
{
	protected Vector3 CurrentPosition;
	protected Vector3 TargetPosition;

	public NavPath NavigationPath;

	public bool IsEmpty => NavigationPath is null || NavigationPath.Count <= 1;

	public const float DestinationLeeway = 1f;

	public void Update( Vector3 from, Vector3 to, NavAgentHull hull = default )
	{
		CurrentPosition = from;

		DebugOverlay.Line( from, to );

		bool needsRebuild = true;
		if ( !TargetPosition.AlmostEqual( to, DestinationLeeway ) )
		{
			TargetPosition = to;
			needsRebuild = true;
		}

		if ( needsRebuild )
		{
			var fromFixed = NavMesh.GetClosestPoint( from );
			var toFixed = NavMesh.GetClosestPoint( to );

			NavigationPath = NavMesh.PathBuilder( fromFixed.Value )
				.WithAgentHull( hull )
				.Build( toFixed.Value );
		}

		if ( IsEmpty )
			return;

		var deltaToNext = from - NavigationPath.Segments[1].Position;
		var delta = NavigationPath.Segments[1].Position - NavigationPath.Segments[0].Position;
		var deltaNormal = delta.Normal;

		if ( deltaToNext.WithZ( 0 ).Length.AlmostEqual( 0, DestinationLeeway ) )
		{
			NavigationPath.Segments.RemoveAt( 0 );
			return;
		}

		if ( deltaToNext.Normal.Dot( deltaNormal ) >= 1.0f )
		{
			NavigationPath.Segments.RemoveAt( 0 );
		}
	}

	public float GetDistance( int point, Vector3 from )
	{
		if ( NavigationPath.Count <= point ) return float.MaxValue;

		return NavigationPath.Segments[point].Position.WithZ( from.z ).Distance( from );
	}

	public Vector3 GetDirection( Vector3 position )
	{
		if ( NavigationPath.Count == 1 )
			return (NavigationPath.Segments[0].Position - position).WithZ( 0 ).Normal;

		return (NavigationPath.Segments[1].Position - position).WithZ( 0 ).Normal;
	}

	private Color CurrentActionColor => Color.Yellow.WithAlpha( 1f );
	private Color QueuedActionColor => Color.White.WithAlpha( 0.35f );

	public void Debug()
	{
		if ( NavigationPath is null )
			return;

		var offset = Vector3.Up * 2;

		int i = 0;
		var cachedPoint = Vector3.Zero;
		foreach ( var point in NavigationPath.Segments )
		{
			var col = i <= 1 ? CurrentActionColor : QueuedActionColor;

			DebugOverlay.Circle( point.Position, Rotation.LookAt( Vector3.Up ), 4f, col, 0f );

			if ( i > 0 )
				DebugOverlay.Line( cachedPoint + offset, point.Position + offset, col );

			cachedPoint = point.Position;
			i++;
		}
	}
}
