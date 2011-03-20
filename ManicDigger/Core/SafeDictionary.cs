#region Using Statements
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
#endregion

namespace ManicDigger.Core
{
    /// <summary>
    /// Provides a dictionary implementation that supports the safety-get mechanisms.
    /// See documentation for further information on thread safety.
    /// </summary>
    /// <typeparam name="TKey">The type to use for the keys.</typeparam>
    /// <typeparam name="TValue">The type to use for the values.</typeparam>
    /// <remarks>All dictionary-manipulating operations (Add, Insert etc.) are thread-safe and may be used in concurrent access.
    /// However, only the methods themselves are thread-safe, which means that a dictionary-global thread-safety is not enforced.</remarks>
    [Serializable()]
    [DebuggerDisplay("Count = {Count}")]
    public class SafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Fields

        /// <summary>
        /// Provides the inner dictionary.
        /// </summary>
        protected Dictionary<TKey, TValue> Inner;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the SafeDictionary class.
        /// </summary>
        public SafeDictionary()
            : this(0)
        {

        }

        /// <summary>
        /// Initializes a new instance of the SafeDictionary class.
        /// </summary>
        /// <param name="capacity">The initial capacity of the dictionary.</param>
        public SafeDictionary(int capacity)
        {
            this.Inner = new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Provides the deserialization constructor for the SafeDictionary.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        protected SafeDictionary(SerializationInfo info, StreamingContext context)
        {
            // retrieve keys and values from the info and create a new dictionary of them
            TKey[] keys = (TKey[])info.GetValue("Keys", typeof(TKey[]));
            TValue[] values = (TValue[])info.GetValue("Values", typeof(TValue[]));

            this.Inner = new Dictionary<TKey, TValue>(keys.Length);
            lock (Inner)
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    this.AddItem(keys[i], values[i]);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a range of key/value pairs.
        /// </summary>
        /// <param name="pairs"></param>
        public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            lock (Inner)
            {
                foreach (KeyValuePair<TKey, TValue> pair in pairs)
                {
                    this.AddItem(pair.Key, pair.Value);
                }
            }
        }

        #endregion

        #region IDictionary<TKey,TValue> Member

        /// <summary>
        /// Adds the key and value as a pair.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            lock (Inner)
            {
                this.AddItem(key, value);
            }
        }

        /// <summary>
        /// Performs the Add-action.
        /// See documentation for further information on thread safety.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <remarks>This method is called in a thread-safe context, which means that the underlying list is already locked.</remarks>
        protected virtual void AddItem(TKey key, TValue value)
        {
            Inner.Add(key, value);
        }

        /// <summary>
        /// Returns whether or not the given key is contained in this dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(TKey key)
        {
            return Inner.ContainsKey(key);
        }

        /// <summary>
        /// Returns the keys collection.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return Inner.Keys; }
        }

        /// <summary>
        /// Removes the given item.
        /// See documentation for further information on thread safety.
        /// </summary>
        /// <param name="key">The item to remove.</param>
        /// <returns>A boolean value indicating whether or not the item could be removed.</returns>
        /// <remarks>This method will lock the underlying list, making this class thread-safe.</remarks>
        public bool Remove(TKey key)
        {
            bool result = false;
            lock (Inner)
            {
                result = this.RemoveItem(key);
            }
            return result;
        }

        /// <summary>
        /// Removes the given item.
        /// See documentation for further information on thread safety.
        /// </summary>
        /// <param name="key">The item to remove.</param>
        /// <returns>A boolean value indicating whether or not the item could be removed.</returns>
        /// <remarks>This method is called in a thread-safe context, which means that the underlying list is already locked.</remarks>
        protected virtual bool RemoveItem(TKey key)
        {
            if (Inner.ContainsKey(key))
            {
                Inner.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to get the value of the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return Inner.TryGetValue(key, out value);
        }

        /// <summary>
        /// Returns the values collection.
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return Inner.Values; }
        }

        /// <summary>
        /// Returns the value that is stored under its key or sets it.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The value, if the key was found.
        /// -or- <c>null</c>, if the key wasn't found.</returns>
        public TValue this[TKey key]
        {
            get
            {
                lock (Inner)
                {
                    if (!this.ContainsKey(key))
                    {
                        return default(TValue);
                    }
                    else
                    {
                        return Inner[key];
                    }
                }
            }
            set
            {
                lock (Inner)
                {
                    if (!this.ContainsKey(key))
                    {
                        this.AddItem(key, value);
                    }
                    else
                    {
                        this.SetItem(key, value);
                    }
                }
            }
        }
        
        /// <summary>
        /// Tries to return the value registered by the key, and if not found, returns a custom default value.
        /// </summary>
        /// <param name="key">The key to return the value from.</param>
        /// <param name="defaultValue">The default value to return in case the key wasn't found.</param>
        /// <returns>The value, if added. Otherwise a default value.</returns>
        public virtual TValue this[TKey key, TValue defaultValue]
        {
            get
            {
                lock (this.Inner)
                {
                    if (this.Inner.ContainsKey(key))
                    {
                        return this.Inner[key];
                    }
                    else
                    {
                        return defaultValue;
                    }
                }
            }
            set
            {
                this[key] = value;
            }
        }

        /// <summary>
        /// Performs the Set-action.
        /// See documentation for further information on thread safety.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <remarks>This method is called in a thread-safe context, which means that the underlying list is already locked.</remarks>
        protected virtual void SetItem(TKey key, TValue value)
        {
            Inner[key] = value;
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Member

        /// <summary>
        /// Directly adds the given key/value-pair to the dictionary.
        /// </summary>
        /// <param name="item">The key/value-pair to add.</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clears the elements from the dictionary.
        /// </summary>
        /// <remarks>This method will lock the underlying list, making this class thread-safe.</remarks>
        public void Clear()
        {
            lock (Inner)
            {
                ClearDictionary();
            }
        }

        /// <summary>
        /// Clears the elements from the dictionary.
        /// </summary>
        /// <remarks>This method is called in a thread-safe context, which means that the underlying list is already locked.</remarks>
        protected virtual void ClearDictionary()
        {
            Inner.Clear();
        }

        /// <summary>
        /// This operation is not supported yet.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// This operation is not supported yet.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the amount of pairs in this dictionary.
        /// </summary>
        public int Count
        {
            get { return Inner.Count; }
        }

        /// <summary>
        /// This dictionary cannot be read-only.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// This operation is not supported yet.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Member

        /// <summary>
        /// Returns the enumerator to enumerate through the pairs of this dictionary.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

        #endregion

        #region IEnumerable Member

        /// <summary>
        /// Returns the enumerator to enumerate through the pairs of this dictionary.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return Inner.GetEnumerator();
        }

        #endregion
    }
}
