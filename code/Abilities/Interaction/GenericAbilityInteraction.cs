namespace Spire.Abilities;

public partial class GenericAbilityInteraction : AbilityInteraction
{
	/// <summary>
	/// Tell the interaction to end immediately
	/// </summary>
	protected override void OnStart()
	{
		base.OnStart();

		End();
	}

	/// <summary>
	/// Run the ability, since it's immediate
	/// </summary>
	protected override void OnEnd()
	{
		base.OnEnd();

		Ability.Run();
	}
}
