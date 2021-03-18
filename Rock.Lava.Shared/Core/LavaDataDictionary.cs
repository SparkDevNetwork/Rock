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
using System.Reflection;

namespace Rock.Lava
{
    /// <summary>
    /// A case-insensitive dictionary implementation for storing and retrieving variables used to resolve Lava templates.
    /// </summary>
    /// <remarks>
    /// The Lava Engine is able to work with any Type that can provide a set of values accessible through the IDictionary<string, object> interface.
    /// This class provides a basic implementation of that interface, with the addition of case-insensitivity and lazy-loading of values.
    /// </remarks>
    public class LavaDataDictionary : IDictionary<string, object>, IDictionary, ILavaDataDictionary
    {
        #region Fields

        private readonly Func<LavaDataDictionary, string, object> _lambda;
        private readonly Dictionary<string, object> _nestedDictionary = new Dictionary<string, object>( StringComparer.OrdinalIgnoreCase );

        #endregion

        #region Static construction methods

        /// <summary>
        /// Create a new instance that provides Lava template access for the properties of an anonymous object.
        /// </summary>
        /// <param name="anonymousObject"></param>
        /// <returns></returns>
        public static LavaDataDictionary FromAnonymousObject( object anonymousObject )
        {
            return new LavaDataDictionary( anonymousObject );
        }

        /// <summary>
        /// Create a new instance that provides Lava template access for the items in an existing .NET dictionary.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static LavaDataDictionary FromDictionary( IDictionary<string, object> dictionary )
        {
            return new LavaDataDictionary( dictionary );
        }

        #endregion

        #region Constructors

        public LavaDataDictionary( object anonymousObject )
        {
            if ( anonymousObject == null )
            {
                return;
            }

            foreach ( PropertyInfo property in anonymousObject.GetType().GetProperties() )
            {
                this[property.Name] = property.GetValue( anonymousObject, null );
            }
        }

        public LavaDataDictionary( IDictionary<string, object> dictionary )
        {
            foreach ( var keyValue in dictionary )
            {
                this.Add( keyValue );
            }
        }

        public LavaDataDictionary( Func<LavaDataDictionary, string, object> lambda )
            : this()
        {
            _lambda = lambda;
        }

        public LavaDataDictionary()
        {
        }

        #endregion

        /// <summary>
        /// Merge the entries from an existing dictionary.
        /// </summary>
        /// <param name="otherValues"></param>
        public void Merge( IDictionary<string, object> otherValues )
        {
            foreach ( string key in otherValues.Keys )
            {
                _nestedDictionary[key] = otherValues[key];
            }
        }

        /// <summary>
        /// Get a value from the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object GetValue( string key )
        {
            if ( _nestedDictionary.ContainsKey( key ) )
            {
                return _nestedDictionary[key];
            }

            if ( _lambda != null )
            {
                return _lambda( this, key );
            }

            return null;
        }

        /// <summary>
        /// Set a value in the dictionary.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue( string key, object value )
        {
            _nestedDictionary[key] = value;
        }

        /// <summary>
        /// Get a strongly-typed value from the dictionary.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>( string key )
        {
            return (T)this[key];
        }

        #region IDictionary<string, object>

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _nestedDictionary.GetEnumerator();
        }

        public void Remove( object key )
        {
            ( (IDictionary)_nestedDictionary ).Remove( key );
        }

        object IDictionary.this[object key]
        {
            get
            {
                if ( !( key is string ) )
                    throw new NotSupportedException();
                return GetValue( (string)key );
            }
            set { ( (IDictionary)_nestedDictionary )[key] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _nestedDictionary.GetEnumerator();
        }

        public void Add( KeyValuePair<string, object> item )
        {
            ( (IDictionary<string, object>)_nestedDictionary ).Add( item );
        }

        public bool Contains( object key )
        {
            return ( (IDictionary)_nestedDictionary ).Contains( key );
        }

        public void Add( object key, object value )
        {
            ( (IDictionary)_nestedDictionary ).Add( key, value );
        }

        public void Clear()
        {
            _nestedDictionary.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ( (IDictionary)_nestedDictionary ).GetEnumerator();
        }

        public bool Contains( KeyValuePair<string, object> item )
        {
            return ( (IDictionary<string, object>)_nestedDictionary ).Contains( item );
        }

        public void CopyTo( KeyValuePair<string, object>[] array, int arrayIndex )
        {
            ( (IDictionary<string, object>)_nestedDictionary ).CopyTo( array, arrayIndex );
        }

        public bool Remove( KeyValuePair<string, object> item )
        {
            return ( (IDictionary<string, object>)_nestedDictionary ).Remove( item );
        }

        #endregion

        #region IDictionary

        public void CopyTo( Array array, int index )
        {
            ( (IDictionary)_nestedDictionary ).CopyTo( array, index );
        }

        public int Count
        {
            get { return _nestedDictionary.Count; }
        }

        public object SyncRoot
        {
            get { return ( (IDictionary)_nestedDictionary ).SyncRoot; }
        }

        public bool IsSynchronized
        {
            get { return ( (IDictionary)_nestedDictionary ).IsSynchronized; }
        }

        ICollection IDictionary.Values
        {
            get { return ( (IDictionary)_nestedDictionary ).Values; }
        }

        public bool IsReadOnly
        {
            get { return ( (IDictionary<string, object>)_nestedDictionary ).IsReadOnly; }
        }

        public bool IsFixedSize
        {
            get { return ( (IDictionary)_nestedDictionary ).IsFixedSize; }
        }

        public bool ContainsKey( string key )
        {
            return _nestedDictionary.ContainsKey( key );
        }

        public void Add( string key, object value )
        {
            _nestedDictionary.Add( key, value );
        }

        public bool Remove( string key )
        {
            return _nestedDictionary.Remove( key );
        }

        public bool TryGetValue( string key, out object value )
        {
            return _nestedDictionary.TryGetValue( key, out value );
        }

        public object GetValue( object key )
        {
            if ( key == null )
            {
                return null;
            }

            return GetValue( key.ToString() );
        }

        public bool ContainsKey( object key )
        {
            if ( key == null )
            {
                return false;
            }

            return ContainsKey( key.ToString() );
        }

        public Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>( _nestedDictionary );
        }

        public object this[string key]
        {
            get { return GetValue( key ); }
            set { _nestedDictionary[key] = value; }
        }

        public ICollection<string> Keys
        {
            get { return _nestedDictionary.Keys; }
        }

        ICollection IDictionary.Keys
        {
            get { return ( (IDictionary)_nestedDictionary ).Keys; }
        }

        public ICollection<object> Values
        {
            get { return _nestedDictionary.Values; }
        }

        public List<string> AvailableKeys
        {
            get { return new List<string>( _nestedDictionary.Keys ); }
        }

        #endregion
    }
}