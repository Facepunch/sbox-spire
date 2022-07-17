using Spire.Gamemodes.Duel;

namespace Spire.UI;

[UseTemplate]
public partial class PlayerNameplate : BaseNameplate
{
	public string PlayerName => Character.Client?.Name;
	public override string NameplateName => PlayerName.Truncate( 16, "..." );
	protected bool IsLocalPlayer => Character == Local.Pawn;

	// @ref
	public Panel HealthBarFill { get; set; }

	protected float LerpedHealthFraction { get; set; } = 1f;

	public PlayerNameplate( BaseCharacter character ) : base( character )
	{
		BindClass( "local", () => IsLocalPlayer );
	}

	public override void Update()
	{
		base.Update();

		LerpedHealthFraction = LerpedHealthFraction.LerpTo( HealthFraction, Time.Delta * 10f );
		HealthBarFill.Style.Width = Length.Fraction( LerpedHealthFraction );

		var cl = Character.Client;
		if ( cl.IsValid() )
		{
			var team = cl.GetTeam();
			SetClass( "red", team == Team.Red );
			SetClass( "blue", team == Team.Blue );
		}
	}
}
