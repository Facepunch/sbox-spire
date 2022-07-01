namespace Spire.Gamemodes.Duel;

public enum DuelTeam
{
	Red,
	Blue
}

public static partial class DuelTeamHelpers
{
	public static DuelTeam GetLowestTeam()
	{
		DuelTeam currentLowest = DuelTeam.Blue;
		int value = 999;
		foreach ( var _team in Enum.GetValues<DuelTeam>() )
		{
			var members = _team.GetMembers();
			if ( members.Count() <= value )
			{
				value = members.Count();
				currentLowest = _team;
			}
		}

		return currentLowest;
	}
}

public static partial class DuelTeamExtensions
{
	public static IEnumerable<Client> GetMembers( this DuelTeam team )
	{
		return Client.All.Where( x => x.GetTeam() == team );
	}

	public static int Count( this DuelTeam team )
	{
		return team.GetMembers().Count();
	}

	public static IEnumerable<Client> GetAliveMembers( this DuelTeam team )
	{
		return Client.All.Where( x => x.GetTeam() == team ).Where( x => x.Pawn is PlayerCharacter && x.Pawn.IsValid() && x.Pawn.LifeState == LifeState.Alive );
	}

	public static int AliveCount( this DuelTeam team )
	{
		return team.GetAliveMembers().Count();
	}

	public static string GetName( this DuelTeam team )
	{
		return team switch
		{
			DuelTeam.Red => "Red",
			DuelTeam.Blue => "Blue",
			_ => "N/A"
		};
	}

	public static Color GetColor( this DuelTeam team )
	{
		return team switch
		{
			DuelTeam.Red => new Color32( 228, 52, 52 ).ToColor(),
			DuelTeam.Blue => new Color32( 52, 99, 228 ).ToColor(),
			_ => Color.Gray
		};
	}
}
