namespace Spire;

public enum HoldHandedness
{
	TwoHands = 0,
	RightHand = 1,
	LeftHand = 2
}

public static partial class HoldTypeExtensions
{
	public static int ToInt( this HoldHandedness handedness )
	{
		return (int)handedness;
	}
}
