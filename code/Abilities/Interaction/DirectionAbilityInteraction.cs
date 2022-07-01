namespace Spire.Abilities;

public partial class DirectionAbilityInteraction : AbilityInteraction
{
	public override void OnTick()
	{
		base.OnTick();

		if ( Input.Pressed( InputButton.PrimaryAttack ) )
		{
			End();
		}
		else if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{
			Cancel();
		}
	}

	public Vector3 Color => new Vector3( 1f, 1f, 1f );

	protected override void TickGuide()
	{
		var character = Ability.GetCharacter();

		GuideEntity.SetParticle( "particles/widgets/arrow/widget_arrow.vpcf" );
		GuideEntity.Position = character.Position + character.EyeRotation.Forward * 25f;

		GuideEntity.Particle.SetPosition( 4, Color );
		GuideEntity.Particle.SetPosition( 1, character.Position + character.EyeRotation.Forward * 256f );
	}

	protected override void OnEnd()
	{
		base.OnEnd();

		Ability.Run();
	}
}
