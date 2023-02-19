namespace PotatoWall.Utils;

public class ThreadSafeObservableCollection<T> : ObservableCollection<T>
{
    private delegate void SetItemCallback(int index, T item);

    private delegate void RemoveItemCallback(int index);

    private delegate void ClearItemsCallback();

    private delegate void InsertItemCallback(int index, T item);

    private delegate void MoveItemCallback(int oldIndex, int newIndex);

    public static volatile bool ModifyIPList = true;

    public Dispatcher Dispatcher;

    public ThreadSafeObservableCollection(Dispatcher dispatcher)
    {
        Dispatcher = dispatcher;
    }

    public ThreadSafeObservableCollection() : base()
    {
        Dispatcher = Dispatcher.CurrentDispatcher;
    }

    public ThreadSafeObservableCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    protected override void SetItem(int index, T item)
    {
        try
        {
            ModifyIPList = false;
            if (Dispatcher.CheckAccess())
            {
                base.SetItem(index, item);
            }
            else
            {
                _ = Dispatcher.Invoke(DispatcherPriority.Send, new SetItemCallback(SetItem), index, new object[] { item });
            }
        }
        finally
        {
            ModifyIPList = true;
        }
    }

    protected override void InsertItem(int index, T item)
    {
        try
        {
            ModifyIPList = false;
            if (Dispatcher == null)
            {
                base.InsertItem(index, item);
            }
            else if (Dispatcher.CheckAccess())
            {
                base.InsertItem(index, item);
            }
            else
            {
                _ = Dispatcher.Invoke(DispatcherPriority.Send, new InsertItemCallback(InsertItem), index, new object[] { item });
            }
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Debug(ex, "InsertItem: ");
        }
        finally
        {
            ModifyIPList = true;
        }
    }

    protected override void MoveItem(int oldIndex, int newIndex)
    {
        if (Dispatcher.CheckAccess())
        {
            base.MoveItem(oldIndex, newIndex);
        }
        else
        {
            _ = Dispatcher.Invoke(DispatcherPriority.Send, new MoveItemCallback(MoveItem), oldIndex, new object[] { newIndex });
        }
    }

    protected override void RemoveItem(int index)
    {
        if (ModifyIPList)
        {
            if (Dispatcher.CheckAccess())
            {
                base.RemoveItem(index);
            }
            else
            {
                _ = Dispatcher.Invoke(DispatcherPriority.Send, new RemoveItemCallback(RemoveItem), index);
            }
        }
    }

    protected override void ClearItems()
    {
        if (ModifyIPList)
        {
            if (Dispatcher.CheckAccess())
            {
                base.ClearItems();
            }
            else
            {
                _ = Dispatcher.Invoke(DispatcherPriority.Send, new ClearItemsCallback(ClearItems));
            }
        }
    }

    public new bool Contains(T item)
    {
        for (int i = 0; i < Count; i++)
        {
            if (Items[i].Equals(item))
            {
                return true;
            }
        }
        return false;
    }

    public bool AddContains(T item)
    {
        if (!base.Contains(item))
        {
            Add(item);
            return false;
        }

        return true;
    }
}