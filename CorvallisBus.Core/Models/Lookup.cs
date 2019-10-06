using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CorvallisBus.Core.Models
{
    public abstract class Lookup<TKey, TValue> : IDictionary<TKey, TValue>
    {
        protected readonly IDictionary<TKey, TValue> _dict;
        protected Lookup(IDictionary<TKey, TValue> dict) => _dict = dict;

        TValue IDictionary<TKey, TValue>.this[TKey key] { get => _dict[key]; set => _dict[key] = value; }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _dict.Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => _dict.Values;

        int ICollection<KeyValuePair<TKey, TValue>>.Count => _dict.Count;

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => _dict.IsReadOnly;

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => _dict.Add(key, value);

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => _dict.Add(item);

        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => _dict.Clear();

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => _dict.Contains(item);

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => _dict.ContainsKey(key);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => _dict.CopyTo(array, arrayIndex);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => _dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();

        bool IDictionary<TKey, TValue>.Remove(TKey key) => _dict.Remove(key);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => _dict.Remove(item);

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => _dict.TryGetValue(key, out value);
    }

    public static class LookupExtension
    {
        /// <summary>
        /// Gets the value for the key if present. Otherwise gets an empty collection.
        /// Only usable when <typeparamref name="TValue"/> is a collection type.
        /// </summary>
        /// <returns></returns>
        public static TValue GetOrEmpty<TKey, TValue>(this Lookup<TKey, TValue> lookup, TKey key) where TValue : IEnumerable, new()
        {
            return ((IDictionary<TKey, TValue>)lookup).TryGetValue(key, out var value) ? value : new TValue();
        }
    }
}
