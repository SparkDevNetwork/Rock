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

namespace Rock.Lava.DotLiquid
{
    /// <summary>
    /// A proxy for a LavaDataDictionary that can be used by the DotLiquid Templating Framework.
    /// </summary>
    internal class DotLiquidLavaDataDictionaryProxy : IDictionary<string, object>, ILiquidFrameworkDataObjectProxy
    {
        private ILavaDataDictionary _dataObject = null;

        #region Constructors

        public DotLiquidLavaDataDictionaryProxy( ILavaDataDictionary dataObject )
        {
            _dataObject = dataObject;
        }

        #endregion

        #region IDictionary<string, object> implementation

        public bool ContainsKey( string key )
        {
            return _dataObject.ContainsKey( key );
        }

        public void Add( string key, object value )
        {
            throw GetReadOnlyException();
        }

        public bool Remove( string key )
        {
            throw GetReadOnlyException();
        }

        public bool TryGetValue( string key, out object value )
        {
            throw new NotImplementedException();
        }

        public void Add( KeyValuePair<string, object> item )
        {
            throw GetReadOnlyException();
        }

        public void Clear()
        {
            throw GetReadOnlyException();
        }

        public bool Contains( KeyValuePair<string, object> item )
        {
            throw new NotImplementedException();
        }

        public void CopyTo( KeyValuePair<string, object>[] array, int arrayIndex )
        {
            throw new NotImplementedException();
        }

        public bool Remove( KeyValuePair<string, object> item )
        {
            throw GetReadOnlyException();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            var dictionary = new Dictionary<string, object>();

            foreach ( var key in GetAvailableKeys() )
            {
                dictionary.Add( key, GetValue( key ) );
            }

            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                return GetAvailableKeys();
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        int ICollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                return _dataObject.AvailableKeys.Count;
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                return true;
            }

        }

        object IDictionary<string, object>.this[string key]
        {
            get
            {
                return GetValue( key );
            }
            set
            {
                throw GetReadOnlyException();
            }
        }

        #endregion

        #region ILiquidFrameworkDataObjectProxy implementation

        object ILiquidFrameworkDataObjectProxy.GetProxiedDataObject()
        {
            return _dataObject;
        }

        #endregion

        private static Exception GetReadOnlyException()
        {
            return new NotImplementedException( "This Lava Data Dictionary instance is read-only." );
        }

        /// <summary>
        /// Return a string representation of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // Return the ToString() for the proxied object.
            if ( _dataObject != null )
            {
                return _dataObject.ToString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the data value associated with the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private object GetValue( string key )
        {
            if ( _dataObject == null )
            {
                return null;
            }

            return _dataObject.GetValue( key );
        }

        /// <summary>
        /// Gets a list of the keys defined by this data object.
        /// </summary>
        private List<string> GetAvailableKeys()
        {
            if ( _dataObject == null )
            {
                return new List<string>();
            }

            return _dataObject.AvailableKeys;
        }
    }
}
