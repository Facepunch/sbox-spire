using Spire.Gamemodes;

namespace Spire;

public partial class ProjectileEntity : ModelEntity, ICleanupEntity
{
	// TODO: Find a better way to achieve this without networking all these strings. Use a projectile data class?
	[Net, Predicted] public string ExplosionEffect { get; set; } = "";
	[Net, Predicted] public string LaunchSoundName { get; set; } = null;
	[Net, Predicted] public string FollowEffect { get; set; } = "";
	[Net, Predicted] public string TrailEffect { get; set; } = "";
	[Net, Predicted] public Vector3 TrailEffectColor { get; set; } = new Vector3( 255f, 255f, 255f );
	[Net, Predicted] public string HitSound { get; set; } = "";
	[Net, Predicted] public string ModelPath { get; set; } = "";

	public Action<ProjectileEntity, Entity> OnHitAction { get; private set; }
	public RealTimeUntil CanHitTime { get; set; } = 0.1f;
	public float? LifeTime { get; set; }
	public string Attachment { get; set; } = null;
	public Entity Attacker { get; set; } = null;
	public bool ExplodeOnDestroy { get; set; } = true;
	public Entity IgnoreEntity { get; set; }
	public float Gravity { get; set; } = 10f;
	public float Radius { get; set; } = 8f;
	public bool FaceDirection { get; set; } = false;
	public Vector3 StartPosition { get; private set; }

	[ConVar.Replicated( "spire_projectile_debug" )]
	public static bool Debug { get; set; } = false;

	protected float GravityModifier { get; set; }
	protected RealTimeUntil DestroyTime { get; set; }
	protected Vector3 InitialVelocity { get; set; }
	protected Sound LaunchSound { get; set; }
	protected Particles Follower { get; set; }
	protected Particles Trail { get; set; }

	public void Initialize( Vector3 start, Vector3 velocity, float radius, Action<ProjectileEntity, Entity> callback = null )
	{
		Initialize( start, velocity, callback );
		Radius = radius;
	}

	public void Initialize( Vector3 start, Vector3 velocity, Action<ProjectileEntity, Entity> callback = null )
	{
		if ( LifeTime.HasValue )
		{
			DestroyTime = LifeTime.Value;
		}

		InitialVelocity = velocity;
		EnableDrawing = true;
		StartPosition = start;
		Velocity = velocity;
		OnHitAction = callback;
		Position = start;

		if ( FaceDirection )
			Rotation = Rotation.LookAt( Velocity.Normal );

		SetModel( ModelPath );

		if ( IsClientOnly )
		{
			using ( Prediction.Off() )
			{
				CreateEffects();
			}
		}
	}

	public override void Spawn()
	{
		Predictable = true;

		base.Spawn();

		Tags.Add( "projectile" );
	}

	public override void ClientSpawn()
	{
		// We only want to create effects if we don't have a client proxy.
		if ( !HasClientProxy() )
		{
			CreateEffects();
		}

		base.ClientSpawn();
	}

	public virtual void CreateEffects()
	{
		if ( !string.IsNullOrEmpty( TrailEffect ) )
		{
			Trail = Particles.Create( TrailEffect, this );

			Trail.SetPosition( 6, TrailEffectColor );

			if ( !string.IsNullOrEmpty( Attachment ) )
				Trail.SetEntityAttachment( 0, this, Attachment );
			else
				Trail.SetEntity( 0, this );
		}

		if ( !string.IsNullOrEmpty( FollowEffect ) )
		{
			Follower = Particles.Create( FollowEffect, this );
		}

		if ( !string.IsNullOrEmpty( LaunchSoundName ) )
			LaunchSound = PlaySound( LaunchSoundName );
	}

	public virtual void Simulate()
	{
		if ( FaceDirection )
		{
			Rotation = Rotation.LookAt( Velocity.Normal );
		}

		if ( Debug )
		{
			DebugOverlay.Sphere( Position, Radius, IsClient ? Color.Blue : Color.Red );
		}

		var newPosition = GetTargetPosition();

		var trace = Trace.Ray( Position, newPosition )
			.HitLayer( CollisionLayer.Water, true )
			.Size( Radius )
			.Ignore( this )
			.Ignore( IgnoreEntity )
			.WithoutTags( "projectile" )
			.Run();

		Position = trace.EndPosition;

		if ( LifeTime.HasValue && DestroyTime )
		{
			if ( ExplodeOnDestroy )
			{
				PlayHitEffects( Vector3.Zero );
				OnHitAction?.Invoke( this, trace.Entity );
			}

			Delete();

			return;
		}

		if ( HasHitTarget( trace ) )
		{
			trace.Surface.DoBulletImpact( trace );

			PlayHitEffects( trace.Normal );
			OnHitAction?.Invoke( this, trace.Entity );
			Delete();
		}
	}

	public bool HasClientProxy()
	{
		return !IsClientOnly && Owner.IsValid() && Owner.IsLocalPawn;
	}

	protected virtual bool HasHitTarget( TraceResult trace )
	{
		return (trace.Hit && CanHitTime) || trace.StartedSolid;
	}

	protected virtual Vector3 GetTargetPosition()
	{
		var newPosition = Position;
		newPosition += Velocity * Time.Delta;

		GravityModifier += Gravity;
		newPosition -= new Vector3( 0f, 0f, GravityModifier * Time.Delta );

		return newPosition;
	}

	[ClientRpc]
	protected virtual void PlayHitEffects( Vector3 normal )
	{
		if ( HasClientProxy() )
		{
			// We don't want to play hit effects if we have a client proxy.
			return;
		}
	}

	[Event.Tick.Server]
	protected virtual void ServerTick()
	{
		Simulate();
	}

	protected override void OnDestroy()
	{
		RemoveEffects();

		base.OnDestroy();
	}

	private void RemoveEffects()
	{
		Follower?.Destroy();
		Trail?.Destroy();
		Trail = null;
	}
}
