using Microsoft.Extensions.Logging;

namespace AmmoraiteLib;
public sealed class GoalExecuter
{
    private sealed class GoalFromCortege : IGoal
    {
        public Delegate Action { get; set; }
        public object[] Objectparametr { get; set; }
        public int Priority { get; set; }
    }
    private ILogger? Logger { get; set; }

    public delegate void MethodContainer ( );

    public event MethodContainer? Compleate;

    /// <summary>
    /// Список делегатов  готовых к выполнению в соответствии с приорететом
    /// в качестве параметров выступает object[]
    /// </summary>
    private ConcurrentList<IGoal> _delegateToWork = new ( );
    public ref readonly ConcurrentList<IGoal> DelegateToWork => ref _delegateToWork;

    /// <summary>
    /// Список успешно выпоненых делегатов  
    /// </summary>
    private ConcurrentList<IGoal> _compleateActions = new ( );
    public ref readonly ConcurrentList<IGoal> CompleateActions => ref _compleateActions;

    /// <summary>
    /// Список  делегатов выполненых с исключениями
    /// </summary>
    private ConcurrentList<IGoal> _unfulfilledActions = new ( );
    public ref readonly ConcurrentList<IGoal> UnfulfilledActions => ref _unfulfilledActions;

    #region Constructor
    public GoalExecuter ( ) { }
    public GoalExecuter ( ILogger logger ) => Logger=logger;
    #endregion

    public Task<bool> AddWork ( params IGoal[] goals )
    {
        return new Task<bool> (( ) =>
        {
            try
            {
                foreach (var goal in goals)
                {
                    _delegateToWork.Add (goal);
                }
            }
            catch (Exception)
            {
                throw;
            }           
            return true;
        });     
    }
    public Task<bool> AddWork ( IEnumerable<IGoal> goals )
    {
        return new Task<bool> (( ) =>
        {
            try
            {
                foreach (var goal in goals)
                {
                    _delegateToWork.Add (goal);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        });
       
    }
    public Task<bool> AddWork ( params (Delegate, object[], int)[] goals )
    {
        return new Task<bool> (( ) =>
        {
            try
            {
                foreach (var goal in goals)
                {
                    _delegateToWork.Add (new GoalFromCortege { Action=goal.Item1, Objectparametr=goal.Item2, Priority=goal.Item3 });
                }
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        });
        
    }

    public Task<ConcurrentList<object>> Execute ( )
    {

        return new Task<ConcurrentList<object>> (( ) =>
        {
            ConcurrentList<object> results = new ( );
            foreach (var item in _delegateToWork.OrderBy (x => x.Priority))
            {
                try
                {
                    if (item is GoalFromCortege)
                    {
                        results.Add (item.Action.DynamicInvoke (item.Objectparametr));
                    }
                    else
                    {
                        results.Add (item.Action.DynamicInvoke ( ));
                    }
                    _compleateActions.Add (item);

                    if (Logger is not null)
                    {
                        Logger.LogInformation (message: "Задача {item.Action.Method.Name} выполнена", item.Action.Method.Name);
                    }
                    else
                    {
                        Console.WriteLine ($"Задача {item.Action.Method.Name} выполнена");
                    }
                }
                catch (Exception e)
                {
                    _unfulfilledActions.Add (item);


                    if (Logger is not null)
                    {
                        Logger.LogInformation (e,"Задача {item.Action.Method.Name} не выполнена так как:{e.Message}", item.Action.Method.Name, e.Message);
                    }
                    else
                    {
                        Console.WriteLine ($"Задача {item.Action.Method.Name} не выполнена так как:{e.Message}");
                    }
                }

            }
            if (Compleate is not null)
            {
                Compleate.Invoke ( );
            }
            return results;
        });

    }
    public Task<ConcurrentList<object>> Execute ( IEnumerable<IGoal> tuples )
    {
        return new Task<ConcurrentList<object>> (( ) =>
        {
            ConcurrentList<object> results = new ( );
            foreach (var item in tuples)
            {
                try
                {
                    if (item is GoalFromCortege)
                    {
                        results.Add(item.Action.DynamicInvoke (item.Objectparametr));
                    }
                    else
                    {
                        results.Add (item.Action.DynamicInvoke ( ));
                    }
                        _compleateActions.Add (item);

                    if (Logger is not null)
                    {
                        Logger.LogInformation (message: "Задача {item.Item1.Method.Name} выполнена", item.Action.Method.Name);
                    }
                    else
                    {
                        Console.WriteLine ("Задача {item.Item1.Method.Name} выполнена");
                    }
                }
                catch (Exception e)
                {
                    _unfulfilledActions.Add (item);

                    if (Logger is not null)
                    {

                        Logger.LogInformation (e, "Задача {item.Item1.Method.Name}"+
                            " не выполнена так как:{e.Message}", item.Action.Method.Name, e.Message);
                    }
                    else
                    {
                        Console.WriteLine ($"Задача {item.Action.Method.Name} не выполнена так как:{e.Message}");
                    }
                }
            }
            if (Compleate is not null)
            {
                Compleate.Invoke ( );
            }
            return results;
        });
    }
}
