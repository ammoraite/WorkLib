﻿namespace AmmoraiteLib;
public abstract class AbstractGoal<T> : IGoal
{
    public Task task { get; set; }
    public int Priority { get; set; }
    public object[] Objectparametr { get; set; }
    public AbstractGoal ( int priority, params T[] values )
    {
        Priority=priority;
        Objectparametr=SetObjectparametr (values)??throw new NullReferenceException (nameof (Objectparametr));
        task=GetTask ( )??throw new NullReferenceException (nameof (Action));
    }
    public abstract object[] SetObjectparametr ( T[] values );
    public abstract Task GetTask ( );
}