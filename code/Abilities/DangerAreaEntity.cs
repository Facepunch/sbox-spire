namespace Spire.Abilities;

public enum DangerType
{
	In,
	Out
}

public partial class DangerAreaEntity : AnimatedEntity
{
	[Net, Change]
	public string ParticlePath { get; set; }

	[Net]
	public float MaxRadius { get; set; } = 256f;

	[Net]
	public TimeUntil UntilFilled { get; set; } = 1f;

	public TimeUntil UntilRemoved { get; set; } = 1.25f;

	[Net]
	public DangerType DangerType { get; set; } = DangerType.Out;

	protected float MinRadius => 0f;
	protected Particles Particles { get; set; } = null;

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;
	}

	protected float GetCurrentRadius()
	{
		var fraction = UntilFilled.Fraction;

		if ( DangerType == DangerType.In )
		{
			return MathX.Lerp( MaxRadius, MinRadius, fraction );
		}

		return MathX.Lerp( MinRadius, MaxRadius, fraction );
	}

	protected void OnParticlePathChanged( string oldPath, string newPath )
	{
		Particles?.Destroy( true );
		Particles = Particles.Create( newPath, this );
	}

	[Event.Tick.Client]
	public void ClientTick()
	{
		if ( Particles is not null )
		{
			Particles.SetPosition( 2, new Vector3().WithX( GetCurrentRadius() ) );
		}
	}

	protected override void OnDestroy()
	{
		Particles?.Destroy( true );

		base.OnDestroy();
	}

	[Event.Tick.Server]
	public void ServerTick()
	{
		if ( UntilRemoved )
		{
			Delete();
		}
	}
}
