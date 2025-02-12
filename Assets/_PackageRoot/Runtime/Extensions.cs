using System;
using System.Collections.Generic;

namespace Extensions.Unity.ImageLoader
{
    static class Extensions
    {
        public static void AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            lock (dictionary)
            {
                if (dictionary.TryGetValue(key, out var existingValue))
                {
                    dictionary[key] = updateValueFactory(key, existingValue);
                }
                else
                {
                    dictionary[key] = addValue;
                }
            }
        }
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default)
        {
            return dictionary.TryGetValue(key, out var value) ? value : defaultValue;
        }
        public static bool Remove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
        {
            if (dictionary.TryGetValue(key, out value))
            {
                dictionary.Remove(key);
                return true;
            }
            value = default;
            return false;
        }
    }
}