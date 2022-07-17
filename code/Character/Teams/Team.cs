namespace Spire;

public enum Team
{
	None,
	Red,
	Blue
}

public static partial class TeamHelpers
{
	public static Team GetLowestTeam()
	{
		Team currentLowest = Team.Blue;
		int value = 999;
		foreach ( var _team in Enum.GetValues<Team>() )
		{
			if ( _team == Team.None ) continue;

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

public static partial class TeamExtensions
{
	public static IEnumerable<Client> GetMembers( this Team team )
	{
		return Client.All.Where( x => x.GetTeam() == team );
	}

	public static int Count( this Team team )
	{
		return team.GetMembers().Count();
	}

	public static IEnumerable<Client> GetAliveMembers( this Team team )
	{
		return Client.All.Where( x => x.GetTeam() == team ).Where( x => x.Pawn is PlayerCharacter && x.Pawn.IsValid() && x.Pawn.LifeState == LifeState.Alive );
	}

	public static int AliveCount( this Team team )
	{
		return team.GetAliveMembers().Count();
	}

	public static string GetName( this Team team )
	{
		return team switch
		{
			Team.Red => "Red",
			Team.Blue => "Blue",
			_ => "N/A"
		};
	}

	public static Color GetColor( this Team team )
	{
		return team switch
		{
			Team.Red => new Color32( 228, 52, 52 ).ToColor(),
			Team.Blue => new Color32( 52, 99, 228 ).ToColor(),
			_ => Color.Gray
		};
	}
}
