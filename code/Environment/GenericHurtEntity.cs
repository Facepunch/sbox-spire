using SandboxEditor;

namespace Spire;

public class HurtEntityInfo
{
	public HurtEntityInfo( float nextHurt )
	{
		NextHurt = nextHurt;
		CurrentHits = 0;
	}

	public TimeUntil NextHurt { get; set; }
	public int CurrentHits { get; set; }

	public float DamageMultiplier { get; set; } = 1f;

	public float GetDamage( float baseDamage )
	{
		return baseDamage * DamageMultiplier; 
	}

	public TimeUntil CalculateNextHurt( float hurtInterval, float timeMutliplier = 0f )
	{
		return hurtInterval - ( timeMutliplier * CurrentHits );
	}
}

[HammerEntity]
[Solid]
[Title( "Hurt Trigger" ), Category( "Triggers" ), Icon( "personal_injury" )]
public partial class GenericHurtEntity : BaseTrigger
{
	[Property, Category( "Trigger" ), Description( "How long this Hurt Entity will exist before being destroyed. Set to 0 for indefinitely (good for maps)." )]
	public float Lifetime { get; set; } = 0f;

	[Property, Category( "Trigger" ), ResourceType( "vpcf" ), Description( "The particle emitted from the character every time they are hurt." )]
	public string HurtParticle { get; set; } = "";

	[Property, Category( "Trigger" )]
	public string HurtSound { get; set; } = "";

	[Property, Category( "Trigger" ), Description( "How often (in seconds) entities inside the trigger will be hurt." )]
	public float HurtInterval { get; set; } = 1f;

	[Property( "damage", Title = "Damage" ), Category( "Trigger" )]
	public float Damage { get; set; } = 10.0f;

	[Property, Category( "Trigger" ), Description( "Damage multiplier to add every time the entity is hit." )]
	public float DamageMultiplier { get; set; } = 0f;

	[Property, Category( "Trigger" ), Description( "Any positive value will increase the hit frequency each time by that amount." )]
	public float TimeMutliplier { get; set; } = 0f;

	[Property, Category( "Trigger" ), Description( "Max hits that this hurt trigger can provide before stopping." )]
	public int MaxHits { get; set; } = 0;


	protected Output OnHurt { get; set; }

	public TimeUntil UntilTick { get; set; }
	public TimeUntil UntilDestroy { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		UntilTick = HurtInterval;
	}

	protected void PerformEffects( Entity entity )
	{
		if ( !string.IsNullOrEmpty( HurtParticle ) )
			Util.CreateParticle( entity, HurtParticle, true );

		if ( !string.IsNullOrEmpty( HurtSound ) )
			entity.PlaySound( HurtSound );
	}

	public Dictionary<Entity, HurtEntityInfo> Interactors { get; set; } = new();

	[Event.Tick.Server]
	protected virtual void Tick()
	{
		if ( !Enabled )
			return;

		if ( Lifetime > 0 && UntilDestroy )
			Delete();

		foreach ( var kv in Interactors )
		{
			var entity = kv.Key;

			if ( !entity.IsValid() )
				continue;

			var info = kv.Value;
			if ( !info.NextHurt ) 
				continue;

			if ( MaxHits > 0 && info.CurrentHits > MaxHits )
				continue;

			info.CurrentHits++;
			info.NextHurt = info.CalculateNextHurt( HurtInterval, TimeMutliplier );

			entity.TakeDamage( DamageInfo.Generic( info.GetDamage( Damage ) ).WithAttacker( this ).WithPosition( entity.Position ) );

			PerformEffects( entity );

			OnHurt.Fire( entity );

			if ( DamageMultiplier > 0f )
				info.DamageMultiplier += DamageMultiplier;
		}
	}

	public override void OnTouchEnd( Entity toucher )
	{
		base.OnTouchEnd( toucher );

		Interactors.Remove( toucher );
	}

	public override void OnTouchStart( Entity toucher )
	{
		base.OnTouchStart( toucher );

		if ( !Interactors.ContainsKey( toucher ) )
			Interactors.Add( toucher, new( HurtInterval ) );
	}
}
