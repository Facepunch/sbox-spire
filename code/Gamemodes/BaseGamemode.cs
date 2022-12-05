namespace Spire.Gamemodes;

public abstract partial class BaseGamemode : Entity
{
	public static BaseGamemode Current { get; set; }

	/// <summary>
	/// A quick accessor to get how many people are in the game
	/// </summary>
	public int PlayerCount { get; private set; }

	/// <summary>
	/// Can specify a panel to be created when the gamemode is made.
	/// </summary>
	/// <returns></returns>
	public virtual Panel GetGamemodePanel() => null;

	/// <summary>
	/// Gamemodes can define what pawn to create
	/// </summary>
	/// <param name="cl"></param>
	/// <returns></returns>
	public virtual BasePawn GetPawn( Client cl ) => new PlayerCharacter( cl );

	public virtual void CreatePawn( Client cl )
	{
		cl.Pawn?.Delete();

		var pawn = Current?.GetPawn( cl );
		cl.Pawn = pawn;
		pawn.Respawn();
	}

	public override void Spawn()
	{
		base.Spawn();

		Transmit = TransmitType.Always;

		// There can be only one gamemode running at a time.
		if ( Current.IsValid() && Current != this )
		{
			Delete();
			Log.Warning( "There can be only one gamemode running at one time. Please make sure there's only 1 gamemode entity on a level." );

			return;
		}

		Current = this;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();
		Current = this;
	}

	/// <summary>
	/// Called when a client joins the game
	/// </summary>
	/// <param name="cl"></param>
	public virtual void OnClientJoined( Client cl )
	{
		PlayerCount++;
	}

	/// <summary>
	/// Called when a client leaves the game
	/// </summary>
	/// <param name="cl"></param>
	public virtual void OnClientLeft( Client cl, NetworkDisconnectionReason reason )
	{
		PlayerCount--;
	}

	/// <summary>
	/// Called when a character takes damage
	/// </summary>
	/// <param name="character"></param>
	/// <param name="damageInfo"></param>
	public virtual void OnCharacterKilled( BaseCharacter character, DamageInfo damageInfo )
	{
	}

	/// <summary>
	/// Allows gamemodes to override character spawn locations
	/// </summary>
	/// <param name="character"></param>
	/// <returns></returns>
	public virtual Transform? GetSpawn( BaseCharacter character )
	{
		return null;
	}

	/// <summary>
	/// Decides whether or not players can move
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowMovement()
	{
		return true;
	}

	/// <summary>
	/// Decides whether or not players can respawn
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowRespawning()
	{
		return true;
	}

	/// <summary>
	/// Decides whether or not characters can take damage
	/// </summary>
	/// <returns></returns>
	public virtual bool AllowDamage()
	{
		return true;
	}

	public virtual void CleanupMap()
	{
		Entity.All.Where( x => x is ICleanupEntity )
			.ToList()
			.ForEach( x => x.Delete() );
	}
}
