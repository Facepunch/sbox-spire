namespace Spire.Abilities;

public abstract class BaseMeleeAttackAbility : WeaponAbility
{
	// BaseMeleeAttackAbility Configuration
	/// <summary>
	/// How long until we try to inflict damage from the weapon after an attack.
	/// </summary>
	public virtual float AttackInflictDelay => 0.02f;
	public virtual float AttackRange => 100f;
	public virtual float BaseDamage => 30f;
	public virtual int MaxConeAngle => 85;

	// Extract to a util later?
	public static bool IsPointInsideCone( Vector3 point, Vector3 coneOrigin, Vector3 coneDirection, int maxAngle, int maxDistance )
	{
		var distanceToConeOrigin = (point - coneOrigin).Length;
		if ( distanceToConeOrigin < maxDistance )
		{
			var pointDirection = point - coneOrigin;
			var angle = Vector3.GetAngle( coneDirection, pointDirection );

			if ( angle < maxAngle )
				return true;
		}
		return false;
	}

	protected virtual float GetDamage()
	{
		return BaseDamage;
	}

	protected void TestInCone( Entity ent )
	{
		var Owner = Weapon.Owner;

		var bIsInCone = IsPointInsideCone(
			ent.EyePosition,
			Owner.EyePosition,
			Owner.EyeRotation.Forward,
			MaxConeAngle,
			AttackRange.CeilToInt()
		);

		if ( bIsInCone )
			RunDamageTrace( ent );
	}

	protected virtual void RunDamageTrace( Entity entity )
	{
		const float _attackForce = 1024f;

		// Trace for visual effects
		var tr = Trace.Ray( Weapon.Position, entity.EyePosition ).Ignore( Weapon.Owner ).UseHitboxes( true ).Run();

		// Damage builder
		var damageInfo = new DamageInfo()
				.WithPosition( tr.EndPosition )
				.WithFlag( DamageFlags.Bullet )
				.WithForce( Weapon.Owner.EyeRotation.Forward * _attackForce )
				.UsingTraceResult( tr )
				.WithAttacker( Weapon.Owner )
				.WithWeapon( Weapon );

		damageInfo.Damage = GetDamage();

		// Handle surface impacts
		tr.Surface.DoBulletImpact( tr );

		// Finally, inflict damage on our target
		entity.TakeDamage( damageInfo );

		PlaySound( "hit", entity );

		OnTargetDamaged( entity, damageInfo );
	}

	protected virtual void OnTargetDamaged( Entity entity, DamageInfo damageInfo )
	{
	}

	protected virtual async Task StartMeleeAttack()
	{
		await GameTask.DelaySeconds( AttackInflictDelay );

		if ( !Weapon.IsValid() || !Weapon.Owner.IsValid() )
			return;

		var ents = Entity.FindInSphere( Weapon.Position, AttackRange )
			.Where( x => x is BaseCharacter && x != Weapon.Owner )
			.ToList();

		foreach ( var entity in ents )
		{
			TestInCone( entity );
		}
	}

	protected override void PreRun()
	{
		base.PreRun();

		DoPlayerAnimation();

		_ = StartMeleeAttack();
	}
}
