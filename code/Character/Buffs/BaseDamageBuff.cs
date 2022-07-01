namespace Spire.Buffs;

public class BaseDamageBuff : Buff
{
	public override string Name => "Minor Damage";
	public override string Description => "";
	public override float Duration => 5f;
	public override string IconPath => "Some icon path.";
	public override float TickInterval => 1f;
	public override bool TickImmediate => false;
	//
	public virtual float DamageAmountPerTick => 2f;

	public override void OnTick( BaseCharacter character )
	{
		base.OnTick( character );

		var damage = DamageInfo.Generic( DamageAmountPerTick )
			.WithPosition( character.Position + Vector3.Up * 42f )
			.WithWeapon( character.ActiveChild as BaseWeapon );

		character.TakeDamage( damage );

		Util.CreateParticle( character, "particles/impact.flesh.vpcf", false, Vector3.Up * 40f );
	}

	public override void OnDestroy( BaseCharacter character )
	{
		base.OnDestroy( character );
	}
}
