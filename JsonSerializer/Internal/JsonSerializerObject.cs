using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
namespace SwiftJson.Internal
{
    sealed class JsonSerializerObject : IDictionary<string, object>//, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private readonly Dictionary<string, object> _members;

        public int Count
        {
            get
            {
                return this._members.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public object this[int index]
        {
            get
            {
                return GetAtIndex(this._members, index);
            }
        }

        public object this[string key]
        {
            get
            {
                return this._members[key];
            }
            set
            {
                this._members[key] = value;
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                return this._members.Keys;
            }
        }

        public ICollection<object> Values
        {
            get
            {
                return this._members.Values;
            }
        }

        public JsonSerializerObject()
        {
            this._members = new Dictionary<string, object>();
        }

        public JsonSerializerObject(IEqualityComparer<string> comparer)
        {
            this._members = new Dictionary<string, object>(comparer);
        }

        public void Add(string key, object value)
        {
            this._members.Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this._members.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this._members.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            if (!this._members.ContainsKey(item.Key))
            {
                return false;
            }
            return this._members[item.Key] == item.Value;
        }

        public bool ContainsKey(string key)
        {
            return this._members.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int count = this.Count;
            foreach (KeyValuePair<string, object> keyValuePair in this)
            {
                int num = arrayIndex;
                arrayIndex = num + 1;
                array[num] = keyValuePair;
                int num1 = count - 1;
                count = num1;
                if (num1 > 0)
                {
                    continue;
                }
                return;
            }
        }

        static object GetAtIndex(IDictionary<string, object> obj, int index)
        {
            object value;
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (index >= obj.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int num = 0;
            using (IEnumerator<KeyValuePair<string, object>> enumerator = obj.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    KeyValuePair<string, object> current = enumerator.Current;
                    int num1 = num;
                    num = num1 + 1;
                    if (num1 != index)
                    {
                        continue;
                    }
                    value = current.Value;
                    return value;
                }

            }
            return null;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this._members.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return this._members.Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return this._members.Remove(item.Key);
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._members.GetEnumerator();
        }

        public override string ToString()
        {
            return Serializer.SerializeObjectToString(this);
        }

        public bool TryGetValue(string key, out object value)
        {
            return this._members.TryGetValue(key, out value);
        }
    }
}