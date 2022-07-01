namespace Spire.Abilities;

public partial class SpeedWalkAbility : PlayerAbility
{
	// Configuration
	public override string Identifier => "speed_walk";

	protected override void PreRun()
	{
		base.PreRun();

		var controller = Character.ActiveController as CharacterController;
		if ( controller is null )
			return;

		controller.SpeedMultiplier = 2f;
	}

	protected override void PostRun()
	{
		base.PostRun();

		var controller = Character.ActiveController as CharacterController;
		if ( controller is null )
			return;

		controller.SpeedMultiplier = 1f;
	}
}
