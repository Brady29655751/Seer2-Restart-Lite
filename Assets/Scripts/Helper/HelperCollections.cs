using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.Collections.Generic {

public static class DictionaryHelper {

    public static bool IsNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dict) {
        return (dict == null) || (dict.Count == 0);
    }

    public static bool TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, out TValue value, TValue defaultReturn = default(TValue)) {
        bool isKeyExist = dict.ContainsKey(key);
        if (isKeyExist)
            value = dict[key];
        else
            value = defaultReturn;

        return isKeyExist;
    }

    public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultReturn = default(TValue)) { 
        dict.TryGet(key, out var value, defaultReturn);
        return value;
        /*
        if (dict.ContainsKey(key))
            return dict[key];
        else
            return defaultReturn;
        */
    }
        
    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value) {
        dict[key] = value;
        /*
        if (dict.ContainsKey(key)) {
            dict[key] = value;
        } else {
            dict.Add(key, value);
        }
        */
    }

    public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> dict, IDictionary<TKey, TValue> rhs) {
        foreach (var item in rhs)
            dict.Set(item.Key, item.Value);
    }
}

public static class ListHelper {
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list) {
        return (list == null) || (!list.Any());
    }

    public static List<T> SingleToList<T>(this T item) => new List<T>(){ item };

    public static void ForEach<T>(this IList<T> list, Action<T, int> action) {
        if (action == null)
            return;

        for (int i = 0; i < list.Count; i++) {
            int copy = i;
            action.Invoke(list[copy], copy);
        }
    }

    public static T Get<T>(this IList<T> list, int index, T defaultValue = default(T)) {
        if (ListHelper.IsNullOrEmpty(list))
            return defaultValue;

        if (!index.IsInRange(0, list.Count))
            return defaultValue;

        return list[index];
    }

    public static void Set<T>(this IList<T> list, int index, T value) {
        if (list == null)
            return;
            
        if (!index.IsInRange(0, list.Count)) {
            list.Add(value);
            return;
        }
            
        list[index] = value;
    }

    public static T Random<T>(this List<T> values, List<int> weights = null) {
        if (weights == null) {
            int index = UnityEngine.Random.Range(0, values.Count);
            return values[index];
        }

        var w = weights.Take(values.Count).ToList();
        var p = UnityEngine.Random.Range(0, w.Sum());
        for (int i = 0; i < w.Count; i++) {
            if (p < w[i])
                return values[i];

            p -= w[i];
        }
        return values.LastOrDefault();
    }

    public static List<T> Random<T>(this List<T> values, int count, bool repeat = true, List<int> weights = null) {
        List<T> result = new List<T>();
        if ((!repeat) && (count > values.Count))
            return values.ToList();

        while (result.Count != count) {
            T rng = values.Random(weights);
            if (!repeat && result.Contains(rng))
                continue;

            result.Add(rng);
        }
        return result;
    }

    public static int IndexOf<T>(this IEnumerable<T> values, T val) {
        var idx = values.AllIndexOf(val);
        return idx.Length == 0 ? -1 : idx.Min();
    }

    public static int FindIndex<T>(this IEnumerable<T> values, Predicate<T> pred) {
        var idx = values.FindAllIndex(pred);
        return idx.Length == 0 ? -1 : idx.Min();
    }

    public static int[] AllIndexOf<T>(this IEnumerable<T> values, T val) {
        Predicate<T> equal = x => object.Equals(x, val);
        return values.FindAllIndex(equal);
    }

    public static int[] FindAllIndex<T>(this IEnumerable<T> values, Predicate<T> pred) {
        return values.Select((x, i) => pred(x) ? i : -1).Where(i => i != -1).ToArray();
    }
    
    public static void Swap<T>(this IList<T> list, int posA, int posB) {
        T value = list[posA];
        list[posA] = list[posB];
        list[posB] = value;
    }

    public static IList<T> Update<T>(this IList<T> list, T oldItem, T newItem) {
        return list.UpdateAt(list.IndexOf(oldItem), newItem);
    }

    public static IList<T> UpdateAt<T>(this IList<T> list, int oldIndex, T newItem) {
        if (!oldIndex.IsInRange(0, list.Count))
            return list;
        
        list[oldIndex] = newItem;
        return list;
    }
    
    // exclude stop
    public static List<T> GetRange<T>(this IList<T> list, int start, int stop) {
        List<T> ret = new List<T>();
        start = Mathf.Max(0, start); 
        stop = Mathf.Min(stop, list.Count);
        for(int i = start; i < stop; i++) {
            ret.Add(list[i]);
        }
        return ret;
    }
    
    public static void RemoveAt<T>(this IList<T> lhs, int index) {
        lhs.RemoveAt(index);
    }
    
    public static void RemoveRange<T>(this IList<T> lhs, IList<T> rhs) {
        for(int i = 0; i < rhs.Count; i++) {
            lhs.Remove(rhs[i]);
        }
    }

    // exclude stop
    public static IList<T> MoveRangeTo<T>(this IList<T> list, int start, int stop, int targetStart) {
        int count = stop - start;
        int targetStop = Mathf.Min(list.Count, targetStart + count);
        IList<T> range = list.GetRange(start, stop);
        for (int i = targetStart; i < targetStop; i++) {
            list[i] = range[i - targetStart];
        }
        return list;
    }

    public static void MoveRangeToOtherList<T>(this IList<T> lhs, List<T> rhs, int start, int stop) {
        List<T> list = lhs.GetRange(start, stop);
        lhs.RemoveRange(list);
        rhs.AddRange(list);
    }

    public static void GetModifiedList<T>(this IList<T> current, IList<T> target, ModifyOption option = ModifyOption.Set) {
        if (option == ModifyOption.Add) {
            foreach (T item in target) {
                current.Add(item);
            }
        } else if (option == ModifyOption.Remove) {
            foreach (T item in target) {
                current.Remove(item);
            }
        } else {
            current = target;
        }
    
    }
}

}
