using Spire.Abilities;

namespace Spire;

public partial class PlayerCharacter
{
	[Net]
	public PlayerAbility MovementAbility { get; set; }
	[Net]
	public PlayerAbility FirstAbility { get; set; }
	[Net]
	public PlayerAbility SecondAbility { get; set; }
	[Net]
	public PlayerAbility UltimateAbility { get; set; }

	public Ability InteractingAbility { get; set; }

	public IEnumerable<PlayerAbility> GetPlayerAbilities()
	{
		yield return MovementAbility;
		yield return FirstAbility;
		yield return SecondAbility;
		yield return UltimateAbility;
	}

	public PlayerAbility GetAbilityFromSlot( int slot )
	{
		var abilities = GetPlayerAbilities().ToList();
		return abilities[slot];
	}

	[ClientRpc]
	protected void RpcKillInteractions()
	{
		Host.AssertClient();

		InteractingAbility = null;

		// Inform the ability interaction system to kill all guides.
		AbilityInteraction.KillGuides();
	}

	public TimeSince LastAbilityUsed { get; set; } = 1f;
	public float GlobalAbilityCooldown => 1f;
	public bool CanUseAbility() => LastAbilityUsed > GlobalAbilityCooldown;

	public TimeSince LastInteractionUsed { get; set; } = 1f;
	public float GlobalInteractionCooldown => 1f;
	public bool CanUseAbilityInteract() => LastInteractionUsed > GlobalInteractionCooldown;

	public void SimulateAbilities( Client cl )
	{
		var canUseAbilities = CanUseAbility();

		if ( canUseAbilities && InteractingAbility.IsValid() )
			InteractingAbility.Interaction.OnTick();

		using ( LagCompensation() )
		{
			int i = 0;
			foreach ( var ability in GetPlayerAbilities() )
			{
				if ( canUseAbilities && Input.Down( PlayerAbility.GetInputButtonFromSlot( i ) ) )
				{
					if ( ability is not null && !InteractingAbility.IsValid() )
					{
						if ( ability.CanRun() )
							ability.Interact();
					}
				}

				ability?.Simulate( cl );

				i++;
			}
		}
	}
}
