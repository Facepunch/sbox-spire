namespace Spire.UI;

public partial class AINameplate : BaseNameplate
{
	public AINameplate( BaseCharacter character ) : base( character )
	{
	}

	public override void Update()
	{
		base.Update();

		if ( Character is AICharacter aiCharacter )
		{
			//
		}
	}
}
