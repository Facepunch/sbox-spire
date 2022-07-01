using Spire.AI;

namespace Spire.WaveSurvival;

public partial class WSAIController : AIController
{
	public WSAIController()
	{
		Navigator = new();
	}

	protected Transform? FindTarget()
	{
		var ents = Entity.All;

		return ents.OfType<PlayerCharacter>().FirstOrDefault()?.Transform;
	}

	public override void Tick()
	{
		var target = FindTarget();

		if ( target is not null )
		{
			Navigator.SetTarget( target.Value.Position );
		}

		base.Tick();
	}
}
