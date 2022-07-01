using Spire.AI.Navigation;

namespace Spire.AI;

public partial class AIController : BaseNetworkable
{
	[Net, Predicted]
	public AnimatedEntity Agent { get; protected set; }

	[Net, Predicted]
	public Navigator Navigator { get; protected set; }

	public NavAgentHull AgentHull { get; protected set; }

	public AIController()
	{
	}

	public void SetAgent( AnimatedEntity agent )
	{
		Agent = agent;
	}

	public virtual void Tick()
	{
		Navigator?.Tick( Agent.Position );

		MoveAgent();
		AnimateAgent();
	}

	protected float AgentMaxSpeed = 100f;
	protected float AgentMaxStandableAngle = 50f;
	protected float AgentHeight = 64f;
	protected float AgentRadius = 4f;
	protected float AgentStepSize = 30f;
	protected float AgentGravity = 900f;

	protected Vector3 InputVelocity = Vector3.Zero;
	protected Vector3 LookDirection = Vector3.Zero;

	protected virtual void MoveAgent()
	{
		InputVelocity = Vector3.Zero;

		var timeDelta = Time.Delta;

		if ( !Navigator.Output.Finished )
		{
			InputVelocity = Navigator.Output.Direction.Normal;
			// @TODO: remove magic
			Agent.Velocity = Agent.Velocity.AddClamped( InputVelocity * timeDelta * 500f, AgentMaxSpeed );
		}

		var bbox = BBox.FromHeightAndRadius( AgentHeight, AgentRadius );

		MoveHelper move = new( Agent.Position, Agent.Velocity );
		move.MaxStandableAngle = AgentMaxStandableAngle;
		move.Trace = move.Trace.Ignore( Agent ).Size( bbox );

		if ( !Agent.Velocity.IsNearlyZero( 0.001f ) )
		{
			move.TryUnstuck();
			move.TryMoveWithStep( timeDelta, AgentStepSize );
		}

		var tr = move.TraceDirection( Vector3.Down * 10.0f );

		if ( move.IsFloor( tr ) )
		{
			Agent.GroundEntity = tr.Entity;

			if ( !tr.StartedSolid )
				move.Position = tr.EndPosition;

			if ( InputVelocity.Length > 0 )
			{
				var movement = move.Velocity.Dot( InputVelocity.Normal );
				move.Velocity = move.Velocity - movement * InputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * InputVelocity.Normal;
			}
			else
			{
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
			}
		}
		else
		{
			Agent.GroundEntity = null;
			move.Velocity += Vector3.Down * AgentGravity * timeDelta;
		}

		Agent.Position = move.Position;
		Agent.Velocity = move.Velocity;
	}

	protected virtual void AnimateAgent()
	{
		var walkVelocity = Agent.Velocity.WithZ( 0 );

		if ( walkVelocity.Length > 0.5f )
		{
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 100, true );
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );
			Agent.Rotation = Rotation.Lerp( Agent.Rotation, targetRotation, turnSpeed * Time.Delta * 20.0f );
		}

		var animHelper = new CitizenAnimationHelper( Agent );

		LookDirection = Vector3.Lerp( LookDirection, InputVelocity.WithZ( 0 ) * 1000, Time.Delta * 100.0f );
		animHelper.WithLookAt( Agent.EyePosition + LookDirection );
		animHelper.WithVelocity( Agent.Velocity );
		animHelper.WithWishVelocity( InputVelocity );
	}
}
