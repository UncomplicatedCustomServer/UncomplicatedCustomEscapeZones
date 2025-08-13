using System;
using System.Collections.Generic;

namespace UncomplicatedEscapeZones.Utilities;

public static class DictionaryExtension
{
    public static void TryAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey Key, TValue value)
    {
        if (dictionary is null)
            throw new ArgumentNullException(nameof(dictionary));

        if (dictionary.ContainsKey(Key))
            dictionary[Key] = value;
        else
            dictionary.Add(Key, value);
    }
}