namespace Spire;

public partial class PlayerHotbar : BaseNetworkable
{
	[Net, Predicted]
	public IList<BaseCarriable> Inventory { get; set; }

	[Net, Predicted]
	public BaseCharacter Character { get; set; }

	public PlayerHotbar()
	{
	}

	public PlayerHotbar( BaseCharacter character )
	{
		Character = character;
	}

	public BaseCarriable Get( int index )
	{
		if ( index >= Inventory.Count || index < 0 )
		{
			return null;
		}

		return Inventory[index];
	}

	public bool CanAdd( BaseCarriable carriable )
	{
		return !InInventory( carriable );
	}

	public void Add( BaseCarriable carriable, bool makeActive = true )
	{
		if ( !CanAdd( carriable ) )
			return;

		Inventory.Add( carriable );

		if ( makeActive )
			SetCurrent( carriable );
	}

	public bool CanRemove( BaseCarriable carriable )
	{
		return true;
	}

	public void Remove( BaseCarriable carriable, bool removeActive = true )
	{
		Inventory.Remove( carriable );

		if ( Character.ActiveChild == carriable )
		{
			SetCurrent( null );
		}
	}

	public bool InInventory( BaseCarriable carriable )
	{
		return Inventory.Contains( carriable );
	}

	public void SetActiveSlot( int index )
	{
		var weapon = Get( index );

		if ( weapon.IsValid() )
		{
			SetCurrent( weapon );
		}
		else
		{
			SetCurrent( null );
		}
	}

	public void SetCurrent( BaseCarriable carriable )
	{
		var current = Character.ActiveChild;
		current?.ActiveEnd( Character, false );

		// We can carry a weapon if we're full, but when we're done with it - drop it.
		if ( !InInventory( current ) )
			current?.OnCarryDrop( Character );

		Character.ActiveChild = carriable;

		carriable?.OnCarryStart( Character );
		carriable?.ActiveStart( Character );
	}
}
