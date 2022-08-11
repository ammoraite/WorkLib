using System.Collections;

using static System.Net.WebRequestMethods;

namespace AmmoraiteLib
{
    public sealed class ConcurrentList<T> : IEnumerable<T>
{
        
        private object _updatelocker = new ( );

        private DelegateQueue<Delegate> ConcurrentListDelegateQueue = new (10);
        private T[] Items { get; set; }

        private int _count;
        public ref readonly int Count => ref _count;

        private int _capacity;
        public ref readonly int Capacity => ref _capacity;
        public T this[int index]
        {
            get
            {
                lock (_updatelocker)
                {
                    CheckOutOfRangeValueIndex (ref index);
                    return Items[index];
                }
            }

            set
            {
                lock (_updatelocker)
                {
                    lock (Items[index])
                    {
                        CheckOutOfRangeValueIndex (ref index);
                        Items[index]=value;
                    }

                }

            }
        }


        #region Constructor

        public ConcurrentList ( ConcurrentList<T> items ) => AddSeveralFromConcurrentList (items);
        public ConcurrentList ( ) => Items=new T[Capacity];

        #endregion

        #region PrivateMetods
        private void CheckOutOfRangeValueIndex ( ref int index )
        {
            if (index>_count-1||index<0)
            {
                throw new IndexOutOfRangeException ( );
            }

        }

        /// <summary>
        /// Увеличивает размер массива T Элементов
        /// </summary>
        private void UpSizeItemsArray ( ref int newSize )
        {
            lock (_updatelocker)
            {
                T[] temporaryItemsArray = new T[newSize];

                for (int i = 0; i<Items.Length; i++)
                {
                    temporaryItemsArray[i]=Items[i];
                }

                Items=temporaryItemsArray;
            }

        }
        #endregion

        #region AddMetods

        #region Additem

        /// <summary>
        /// Добавляет элемент в текуущую коллекцию 
        /// </summary>
        /// <param name="item"></param>
        public void Add ( T item ) =>
            ConcurrentListDelegateQueue.AddDelegateAndExecute (( ) => AddItem (item));
        private void AddItem ( T item )

        {
            lock (_updatelocker)
            {
                if (_capacity==0)
                {
                    _capacity=4;
                    UpSizeItemsArray (ref _capacity);
                    Add (item);
                }
                else if (_count<_capacity)
                {
                    Items[_count]=item;
                    _count++;
                }
                else
                {
                    _capacity*=2;
                    UpSizeItemsArray (ref _capacity);
                    Add (item);
                }
            }
        }
        #endregion

        #region AddSeveralItems

        /// <summary>
        /// Добавляет в текуущую коллекцию элементы из массива "itemsToAdd" начиная с "index"(по умолчанию index=0)
        /// </summary>
        /// <param name="itemsToAdd">Массив элементы которого будут добавлены начиная с index</param>
        /// <param name="index">индекс с которого будут добавлены элементы (по умолчанию 0)</param>
        public void AddSeveralFromConcurrentList ( ConcurrentList<T> itemsToAdd, int index = 0 ) =>
         ConcurrentListDelegateQueue.AddDelegateAndExecute (( ) => AddSeveralItemFromConcurrentList (itemsToAdd, index));
        private void AddSeveralItemFromConcurrentList ( ConcurrentList<T> itemsToAdd, int index = 0 )

        {
            lock (_updatelocker)
            {
                for (int i = index; i<itemsToAdd._count; i++)
                {
                    Add (itemsToAdd[i]);
                }
            }
        }


        #endregion

        #endregion

        #region RemoveMetods

        #region RemoveAllEquals

        /// <summary>
        /// Удаляет все входящие в колекцию элементы эквивалентные item
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public void RemoveAllEquals ( params T[] items ) => ConcurrentListDelegateQueue.AddDelegateAndExecute (( ) => RemoveAllEqualsItems (items));
        private void RemoveAllEqualsItems ( params T[] items )
        {
            lock (_updatelocker)
            {

                for (int i = 0; i<items.Length; i++)
                {
                    for (int j = 0; j<_count; j++)
                    {
                        if (Items[j].Equals (items[i]))
                        {
                            RemoveOnIndex (j);
                        }
                    }
                }
            }
        }

        #endregion

        #region Clear
        /// <summary>
        /// Удаляет все элементы из колекции
        /// </summary>
        public void Clear ( ) =>
            ConcurrentListDelegateQueue.AddDelegateAndExecute (( ) => ClearCollectionns ( ));
        private void ClearCollectionns ( )
        {
            lock (_updatelocker)
            {
                try
                {
                    Items=new T[Capacity];
                }
                catch (Exception e)
                {
                    Console.WriteLine (e);
                    throw;
                }
            }
        }

        #endregion

        #region RemoveOnIndex
        public void RemoveOnIndex ( int index ) =>
            ConcurrentListDelegateQueue.AddDelegateAndExecute (( ) => RemoveItemOnIndex (index));
        private void RemoveItemOnIndex ( int index )
        {
            lock (_updatelocker)
            {
                lock (Items[index])
                {
                    CheckOutOfRangeValueIndex (ref index);

                    if (_count==1)
                    {
                        Items[0]=default;
                    }

                    else if (_count-1!=index)
                    {
                        for (int j = index; j<_count; j++)
                        {

                            if (Items[j]!=null&&j+1<=_count)
                            {
                                Items[j]=Items[j+1];
                            }
                            else
                            {
                                Items[j]=default;
                                break;
                            }
                        }
                    }
                    else
                    {
                        Items[index]=default;
                    }
                    _count--;
                }
            }
        }
        #endregion

        #endregion

        #region Sort/FindMetod

        #region GetIndex
        public IEnumerable<int> GetIndex ( Predicate<T> predicate )
        {
            lock (_updatelocker)
            {
                for (int i = 0; i<_count; i++)
                {
                    if (predicate.Invoke (this[i]))
                    {
                        yield return i;
                    }
                }
            }

        }
        #endregion

        #region Contains
        public bool Contains ( Predicate<T> predicate )
        {
            lock (_updatelocker)
            {
                foreach (var item in Items)
                {
                    if (predicate.Invoke (item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Sort
        public void Sort ( ) =>
            ConcurrentListDelegateQueue.AddDelegateAndExecute (( ) => SortItems ( ));
        private void SortItems ( )
        {
            lock (_updatelocker)
            {
                Array.Sort (Items);
            }
        }
        #endregion

        #endregion

        #region IEnumerable<T>
        public IEnumerator<T> GetEnumerator ( )
        {
            lock (_updatelocker)
            {
                var a = Items.Take (_count).Where (x => x!=null).GetEnumerator ( );
                return a;
            }
        }
        IEnumerator IEnumerable.GetEnumerator ( )
        {
            return GetEnumerator ( );
        }
        #endregion
    }
}