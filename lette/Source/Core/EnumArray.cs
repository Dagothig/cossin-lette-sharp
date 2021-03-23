using System;
using System.Linq;
using System.Collections.Generic;

public struct EnumArray<K, V> where K : struct, IConvertible
{
    V[] Values;

    public static EnumArray<K, V> New() => new EnumArray<K, V>()
    {
        Values = new V[Enum.GetValues(typeof(K)).Length]
    };

    public IEnumerable<K> Keys =>
        Enumerable.Range(0, Values.Length).Select(k => (K)(object)k);

    public IEnumerable<(K, V)> Entries =>
        Values.Select((v, k) => ((K)(object)k, v));

    public V this[K k]
    {
        get => Values[k.ToInt32(null)];
        set => Values[k.ToInt32(null)] = value;
    }
}
