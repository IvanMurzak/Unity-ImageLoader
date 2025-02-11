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
    }
}