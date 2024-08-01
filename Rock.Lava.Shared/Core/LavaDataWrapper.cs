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
using System.Collections.Generic;

namespace Rock.Lava
{
    /// <summary>
    /// A wrapper for generic objects that allows them to be accessed as a dictionary of values by the Lava Engine.
    /// </summary>
    internal class LavaDataWrapper : ILavaDataDictionary
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the wrapper with the specified object.
        /// </summary>
        /// <param name="baseObject">The base object.</param>
        public LavaDataWrapper( object baseObject )
        {
            _baseObject = baseObject;
            _availableKeys = new Lazy<List<string>>( GetAvailableKeys );
        }

        /// <summary>
        /// Gets the object wrapped in a LavaDataWrapper if applicable. If the
        /// object does not need to be wrapped the original value will be
        /// returned.
        /// </summary>
        /// <param name="value">The object to wrap.</param>
        /// <returns>The wrapped object or object itself.</returns>
        public static object GetWrappedObject( object value )
        {
            // If the value is a class and not a primitive type or string, wrap it in a new LavaDataWrapper.
            if ( value != null && value.GetType().IsClass )
            {
                if ( value is ILavaDataDictionary || value is string )
                {
                    return value;
                }

                if ( value is System.Collections.ICollection collection )
                {
                    var newCollection = new List<object>();

                    foreach ( var item in collection )
                    {
                        var wrappedItem = GetWrappedObject( item );
                        newCollection.Add( wrappedItem );
                    }

                    return newCollection;
                }

                return new LavaDataWrapper( value );
            }

            return value;
        }

        #endregion

        #region Fields

        /// <summary>
        /// The internal store of available keys.
        /// </summary>
        private readonly Lazy<List<string>> _availableKeys;

        /// <summary>
        /// The internal reference to the base object.
        /// </summary>
        private readonly object _baseObject;

        #endregion

        #region ILavaDataDictionary

        /// <inheritdoc />
        public List<string> AvailableKeys => _availableKeys.Value;

        /// <inheritdoc />
        public bool ContainsKey( string key )
        {
            return AvailableKeys.Contains( key );
        }

        /// <inheritdoc />
        public object GetValue( string key )
        {
            var property = _baseObject.GetType().GetProperty( key );
            if ( property == null )
            {
                return null;
            }

            var value = property.GetValue( _baseObject );
            return GetWrappedObject( value );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the available keys for the base object.
        /// </summary>
        /// <returns>A list of strings used for the keys.</returns>
        private List<string> GetAvailableKeys()
        {
            if ( _baseObject == null )
            {
                return new List<string>();
            }

            // Use reflection to get the properties of the object,
            // and add them as available keys.
            var properties = _baseObject.GetType().GetProperties();
            var availableKeys = new List<string>();

            foreach ( var property in properties )
            {
                availableKeys.Add( property.Name );
            }

            return availableKeys;
        }

        #endregion
    }
}
