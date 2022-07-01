namespace Spire;

public enum HoldType
{
	None = 0,
	Pistol = 1,
	Rifle = 2,
	Shotgun = 3,
	Item = 4,
	Fists = 5,
	Melee = 6
}

public static partial class HoldTypeExtensions
{
	public static int ToInt( this HoldType holdType )
	{
		return (int)holdType;
	}
}
