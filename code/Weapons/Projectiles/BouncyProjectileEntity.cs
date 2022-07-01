namespace Spire;

[Library]
public partial class BouncyProjectileEntity : ProjectileEntity
{
	public float Bounciness { get; set; } = 1f;
	public float ReflectionScale { get; set; } = 1f;

	public Action<ProjectileEntity, Entity> OnBounce { get; set; }

	protected override bool HasHitTarget( TraceResult trace )
	{
		if ( LifeTime.HasValue )
		{
			if ( trace.Hit )
			{
				var reflectAmount = Vector3.Reflect( Velocity.Normal, trace.Normal );
				GravityModifier = 0f;
				Velocity = (reflectAmount * ReflectionScale) * Velocity.Length * Bounciness;

				OnBounce?.Invoke( this, trace.Entity );
			}

			return false;
		}

		return base.HasHitTarget( trace );
	}
}
