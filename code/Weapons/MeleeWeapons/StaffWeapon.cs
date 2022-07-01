using Spire.Abilities;

namespace Spire;

public partial class MagicStaffWeapon : BaseWeapon
{
	public override HoldType HoldType => HoldType.Item;
	public override HoldHandedness HoldHandedness => HoldHandedness.LeftHand;

	public override string ModelPath => "models/weapons/magicstaff/magicstaff.vmdl";

	// Abilities
	public override Type AttackAbilityType => typeof( FireballAbility );
	// End of abilities

	public override void SimulateAnimator( PawnAnimator anim )
	{
		base.SimulateAnimator( anim );

		anim.SetAnimParameter( "holdtype_pose_hand", 0.07f );
	}
}
