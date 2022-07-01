
namespace Spire;

public partial class PlayerHotbar
{
	public void Simulate( Client cl )
	{
		if ( Input.Pressed( InputButton.Slot1 ) ) SetActiveSlot( 0 );
		if ( Input.Pressed( InputButton.Slot2 ) ) SetActiveSlot( 1 );
		if ( Input.Pressed( InputButton.Slot3 ) ) SetActiveSlot( 2 );
		if ( Input.Pressed( InputButton.Slot4 ) ) SetActiveSlot( 3 );
		if ( Input.Pressed( InputButton.Slot5 ) ) SetActiveSlot( 4 );
		if ( Input.Pressed( InputButton.Slot6 ) ) SetActiveSlot( 5 );
		if ( Input.Pressed( InputButton.Slot7 ) ) SetActiveSlot( 6 );
		if ( Input.Pressed( InputButton.Slot8 ) ) SetActiveSlot( 7 );
		if ( Input.Pressed( InputButton.Slot9 ) ) SetActiveSlot( 8 );
	}
}
