namespace Spire.UI;

[UseTemplate]
public partial class NameplateContainer : Panel
{
	// @ref
	public Panel Canvas { get; set; }

	public override void Tick()
	{
		Entity.All.OfType<BaseCharacter>()
				.OrderBy( x => Vector3.DistanceBetween( x.EyePosition, CurrentView.Position ) )
				.ToList()
				.ForEach( x => UpdateNameplate( x ) );
	}

	protected void UpdateNameplate( BaseCharacter character )
	{
		if ( !character.IsValid() )
			return;

		if ( character.Nameplate is not null )
		{
			var nameplate = character.Nameplate;
			nameplate.Parent = Canvas;
			nameplate.Update();
		}
	}
}
