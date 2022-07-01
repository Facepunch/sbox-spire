namespace Spire.Buffs;

public enum BuffType
{
	Buff,
	Debuff
}

public abstract partial class Buff : BaseNetworkable
{
	public Buff()
	{
		UntilDestroy = Duration;
		NextTick = TickImmediate ? 0 : TickInterval;
	}

	public TimeSince SinceCreation { get; set; } = 0;

	public virtual BuffType Type => BuffType.Buff;

	public virtual string Name => "";
	public virtual string Description => "";

	/// <summary>
	/// How long this buff should last for
	/// </summary>
	public virtual float Duration => 0f;

	/// <summary>
	// The icon path for UI
	/// </summary>
	public virtual string IconPath => "";

	/// <summary>
	/// How often a tick is for this buff
	/// </summary>
	public virtual float TickInterval => 0f;

	public virtual bool TickImmediate => true;

	public TimeUntil UntilDestroy { get; set; }

	public TimeUntil NextTick { get; set; }

	protected Action<BaseCharacter> TickAction { get; set; }
	protected Action<BaseCharacter> DestroyAction { get; set; }

	public virtual void OnBegin()
	{
	}

	public Buff WithTickAction( Action<BaseCharacter> action )
	{
		TickAction += action;
		return this;
	}

	public Buff WithDestroyAction( Action<BaseCharacter> action )
	{
		DestroyAction += action;
		return this;
	}

	public virtual void OnTick( BaseCharacter character )
	{
		TickAction?.Invoke( character );
	}

	public virtual void OnDestroy( BaseCharacter character )
	{
		DestroyAction?.Invoke( character );
	}
}
