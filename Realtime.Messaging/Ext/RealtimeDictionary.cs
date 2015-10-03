using System;
using System.Collections.Generic;

namespace RealtimeFramework.Messaging.Ext
{
    /// <summary>
    /// PCL Concurrent Dictionary
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class RealtimeDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        #region FIELDS

        private readonly object locker = new object();

        #endregion

        #region METHODS

        public TValue AddOrUpdate(
            TKey key,
            Func<TKey, TValue> addValueFactory,
            Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue value;
            lock (this.locker)
            {
                if (ContainsKey(key))
                {
                    value = updateValueFactory(key, this[key]);
                    this[key] = value;
                }
                else
                {
                    value = addValueFactory(key);
                    Add(key, value);
                }
            }
            return value;
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue value;
            lock (this.locker)
            {
                if (ContainsKey(key))
                {
                    value = this[key];
                }
                else
                {
                    value = valueFactory(key);
                    Add(key, value);
                }
            }
            return value;
        }

        public TValue GetOrAdd(TKey key, TValue value)
        {
            lock (this.locker)
            {
                if (ContainsKey(key))
                {
                    return this[key];
                }

                Add(key, value);
                return value;
            }
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            lock (this.locker)
            {
                if (ContainsKey(key))
                {
                    value = this[key];
                    return Remove(key);
                }

                value = default(TValue);
                return false;
            }
        }

        #endregion
    }
}