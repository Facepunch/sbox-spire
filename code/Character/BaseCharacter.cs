using Sandbox;
using Spire.Gamemodes;
using Spire.UI;
using Spire.AI;

namespace Spire;

public partial class BaseCharacter : BasePawn
{
	[Net, Predicted]
	public BaseCarriable LastActiveChild { get; set; }

	[Net, Predicted]
	public BaseCarriable ActiveChild { get; set; }

	[Net, Predicted]
	public CharacterController Controller { get; set; }

	[Net, Predicted]
	protected PawnAnimator Animator { get; set; }

	[Net, Predicted]
	public AIController AIController { get; set; }

	public virtual PawnController ActiveController => Controller;
	public virtual PawnAnimator ActiveAnimator => Animator;
	public virtual float MaxHealth => 100f;

	public DamageInfo LastDamageInfo { get; protected set; }

	public ClothingContainer Clothing = new();

	public BaseNameplate Nameplate { get; set; }

	public TimeSince TimeSinceDamage = 1f;

	public BaseCharacter()
	{
		Tags.Add( "solid" );
		Tags.Add( "player" );
	}

	public override void Simulate( Client cl )
	{
		SimulateActiveChild( cl, ActiveChild );
		ActiveController?.Simulate( cl, this, ActiveAnimator );

		SimulateBuffs( cl );
	}

	public override void Respawn()
	{
		base.Respawn();

		Host.AssertServer();

		SetModel( "models/citizen/citizen.vmdl" );

		Animator = new CharacterAnimator();

		LifeState = LifeState.Alive;
		Health = MaxHealth;
		Velocity = Vector3.Zero;

		Clothing.DressEntity( this );

		CreateHull();
		ResetInterpolation();

		Transform = ( BaseGamemode.Current?.GetSpawn( this ) ?? All.OfType<SpawnPoint>().FirstOrDefault()?.Transform ?? Transform.Zero ).WithScale( 1f );
	}

	[ClientRpc]
	protected void RpcTakeDamage( Vector3 pos, float damage )
	{
		ClientTakeDamage( pos, damage );

		TimeSinceDamage = 0;
	}

	protected virtual void ClientTakeDamage( Vector3 pos, float damage )
	{
		DamageIndicator.Create( pos, damage );
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( BaseGamemode.Current?.AllowDamage() == false )
			return;

		base.TakeDamage( info );

		LastDamageInfo = info;

		if ( Host.IsServer )
			RpcTakeDamage( info.Position, info.Damage );
	}

	public virtual void SimulateActiveChild( Client client, BaseCarriable child )
	{
		if ( LastActiveChild != child )
		{
			OnActiveChildChanged( LastActiveChild, child );
			LastActiveChild = child;
		}

		if ( !LastActiveChild.IsValid() )
			return;

		LastActiveChild.Simulate( client );
	}

	public virtual void OnActiveChildChanged( BaseCarriable previous, BaseCarriable next )
	{
		previous?.ActiveEnd( this, previous.Owner != this );
		next?.ActiveStart( this );
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		ActiveController?.FrameSimulate( cl, this, ActiveAnimator );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Host.IsClient )
			Nameplate?.Delete();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		BecomeRagdollOnClient(
			Velocity,
			LastDamageInfo.Flags,
			LastDamageInfo.Position,
			LastDamageInfo.Force,
			LastDamageInfo.BoneIndex );

		BaseGamemode.Current?.OnCharacterKilled( this, LastDamageInfo );
	}

	public virtual void CreateHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		EnableHitboxes = true;
	}

	TimeSince timeSinceLastFootstep = 0;

	/// <summary>
	/// A foostep has arrived!
	/// </summary>
	public override void OnAnimEventFootstep( Vector3 pos, int foot, float volume )
	{
		if ( LifeState != LifeState.Alive )
			return;

		if ( !IsClient )
			return;

		if ( timeSinceLastFootstep < 0.2f )
			return;

		volume *= FootstepVolume();

		timeSinceLastFootstep = 0;

		var tr = Trace.Ray( pos, pos + Vector3.Down * 20 )
			.Radius( 1 )
			.Ignore( this )
			.Run();

		if ( !tr.Hit ) return;

		tr.Surface.DoFootstep( this, tr, foot, volume );
	}

	public virtual float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0.0f, 200.0f ) * 0.2f;
	}
}
