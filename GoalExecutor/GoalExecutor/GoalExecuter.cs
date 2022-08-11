using Microsoft.Extensions.Logging;

namespace AmmoraiteLib;
public sealed class GoalExecuter
{
    private sealed class GoalFromCortege : IGoal
    {
        private object[]? _objects;
        private Task _task;
        public Task task
        {
            get
            {
                if (_task is null)
                {
                    throw new NullReferenceException (nameof (_objects));
                }
                return _task;
            }
            set => _task=value;
        }
        public object[] Objectparametr
        {
            get
            {
                if (_objects is null)
                {
                    throw new NullReferenceException (nameof (_objects));
                }
                return _objects;
            }
            set => _objects=value;
        }
        public int Priority { get; set; }
    }
    private ILogger? Logger { get; set; }

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
        return Task.Run (( ) =>
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
        return Task.Run (( ) =>
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
    public Task<bool> AddWork ( params (Action, object[], int)[] goals )
    {
        return Task.Run (( ) =>
        {
            try
            {
                foreach (var goal in goals)
                {
                    _delegateToWork.Add (new GoalFromCortege{ task=new Task(goal.Item1), Objectparametr=goal.Item2, Priority=goal.Item3 });
                }
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        });
    }

    public async Task Execute ( )
    {
       
            foreach (var item in _delegateToWork.OrderBy (x => x.Priority))
            {
                try
                {
                     await item.task;

                    _compleateActions.Add (item);

                    if (Logger is not null)
                    {
                        Logger.LogInformation (message: "Задача {item.task} выполнена", item.task);
                    }
                    else
                    {
                        Console.WriteLine ($"Задача {item.task} выполнена");
                    }
                }
                catch (Exception e)
                {
                    _unfulfilledActions.Add (item);


                    if (Logger is not null)
                    {
                        Logger.LogInformation (e, "Задача {item.task} не выполнена так как:{item.task.Exception}", item.task, item.task.Exception);
                    }
                    else
                    {
                        Console.WriteLine ($"Задача {item.task} не выполнена так как:{item.task.Exception}");
                    }
                }

            }
    }
    public async Task Execute ( IEnumerable<IGoal> tuples )
    {

        foreach (var item in tuples)
        {
            try
            {               
                await item.task;
               
                _compleateActions.Add (item);

                if (Logger is not null)
                {
                    Logger.LogInformation (message: "Задача {item.task} выполнена", item.task);
                }
                else
                {
                    Console.WriteLine ("Задача {item.task} выполнена");
                }
            }
            catch (Exception e)
            {
                _unfulfilledActions.Add (item);

                if (Logger is not null)
                {

                    Logger.LogInformation (e, "Задача {item.task}"+
                        " не выполнена так как:{e.Message}", item.task, item.task.Exception);
                }
                else
                {
                    Console.WriteLine ($"Задача {item.task} не выполнена так как:{item.task.Exception}");
                }
            }
        }
    }
}
