using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using Lette.Core;

public struct EnumArray<K, V>: IEnumerable<(K, V)> where K : struct, IConvertible
{
    V[] Values;

    public static EnumArray<K, V> New() => new EnumArray<K, V>()
    {
        Values = new V[Enum<K>.Values.Length]
    };

    public static EnumArray<K, V> New(params (K, V)[] entries)
    {
        var arr = New();
        foreach (var (k, v) in entries)
            arr[k] = v;
        return arr;
    }

    public IEnumerable<K> Keys =>
        Enumerable.Range(0, Values.Length).Select(k => (K)(object)k);

    public IEnumerable<(K, V)> Entries =>
        Values.Select((v, k) => ((K)(object)k, v));

    public IEnumerator<(K, V)> GetEnumerator() =>
        Entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public V this[K k]
    {
        get => Values[k.ToInt32(null)];
        set => Values[k.ToInt32(null)] = value;
    }
}
