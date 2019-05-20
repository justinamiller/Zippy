using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace JsonSerializer.Utility
{
    sealed class FactoryDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private readonly object _lock = new object();

        private readonly ReflectionExtension.DictionaryValueFactory<TKey, TValue> _valueFactory;

        private Dictionary<TKey, TValue> _dictionary;

        public int Count
        {
            get
            {
                return this._dictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return this._dictionary.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                return this._dictionary.Values;
            }
        }

        public FactoryDictionary(ReflectionExtension.DictionaryValueFactory<TKey, TValue> valueFactory)
        {
            this._valueFactory = valueFactory;
        }

        public void Add(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        private TValue AddValue(TKey key)
        {
            if (key == null)
                return default(TValue);
            TValue tValue1 = this._valueFactory(key);

            Monitor.Enter(this._lock);
            try
            {
                TValue tValue;
                if (this._dictionary == null)
                {
                    this._dictionary = new Dictionary<TKey, TValue>
                    {
                        [key] = tValue1
                    };
                }
                else if (!this._dictionary.TryGetValue(key, out tValue))
                {
                    Dictionary<TKey, TValue> tKeys = new Dictionary<TKey, TValue>(this._dictionary)
                    {
                        [key] = tValue1
                    };
                    this._dictionary = tKeys;
                }
                else
                {
                    return tValue;
                }
            }
            finally
            {
                Monitor.Exit(this._lock);
            }
            return tValue1;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(TKey key)
        {
            return this._dictionary != null && this._dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        private TValue Get(TKey key)
        {
            if (this._dictionary == null)
            {
                return this.AddValue(key);
            }

            TValue tValue;
            if (this._dictionary.TryGetValue(key, out tValue))
            {
                return tValue;
            }
            return this.AddValue(key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            Monitor.Enter(this._lock);
            try
            {
                return this._dictionary.Remove(key);
            }
            finally
            {
                Monitor.Exit(this._lock);
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = this[key];
            return true;
        }

    }
}
