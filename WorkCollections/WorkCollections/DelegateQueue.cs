namespace AmmoraiteLib
{
    public sealed class DelegateQueue<T> where T : Delegate
    {
        private Queue<T> _delegates { get; set; }
        public int Size { get { lock (_lock) { return _delegates.Count; } } }
        public bool StopWork { get; set; } = false;

        private object _lock = new object ( );
        public DelegateQueue ( int size )
        {
            _delegates=new Queue<T> (size);
        }

        public void Clear ( )
        {
            lock (_lock)
            {
                StopWork=true;
                _delegates.Clear ( );
                StopWork=false;
            }
        }
        public void Execute ( )
        {
            lock (_lock)
            {
                while (Size>=1)
                {
                    if (Size>=1&&_delegates.Peek ( )!=null)
                    {
                        _delegates.Dequeue ( ).DynamicInvoke ( );
                    }
                }
            }
        }

        public void AddDelegate ( T delegat_e )
        {
            if (!StopWork)
            {
                lock (_lock)
                {
                    _delegates.Enqueue (delegat_e);
                }
            }
        }

        public void AddDelegateAndExecute ( T delegat_e )
        {
            if (!StopWork)
            {
                lock (_lock)
                {
                    _delegates.Enqueue (delegat_e);
                    while (Size>=1)
                    {
                        if (Size>=1&&_delegates.Peek ( )!=null)
                        {
                            _delegates.Dequeue ( ).DynamicInvoke ( );
                        }
                    }
                }
            }
        }
    }
}
