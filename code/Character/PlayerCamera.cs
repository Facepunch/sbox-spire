
using System;

namespace Spire;

public partial class PlayerCamera : CameraMode
{
	protected Angles OrbitAngles;

	protected float OrbitDistance { get; set; } = 400f;
	protected float TargetOrbitDistance { get; set; } = 400f;
	protected float WheelSpeed => 10f;

	protected Range CameraDistance { get; set; } = new( 125, 500 );
	protected Range PitchClamp { get; set; } = new( 40, 60 );

	[ConVar.Client( "spire_camera_farz", Max = 80000f, Min = 1024f, Saved = true )]
	public static float ZFarPreference { get; set; } = 2048f;

	public AnimatedEntity TargetEntity { get; set; }

	protected Rotation LookAt( Vector3 targetPosition, Vector3 position )
	{
		var targetDelta = (targetPosition - position);
		var direction = targetDelta.Normal;

		return Rotation.From( new Angles(
			((float)Math.Asin( direction.z )).RadianToDegree() * -1.0f,
			((float)Math.Atan2( direction.y, direction.x )).RadianToDegree(),
			0.0f ) );
	}

	protected void UpdateWithoutTarget()
	{
		var target = Entity.All.FirstOrDefault( x => x is SpawnPoint );

		if ( target.IsValid() )
		{
			Position = target.Position + Vector3.Up * 300f + Vector3.Forward * 300f;
			Rotation = LookAt( target.Position, Position );
		}
		else
		{
			// At this point... who cares, really
		}
	}

	public override void Update()
	{
		var pawn = TargetEntity;
		if ( !pawn.IsValid() )
			TargetEntity = FindTargetEntity();

		if ( !pawn.IsValid() )
		{
			UpdateWithoutTarget();
			return;
		}

		Position = pawn.Position;
		Vector3 targetPos;

		Position += Vector3.Up * (pawn.CollisionBounds.Center.z * pawn.Scale);
		Rotation = Rotation.From( OrbitAngles );

		targetPos = Position + Rotation.Backward * OrbitDistance;

		Position = targetPos;
		FieldOfView = 70f;

		ZFar = ZFarPreference;
		Viewer = null;
	}

	private AnimatedEntity FindTargetEntity()
	{
		var localPawn = Local.Pawn;

		if ( localPawn is PlayerCharacter character )
		{
			return character;
		}
		else
		{
			var target = Client.All.Select( x => x.Pawn as PlayerCharacter )
				.FirstOrDefault();

			if ( target.IsValid() )
			{
				return target;
			}
		}

		return null;
	}

	public bool IsSpectator => Local.Pawn != TargetEntity;

	public override void BuildInput( InputBuilder input )
	{
		var pawn = TargetEntity;

		var wheel = input.MouseWheel;
		if ( wheel != 0 )
		{
			TargetOrbitDistance -= wheel * WheelSpeed;
			TargetOrbitDistance = TargetOrbitDistance.Clamp( CameraDistance.Min, CameraDistance.Max );
		}

		OrbitDistance = OrbitDistance.LerpTo( TargetOrbitDistance, Time.Delta * 10f );

		if ( Input.UsingController )
		{
			OrbitAngles.yaw += input.AnalogLook.yaw;
			OrbitAngles.pitch += input.AnalogLook.pitch;
			OrbitAngles = OrbitAngles.Normal;

			if ( !IsSpectator )
				input.ViewAngles = OrbitAngles.WithPitch( 0f );
		}
		else if ( input.Down( InputButton.SecondaryAttack ) )
		{
			OrbitAngles.yaw += input.AnalogLook.yaw;
			OrbitAngles.pitch += input.AnalogLook.pitch;
			OrbitAngles = OrbitAngles.Normal;

			if ( !IsSpectator )
				input.ViewAngles = OrbitAngles.WithPitch( 0f );
		}
		
		if ( !IsSpectator && pawn.IsValid() && !input.Down( InputButton.SecondaryAttack ) )
		{
			var direction = Screen.GetDirection( Mouse.Position, FieldOfView, Rotation, Screen.Size );
			var hitPos = Utils.PlaneIntersectionWithZ( Position, direction, pawn.EyePosition.z );

			input.ViewAngles = (hitPos - pawn.EyePosition).EulerAngles;
		}

		OrbitAngles.pitch = OrbitAngles.pitch.Clamp( PitchClamp.Min, PitchClamp.Max );

		// Let players move around at will
		if ( !IsSpectator )
			input.InputDirection = Rotation.From( OrbitAngles.WithPitch( 0f ) ) * input.AnalogMove;

		Sound.Listener = new()
		{
			Position = pawn.IsValid() ? pawn.EyePosition : Position,
			Rotation = Rotation
		};
	}
}
