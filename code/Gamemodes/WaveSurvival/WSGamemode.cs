using SandboxEditor;
using Spire.Gamemodes;

namespace Spire.WaveSurvival;

[HammerEntity]
[Title( "Wave Survival" ), Category( "Spire - Game Modes" ), Icon( "sports_kabaddi" )]
public partial class WSGamemode : BaseGamemode
{
	public override Panel GetGamemodePanel() => null;


	public override BasePawn GetPawn( Client cl )
	{
		return new PlayerCharacter( cl );
	}

	public override void Spawn()
	{
		base.Spawn();

		int SpawnNum = 1;
		// Create a couple of AI characters.
		for ( int i = 0; i < SpawnNum; i++ )
			CreateRandomAI();
	}

	public override void OnClientJoined( Client cl )
	{
		base.OnClientJoined( cl );
	}

	public override void OnCharacterKilled( BaseCharacter character, DamageInfo damageInfo )
	{
		if ( character is WSNpcTest ai )
		{
			CreateRandomAI();
		}

		base.OnCharacterKilled( character, damageInfo );
	}

	public override Transform? GetSpawn( BaseCharacter character )
	{
		return All.OfType<SpawnPoint>()
			.Where( x => !x.Tags.Has( "ai" ) )
			.OrderBy( x => Guid.NewGuid() )
			.FirstOrDefault()?.Transform ?? null;
	}

	public virtual Transform? GetAISpawn( BaseCharacter character )
	{
		return All.OfType<SpawnPoint>()
			.Where( x => x.Tags.Has( "ai" ) )
			.OrderBy( x => Guid.NewGuid() )
			.FirstOrDefault()?.Transform ?? null;
	}

	public void CreateRandomAI()
	{
		var ai = new WSNpcTest();
		ai.Respawn();

		var transform = GetAISpawn( ai ) ?? default;
		ai.Transform = transform;
	}

	public override bool AllowMovement()
	{
		return true;
	}

	public override bool AllowRespawning()
	{
		return true;
	}
}
