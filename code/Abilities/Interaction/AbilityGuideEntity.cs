namespace Spire.Abilities;

public partial class AbilityGuideEntity : ModelEntity
{
	public Particles Particle;
	public string CurrentParticlePath = "";

	public AbilityGuideEntity()
	{
	}

	public void SetParticle( string path )
	{
		if ( path == CurrentParticlePath )
			return;

		CurrentParticlePath = path;
		Particle?.Destroy( true );
		Particle = Particles.Create( path, this );
	}

	public override void Spawn()
	{
		base.Spawn();

		SetParticle( "particles/widgets/widget_aoe_pulse.vpcf" );
	}

	protected override void OnDestroy()
	{
		Particle.Destroy( true );

		base.OnDestroy();
	}
}
