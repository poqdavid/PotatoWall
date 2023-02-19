namespace PotatoWall.Extensions;

public static class DictionaryExtensions
{
    public static TValue Pop<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    {
        if (dictionary.TryGetValue(key, out TValue temp))
        {
            dictionary.Remove(key);
            return temp;
        }
        else
        {
            throw new KeyNotFoundException($"Key {key} not found in dictionary.");
        }
    }
}