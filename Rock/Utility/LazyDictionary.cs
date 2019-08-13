// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections;
using System.Collections.Generic;

namespace Rock.Utility
{
    /// <summary>
    /// The lazy dictionary is intended to be used in cases where you already
    /// have Lazy<typeparamref name="TValue"/> values to store. When the item
    /// is accessed it, the Lazy value will be loaded and the actual lazy value
    /// will be returned rather than the lazy object. This will not magically
    /// turn eager loaded objects into lazy loaded objects.
    /// </summary>
    /// <typeparam name="TKey">The key type that will be used to access the values.</typeparam>
    /// <typeparam name="TValue">The type of value that will be associated with each key.</typeparam>
    public class LazyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Fields

        /// <summary>
        /// The underlying dictionary that will hold the lazy objects.
        /// </summary>
        private Dictionary<TKey, Lazy<TValue>> _dictionary;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize an empty lazy dictionary.
        /// </summary>
        public LazyDictionary()
        {
            _dictionary = new Dictionary<TKey, Lazy<TValue>>();
        }

        #endregion

        #region IDictionary

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <param name="key">The key to use when getting or setting the value.</param>
        /// <value>The <see cref="TValue"/> to be accessed.</value>
        /// <returns></returns>
        public TValue this[TKey key]
        {
            get
            {
                var lazyValue = _dictionary[key];
                var value = lazyValue.Value;
                return value;
            }
            set => _dictionary[key] = new Lazy<TValue>( () => value );
        }

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the keys
        /// of the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        public ICollection<TKey> Keys => _dictionary.Keys;

        /// <summary>
        /// Gets an <see cref="T:System.Collections.Generic.ICollection`1" /> containing the values
        /// in the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <exception cref="NotSupportedException">Accessing all Values will cause eager loading.</exception>
        public ICollection<TValue> Values => throw new NotSupportedException( "Accessing all Values will cause eager loading." );

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => _dictionary.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds an element with the provided key and value to the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The object to use as the key of the element to add.</param>
        /// <param name="value">The object to use as the value of the element to add.</param>
        public void Add( TKey key, TValue value )
        {
            _dictionary.Add( key, new Lazy<TValue>( () => value ) );
        }

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add( KeyValuePair<TKey, TValue> item )
        {
            _dictionary.Add( item.Key, new Lazy<TValue>( () => item.Value ) );
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            _dictionary.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        /// <exception cref="NotSupportedException">Checking for a specific value will cause eager loading.</exception>
        public bool Contains( KeyValuePair<TKey, TValue> item )
        {
            throw new NotSupportedException( "Checking for a specific value will cause eager loading." );
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.IDictionary`2" />.</param>
        /// <returns>
        /// true if the <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the key; otherwise, false.
        /// </returns>
        public bool ContainsKey( TKey key )
        {
            return _dictionary.ContainsKey( key );
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        /// <exception cref="NotSupportedException">Copying specific item values will cause eager loading.</exception>
        public void CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
        {
            throw new NotSupportedException( "Copying specific item values will cause eager loading." );
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach ( var kvp in _dictionary )
            {
                yield return new KeyValuePair<TKey, TValue>( kvp.Key, kvp.Value.Value );
            }
        }

        /// <summary>
        /// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key" /> was not found in the original <see cref="T:System.Collections.Generic.IDictionary`2" />.
        /// </returns>
        public bool Remove( TKey key )
        {
            return _dictionary.Remove( key );
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        /// <exception cref="NotSupportedException">Removing a specific value will cause eager loading.</exception>
        public bool Remove( KeyValuePair<TKey, TValue> item )
        {
            throw new NotSupportedException( "Removing a specific value will cause eager loading." );
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// true if the object that implements <see cref="T:System.Collections.Generic.IDictionary`2" /> contains an element with the specified key; otherwise, false.
        /// </returns>
        public bool TryGetValue( TKey key, out TValue value )
        {
            bool status = _dictionary.TryGetValue( key, out var lazyValue );

            value = status ? lazyValue.Value : default;

            return status;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach ( var kvp in _dictionary )
            {
                yield return new KeyValuePair<TKey, TValue>( kvp.Key, kvp.Value.Value );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a lazy-wrapped value with the specified key to the dictionary.
        /// </summary>
        /// <param name="key">The key that will be used to reference this value.</param>
        /// <param name="lazyValue">The lazy-wrapped value to used by this key.</param>
        public void Add( TKey key, Lazy<TValue> lazyValue )
        {
            _dictionary.Add( key, lazyValue );
        }

        /// <summary>
        /// Adds a lazy-wrapped value with the specified key to the dictionary.
        /// </summary>
        /// <param name="key">The key that will be used to reference this value.</param>
        /// <param name="valueFactory">The function that wil be executed in order to initialize the value.</param>
        public void Add( TKey key, Func<TValue> valueFactory )
        {
            Add( key, new Lazy<TValue>( valueFactory ) );
        }

        /// <summary>
        /// Adds a lazy-wrapped value with the specified key to the dictionary. If an
        /// item with the key already exists then it will be replaced.
        /// </summary>
        /// <param name="key">The key that will be used to reference this value.</param>
        /// <param name="lazyValue">The lazy-wrapped value to used by this key.</param>
        public void AddOrReplace( TKey key, Lazy<TValue> lazyValue )
        {
            _dictionary.AddOrReplace( key, lazyValue );
        }

        /// <summary>
        /// Adds a lazy-wrapped value with the specified key to the dictionary. If an
        /// item with the key already exists then it will be replaced.
        /// </summary>
        /// <param name="key">The key that will be used to reference this value.</param>
        /// <param name="valueFactory">The function that wil be executed in order to initialize the value.</param>
        public void AddOrReplace( TKey key, Func<TValue> valueFactory )
        {
            AddOrReplace( key, new Lazy<TValue>( valueFactory ) );
        }

        #endregion
    }
}
