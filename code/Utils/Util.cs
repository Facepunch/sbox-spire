namespace Spire;

public partial class Util
{
	[ClientRpc]
	public static void CreateParticle( Entity entity, string path, bool follow, Vector3 offset = default )
	{
		if ( follow )
			Particles.Create( path, entity, true );
		else
			Particles.Create( path, entity.Position + offset );
	}

	protected static async Task DeleteAsync( Particles particle, float lifetime )
	{
		await GameTask.DelaySeconds( lifetime );

		particle.Destroy();
	}

	[ClientRpc]
	public static void CreateParticle( Entity entity, string path, bool follow, string attachment, float lifetime = 0f )
	{
		var particle = Particles.Create( path, entity, attachment, follow );

		if ( lifetime > 0f )
		{
			_ = DeleteAsync( particle, lifetime );
		}
	}
}
