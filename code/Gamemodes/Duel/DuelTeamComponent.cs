namespace Spire.Gamemodes.Duel;

public partial class DuelTeamComponent : EntityComponent
{
	[Net]
	public DuelTeam Team { get; set; }
}

public static partial class ClientExtensions
{
	public static DuelTeam GetTeam( this Client cl )
	{
		var teamComp = cl.Components.Get<DuelTeamComponent>();
		if ( teamComp is not null )
			return teamComp.Team;

		return DuelTeam.Blue;
	}

	public static void SetTeam( this Client cl, DuelTeam team, bool noRespawn = false )
	{
		var teamComp = cl.Components.GetOrCreate<DuelTeamComponent>();
		teamComp.Team = team;

		if ( noRespawn )
			return;
		
		Game.Current?.RespawnPlayer( cl );
	}
}
