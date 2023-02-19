using System.Collections.Specialized;

namespace PotatoWall.Utils;

public class ThreadSafeObservableDictionary<TKey, TValue> : INotifyPropertyChanged, INotifyCollectionChanged
{
    private readonly Dictionary<TKey, TValue> _dictionary = new();

    private delegate bool TryGetValueCallback(TKey key, out TValue value);

    private delegate bool RemoveCallback(TKey key);

    private delegate void AddCallback(TKey key, TValue value);

    private delegate void ClearCallback();

    public Dispatcher _dispatcher;

    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add => PropertyChanged += value;
        remove => PropertyChanged -= value;
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public ThreadSafeObservableDictionary()
    {
        _dispatcher = Dispatcher.CurrentDispatcher;
    }

    public ThreadSafeObservableDictionary(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public TValue this[TKey key]
    {
        get
        {
            TValue value = default(TValue);
            _dispatcher.Invoke(() =>
            {
                value = _dictionary[key];
            });
            return value;
        }
        set
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    _dictionary[key] = value;
                }
                catch (Exception ex)
                {
                    PotatoWallClient.Logger.Error(ex, "Error setting value in dictionary for key {key}", key);
                    throw;
                }

                OnIndexerPropertyChanged();
                OnPropertyChanged(new PropertyChangedEventArgs(key.ToString()));
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, this[key]));
            }));
        }
    }

    public int Count
    {
        get
        {
            int count = 0;
            _dispatcher.Invoke(() =>
            {
                try
                {
                    count = _dictionary.Count;
                }
                catch (Exception ex)
                {
                    PotatoWallClient.Logger.Error(ex, "Error getting count from dictionary");
                    throw;
                }
            });
            return count;
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            ICollection<TKey> keys = null;
            _dispatcher.Invoke(() =>
            {
                try
                {
                    keys = _dictionary.Keys;
                }
                catch (Exception ex)
                {
                    PotatoWallClient.Logger.Error(ex, "Error getting keys from dictionary");
                    throw;
                }
            });
            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            ICollection<TValue> values = null;
            _dispatcher.Invoke(() =>
            {
                try
                {
                    values = _dictionary.Values;
                }
                catch (Exception ex)
                {
                    PotatoWallClient.Logger.Error(ex, "Error getting values from dictionary");
                    throw;
                }
            });
            return values;
        }
    }

    public void Add(TKey key, TValue value)
    {
        try
        {
            if (_dispatcher == null)
            {
                _dictionary.Add(key, value);

                OnValuesPropertyChanged();
                OnKeysPropertyChanged();
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();

                OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
            }
            else if (_dispatcher.CheckAccess())
            {
                _dictionary.Add(key, value);
                OnValuesPropertyChanged();
                OnKeysPropertyChanged();
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();

                OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
            }
            else
            {
                _ = _dispatcher.Invoke(DispatcherPriority.Send, new AddCallback(Add), key, value);
            }
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error adding key {key} and value {value} to dictionary", key, value);
            throw;
        }
    }

    public bool Remove(TKey key)
    {
        try
        {
            if (_dispatcher.CheckAccess())
            {
                if (!_dictionary.TryGetValue(key, out TValue value))
                {
                    return false;
                }
                _dictionary.Remove(key);

                OnValuesPropertyChanged();
                OnKeysPropertyChanged();
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();

                OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
            }
            else
            {
                _ = _dispatcher.Invoke(DispatcherPriority.Send, new RemoveCallback(Remove), key);
            }
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error removing key {key} from dictionary", key);
            throw;
        }

        return true;
    }

    public bool ContainsKey(TKey key)
    {
        bool contains;
        try
        {
            contains = _dictionary.ContainsKey(key);
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error checking if dictionary contains key {key}", key);
            throw;
        }

        return contains;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        bool tryGet = false;
        try
        {
            if (_dispatcher.CheckAccess())
            {
                tryGet = _dictionary.TryGetValue(key, out value);
            }
            else
            {
                //ref TValue _value = value;
                value = default;
                _ = _dispatcher.Invoke(DispatcherPriority.Send, new TryGetValueCallback(TryGetValue));
            }
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error getting value for key {key} from dictionary", key);
            throw;
        }

        return tryGet;
    }

    public void Clear()
    {
        try
        {
            if (_dispatcher.CheckAccess())
            {
                _dictionary.Clear();
                OnCountPropertyChanged();
                OnIndexerPropertyChanged();
                OnCollectionReset();
            }
            else
            {
                _ = _dispatcher.Invoke(DispatcherPriority.Send, new ClearCallback(Clear));
            }
        }
        catch (Exception ex)
        {
            PotatoWallClient.Logger.Error(ex, "Error clearing dictionary");
            throw;
        }
    }

    public bool AddContains(TKey key, TValue value)
    {
        bool contains = false;

        if (_dictionary.ContainsKey(key))
        {
            contains = true;
        }
        else
        {
            _dictionary.Add(key, value);
        }

        return contains;
    }

    public void AddOrUpdate(TKey key, TValue value)
    {
        if (AddContains(key, value))
        {
            try
            {
                _dictionary[key] = value;
            }
            catch (Exception ex)
            {
                PotatoWallClient.Logger.Debug(ex, "AddOrUpdate: ");
            }
        }
    }

    protected void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    protected event PropertyChangedEventHandler PropertyChanged;

    protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        NotifyCollectionChangedEventHandler handler = CollectionChanged;
        if (handler != null)
        {
            try
            {
                handler(this, e);
            }
            catch (Exception ex)
            {
                PotatoWallClient.Logger.Error(ex, "Error handling collection change");
                throw;
            }
        }
    }

    private void OnCountPropertyChanged() => OnPropertyChanged(EventArgsCache.CountPropertyChanged);

    private void OnIndexerPropertyChanged() => OnPropertyChanged(EventArgsCache.IndexerPropertyChanged);

    private void OnKeysPropertyChanged() => OnPropertyChanged(EventArgsCache.KeysPropertyChanged);

    private void OnValuesPropertyChanged() => OnPropertyChanged(EventArgsCache.ValuesPropertyChanged);

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> keyValuePair)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, keyValuePair));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> oldItem, KeyValuePair<TKey, TValue> newItem)
    {
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }

    private void OnCollectionReset() => OnCollectionChanged(EventArgsCache.ResetCollectionChanged);
}

internal static class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs KeysPropertyChanged = new("Keys");
    internal static readonly PropertyChangedEventArgs ValuesPropertyChanged = new("Values");
    internal static readonly PropertyChangedEventArgs CountPropertyChanged = new("Count");
    internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new("Item[]");
    internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged = new(NotifyCollectionChangedAction.Reset);
}