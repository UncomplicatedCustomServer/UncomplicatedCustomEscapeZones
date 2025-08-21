using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UncomplicatedEscapeZones.Extensions;

public static class DictionaryExtension
{
    public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey Key, TValue value)
    {
        if (dictionary is null)
            throw new ArgumentNullException(nameof(dictionary));

        dictionary[Key] = value;
    }
    
    public static ConcurrentDictionary<TKey, TValue> Clone<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary)
    {
        ConcurrentDictionary<TKey, TValue> newDictionary = new();

        foreach (KeyValuePair<TKey, TValue> kvp in dictionary)
            newDictionary[kvp.Key] =  kvp.Value;

        return newDictionary;
    }
}