namespace Spire;

public partial class StickyProjectileEntity : ProjectileEntity
{
	public Entity AttachedEntity { get; set; }
	protected float StickyDestroyTime { get; set; } = 60f;

	protected virtual float GetDestroyTime( Entity hitEntity )
	{
		return StickyDestroyTime;
	}

	[ClientRpc]
	protected void KillEffects()
	{
		Trail?.Destroy();
		Follower?.Destroy();
	}

	public override void Simulate()
	{
		if ( AttachedEntity.IsValid() )
		{
			if ( DestroyTime )
			{
				PlayHitEffects( Vector3.Zero );
				Delete();
			}
		}
		else
		{
			base.Simulate();
		}
	}

	protected override bool HasHitTarget( TraceResult trace )
	{
		if ( trace.Entity.IsValid() )
		{
			AttachedEntity = trace.Entity;
			SetParent( trace.Entity );
			DestroyTime = GetDestroyTime( trace.Entity );

			KillEffects();

			OnHitAction?.Invoke( this, AttachedEntity );
		}

		return false;
	}
}
