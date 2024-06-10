using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace System.Collections.Generic {

public static class Dictionary {
    public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultReturn = default(TValue)) { 
        if (dict.ContainsKey(key))
            return dict[key];
        else
            return defaultReturn;
    }
        
    public static void Set<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value) {
        if (dict.ContainsKey(key)) {
            dict[key] = value;
        } else {
            dict.Add(key, value);
        }
    }
}

public static class List {
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list) {
        return (list == null) || (!list.Any());
    }

    public static T Random<T>(this List<T> values) {
        int index = UnityEngine.Random.Range(0, values.Count);
        return values[index];
    }

    public static List<T> Random<T>(this List<T> values, int count, bool repeat = true) {
        List<T> result = new List<T>();
        if ((!repeat) && (count >= values.Count))
            return values.ToList();

        while (result.Count != count) {
            T rng = values.Random();
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
        return list.Update(list.IndexOf(oldItem), newItem);
    }

    public static IList<T> Update<T>(this IList<T> list, int oldIndex, T newItem) {
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
