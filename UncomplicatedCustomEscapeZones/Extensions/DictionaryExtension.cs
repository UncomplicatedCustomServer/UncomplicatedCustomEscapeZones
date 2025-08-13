using System;
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
}