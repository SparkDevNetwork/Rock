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
using System.Dynamic;
using System.Linq;

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Takes an existing (or empty) dictionary and adds a new key and value.
        /// </summary>
        /// <param name="input">The existing dictionary.</param>
        /// <param name="key">The key to use when adding the value.</param>
        /// <param name="value">The value ot use when adding the key.</param>
        /// <returns>A new dictionary that contains the old values and the new value.</returns>
        /// <example><![CDATA[
        /// {% assign dict = '' | AddToDictionary:'key1','value2' %}
        /// {% assign dict = array | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
        /// {% assign dict = array | RemoveFromDictionary:'key2' %}
        /// {{ dict | ToJSON }}
        /// {{ dict | AllKeysFromDictionary }}
        /// ]]></example>
        public static IDictionary<string, object> AddToDictionary( object input, object key, object value )
        {
            var dict = AsDictionary( input );

            dict.AddOrReplace( key.ToString(), value );

            return dict;
        }

        /// <summary>
        /// Takes an existing (or empty) dictionary and removes a key and it's value.
        /// </summary>
        /// <param name="input">The existing dictionary.</param>
        /// <param name="key">The key to use when removing the value.</param>
        /// <returns>A new dictionary that contains the old values without the specified key.</returns>
        /// <example><![CDATA[
        /// {% assign dict = '' | AddToDictionary:'key1','value2' %}
        /// {% assign dict = array | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
        /// {% assign dict = array | RemoveFromDictionary:'key2' %}
        /// {{ dict | ToJSON }}
        /// {{ dict | AllKeysFromDictionary }}
        /// ]]></example>
        public static IDictionary<string, object> RemoveFromDictionary( object input, object key )
        {
            var dict = AsDictionary( input );

            dict.Remove( key.ToString() );

            return dict;
        }

        /// <summary>
        /// Returns an array of all keys in the dictionary.
        /// </summary>
        /// <param name="input">The existing dictionary.</param>
        /// <returns>An enumerable collection of keys.</returns>
        /// <example><![CDATA[
        /// {% assign dict = '' | AddToDictionary:'key1','value2' %}
        /// {% assign dict = array | AddToDictionary:'key2','value2' | AddToDictionary:'key3','value3' %}
        /// {% assign dict = array | RemoveFromDictionary:'key2' %}
        /// {{ dict | ToJSON }}
        /// {{ dict | AllKeysFromDictionary }}
        /// ]]></example>
        public static IEnumerable<string> AllKeysFromDictionary( object input )
        {
            var dict = AsDictionary( input );

            return dict.Keys.ToList();
        }

        /// <summary>
        /// Converts the given value to a dictionary.
        /// </summary>
        /// <param name="input">The value to be converted.</param>
        /// <returns>An IDictionary&lt;string, object&gt; that represents the value.</returns>
        public static IDictionary<string, object> AsDictionary( object input )
        {
            if ( input == null || ( input is string inputStr && inputStr.IsNullOrWhiteSpace() ) )
            {
                return new Dictionary<string, object>();
            }
            else if ( input is IDictionary<string, object> inputGenericDictionary )
            {
                return inputGenericDictionary;
            }
            else if ( input is IDictionary inputDictionary )
            {
                var dict = new Dictionary<string, object>();

                foreach ( DictionaryEntry kvp in inputDictionary )
                {
                    dict.Add( kvp.Key.ToString(), kvp.Value );
                }

                return dict;
            }
            else
            {
                var dict = new Dictionary<string, object>();
                var properties = input.GetType().GetProperties( System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public );

                foreach ( var property in properties )
                {
                    if ( property.GetIndexParameters().Length == 0 )
                    {
                        dict[property.Name] = property.GetValue( input );
                    }
                }

                return dict;
            }
        }
    }
}
