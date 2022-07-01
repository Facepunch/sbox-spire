namespace Spire.Abilities;

public partial class WorldPointAbilityInteraction : AbilityInteraction
{
	public override void OnTick()
	{
		WorldCursorPosition = GetWorldCursor();

		base.OnTick();

		if ( Input.Pressed( InputButton.PrimaryAttack ) )
		{
			TryEnd();
		}
		else if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{
			Cancel();
		}
	}

	protected bool IsInRange()
	{
		var range = Ability.Data.AbilityRange;
		var entity = Ability.Entity;
		var dist = entity.Position.Distance( WorldCursorPosition );

		return dist <= range.Max && dist >= range.Min;
	}

	protected void TryEnd()
	{
		if ( !IsInRange() )
		{
			Cancel();
			return;
		}

		End();
	}

	protected Vector3 AvailableCPColor => new Vector3( Color.Green );
	protected Vector3 UnavailableCPColor => new Vector3( Color.Red );

	protected override void TickGuide()
	{
		AltGuideEntity.SetParticle( "particles/widgets/widget_area.vpcf" );
		AltGuideEntity.Position = Ability.GetCharacter().Position;
		AltGuideEntity.Particle.SetPosition( 4, new Vector3().WithX( Ability.Data.AbilityRange.Max ) );

		GuideEntity.Position = WorldCursorPosition + Vector3.Up * 10f;
		GuideEntity.Particle.SetPosition( 2, IsInRange() ? AvailableCPColor : UnavailableCPColor );
		GuideEntity.Particle.SetPosition( 4, new Vector3().WithX( Ability.Data.AbilityEffectRadius ) );
	}

	protected override void OnEnd()
	{
		base.OnEnd();

		Ability.Run();
	}
}
