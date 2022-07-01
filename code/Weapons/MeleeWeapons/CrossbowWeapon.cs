using Spire.Abilities;

namespace Spire;

public partial class CrossbowWeapon : BaseWeapon
{
	public override HoldType HoldType => HoldType.Shotgun;
	public override HoldHandedness HoldHandedness => HoldHandedness.RightHand;

	public override string ModelPath => "weapons/rust_crossbow/rust_crossbow.vmdl";

	// Abilities
	public override Type AttackAbilityType => typeof( BasicArrowAttack );
	public override Type SpecialAbilityType => typeof( ConeArrowAttack );
	public override Type UltimateAbilityType => typeof( ExplosiveArrowAttack );
	// End of abilities

	public override void SimulateAnimator( PawnAnimator anim )
	{
		base.SimulateAnimator( anim );
	}
}
