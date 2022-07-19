namespace Spire;

public partial class SpectatorPawn : BasePawn
{
	public override void Respawn()
	{
		base.Respawn();

		var camera = new PlayerCamera();
		CameraMode = camera;
	}
}
