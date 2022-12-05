using Spire.Buffs;

namespace Spire;

public partial class BaseCharacter
{
	[Net, Predicted]
	public IList<Buff> Buffs { get; set; }

	public Buff AddBuff<T>() where T : Buff, new()
	{
		Log.State( $"AddBuff {Time.Now}" );

		var buff = new T();

		Buffs.Add( buff );

		return buff;
	}

	protected void SimulateBuffs( Client cl )
	{
		for ( var i = Buffs.Count - 1; i >= 0; i-- )
		{
			var buff = Buffs[i];

			// Log.State( $"NextTick = {buff.NextTick}" );

			if ( buff.NextTick <= 0 )
			{
				buff.OnTick( this );
				buff.NextTick = buff.TickInterval;
			}

			// @TODO: This is a bit of a hack, Untildestroy gets assigned to some random garbo value.
			if ( buff.UntilDestroy <= 0 && buff.SinceCreation >= 1f )
			{
				buff.OnDestroy( this );
				Buffs.RemoveAt( i );
			}
		}
	}
}
