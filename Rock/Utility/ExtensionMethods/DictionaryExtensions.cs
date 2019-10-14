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

namespace Rock
{
    /// <summary>
    /// Dictionary Extensions that don't reference Rock
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region Dictionary<TKey, TValue> extension methods

        /// <summary>
        /// Adds or replaces an item in a Dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrReplace<TKey, TValue>( this Dictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            AddOrReplace( ( IDictionary<TKey, TValue> ) dictionary, key, value );
        }

        /// <summary>
        /// Adds the or replace.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrReplace<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            if ( !dictionary.ContainsKey( key ) )
            {
                dictionary.Add( key, value );
            }
            else
            {
                dictionary[key] = value;
            }
        }

        /// <summary>
        /// Adds an item to a Dictionary if it doesn't already exist in Dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public static void AddOrIgnore<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            if ( !dictionary.ContainsKey( key ) )
            {
                dictionary.Add( key, value );
            }
        }

        /// <summary>
        /// Adds if not empty.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="replace">if set to <c>true</c> [replace].</param>
        public static void AddIfNotBlank( this IDictionary<string, string> dictionary, string key, string value, bool replace = true )
        {
            if ( value.IsNotNullOrWhiteSpace() )
            {
                if ( !dictionary.ContainsKey( key ) )
                {
                    dictionary.Add( key, value );
                }
                else
                {
                    if ( replace )
                    {
                        dictionary[key] = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets value for the specified key, or null if the dictionary doesn't contain the key
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static TValue GetValueOrNull<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return default( TValue );
            }
        }

        /// <summary>
        /// Gets the value or null.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static int? GetValueOrNull<TKey>( this IDictionary<TKey, int> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the value or null.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static decimal? GetValueOrNull<TKey>( this IDictionary<TKey, decimal> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the value or null.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static double? GetValueOrNull<TKey>( this IDictionary<TKey, double> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the value or null.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static DateTime? GetValueOrNull<TKey>( this IDictionary<TKey, DateTime> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the value or null.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static Guid? GetValueOrNull<TKey>( this IDictionary<TKey, Guid> dictionary, TKey key )
        {
            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return null;
            }
        }

        #endregion Dictionary<TKey, TValue> extension methods
    }
}
