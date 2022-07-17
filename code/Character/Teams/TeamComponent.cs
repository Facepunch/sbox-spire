namespace Spire.Gamemodes.Duel;

public partial class TeamComponent : EntityComponent
{
	[Net]
	public Team Team { get; set; }
}

public static partial class ClientExtensions
{
	public static Team GetTeam( this Client cl )
	{
		var teamComp = cl.Components.Get<TeamComponent>();
		if ( teamComp is not null )
			return teamComp.Team;

		return Team.Blue;
	}

	public static void SetTeam( this Client cl, Team team, bool noRespawn = false )
	{
		var teamComp = cl.Components.GetOrCreate<TeamComponent>();
		teamComp.Team = team;

		if ( noRespawn )
			return;
		
		Game.Current?.RespawnPlayer( cl );
	}
}
