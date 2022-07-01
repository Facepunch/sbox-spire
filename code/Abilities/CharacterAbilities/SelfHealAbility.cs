using Spire.Buffs;

namespace Spire.Abilities;

public partial class SelfHealAbility : PlayerAbility
{
	// Configuration
	public override string Identifier => "self_heal";

	protected override void PreRun()
	{
		base.PreRun();

		Character.AddBuff<BaseHealingBuff>();
	}
}
