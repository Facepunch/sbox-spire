namespace Spire.Abilities;

public partial class ConeArrowAttack : BasicArrowAttack
{
	// Configuration
	public override string Identifier => "cone_arrow_attack";
	public override WeaponAbilityType Type => WeaponAbilityType.Special;

	protected override void PostRun()
	{
		base.PostRun();

		if ( Host.IsClient )
			return;

		CreateProjectile( -15 );
		CreateProjectile( 0f );
		CreateProjectile( 15 );
	}

	public override bool TickGuide( AbilityInteraction interaction )
	{
		var character = GetCharacter();

		var entity = AbilityInteraction.AltGuideEntity;

		entity.SetParticle( "particles/widgets/cone/widget_cone_base_45.vpcf" );
		entity.Position = character.Position;
		entity.Rotation = character.EyeRotation;

		return true;
	}
}
