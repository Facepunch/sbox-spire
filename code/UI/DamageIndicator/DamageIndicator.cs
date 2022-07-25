namespace Spire.UI;

[UseTemplate]
public partial class DamageIndicator : Panel
{
	protected float DamageAmount { get; set; } = 0f;
	protected Vector3 WorldPosition { get; set; } = Vector3.Zero;

	protected virtual float Lifetime => 1f;

	public static void Create( Vector3 worldPos, float damageAmount )
	{
		var p = new DamageIndicator();
		p.Parent = Local.Hud;
		p.Setup( worldPos, damageAmount );
	}

	protected async Task Fade()
	{
		await GameTask.DelaySeconds( Lifetime );
		Delete();
	}

	protected void Setup( Vector3 worldPos, float damageAmount )
	{
		if ( damageAmount.FloorToInt() < 1 )
		{
			Delete( true );
			return;
		}

		WorldPosition = worldPos;
		DamageAmount = -damageAmount.Floor();

		_ = Fade();
	}

	public override void Tick()
	{
		base.Tick();

		var screenPos = WorldPosition.ToScreen();

		Style.Left = Length.Fraction( screenPos.x );
		Style.Top = Length.Fraction( screenPos.y );
	}
}
