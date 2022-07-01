namespace Spire;

public partial class BasePawn : AnimatedEntity
{
	public CameraMode CameraMode
	{
		get => Components.Get<CameraMode>();
		set => Components.Add( value );
	}

	public virtual void Respawn()
	{
	}
}
