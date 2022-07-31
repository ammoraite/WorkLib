namespace AmmoraiteLib;
public abstract class AbstractGoal<T> : IGoal
{
    public Delegate Action { get; set; }
    public int Priority { get; set; }
    public object[] Objectparametr { get; set; }
    public AbstractGoal ( int priority, params T[] values )
    {
        this.Priority=priority;
        Objectparametr=SetObjectparametr (values)??throw new NullReferenceException (nameof (Objectparametr));
        Action=SetDelegate ( )??throw new NullReferenceException (nameof (Action));
    }
    public abstract object[] SetObjectparametr ( T[] values );
    public abstract Delegate SetDelegate ( );
}