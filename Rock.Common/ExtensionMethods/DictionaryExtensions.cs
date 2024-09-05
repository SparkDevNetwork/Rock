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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

/*
 * 2020-11-16 ETD
 * IMPORTANT!
 * This class is used by the CheckScanner which does not have the Rock dll. This file cannot contain any dependencies on that assembly or NuGet packages.
 */

namespace Rock
{
    /// <summary>
    /// Extension methods for the Dictionary types.
    /// </summary>
    public static class DictionaryExtensions
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
        public static void AddOrReplace<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            dictionary[key] = value;
        }

        /// <summary>
        /// <para>
        /// Adds an item to a Dictionary if it doesn't already exist in Dictionary.
        /// </para>
        /// <para>
        /// <strong>This method should not be used with <see cref="ConcurrentDictionary{TKey, TValue}"/>.</strong>
        /// </para>
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        [RockObsolete( "1.16.6" )]
        [Obsolete( "Use TryAdd() instead." )]
        public static void AddOrIgnore<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, TValue> concurrentDictionary )
            {
                concurrentDictionary.TryAdd( key, value );

                return;
            }

            if ( !dictionary.ContainsKey( key ) )
            {
                dictionary.Add( key, value );
            }
        }

#if !NET5_0_OR_GREATER
        /// <summary>
        /// Tries to add the specified key and value to the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="dictionary">The dictionary to add the key and value to.</param>
        /// <param name="key">The key to add to the dictionary.</param>
        /// <param name="value">The value to add to the dictionary.</param>
        /// <returns><c>true</c> when the key and value are successfully added to the dictionary; <c>false</c> when the dictionary already contains the specified key, in which case nothing gets added.</returns>
        public static bool TryAdd<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, TValue value )
        {
            // This method is taken from the .NET Core runtime. When calling TryAdd() on
            // a ConcurrentDictionary the C# compiler will pick the TryAdd() method
            // declared on the concurrent implementation rather than this method.

            if ( dictionary == null )
            {
                throw new ArgumentNullException( nameof( dictionary ) );
            }

            if ( !dictionary.ContainsKey( key ) )
            {
                dictionary.Add( key, value );
                return true;
            }

            return false;
        }
#endif

        /// <summary>
        /// Adds a string value to a Dictionary if it is not blank.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="replace">if set to <c>true</c> [replace].</param>
        [Obsolete( "This method will be removed in the future, use manual logic instead." )]
        [RockObsolete( "1.16.6" )]
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
        /// Gets value for the specified key, or null if the dictionary doesn't contain the key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static TValue GetValueOrNull<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key )
        {
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, TValue> concurrentDictionary )
            {
                return concurrentDictionary.TryGetValue( key, out var value )
                    ? value
                    : default;
            }

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
        /// Gets the value of the given key if found. If not then returns the provided default value.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static TValue GetReadOnlyValueOrDefault<TKey, TValue>( this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue )
        {
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, TValue> concurrentDictionary )
            {
                return concurrentDictionary.TryGetValue( key, out var value )
                    ? value
                    : defaultValue;
            }

            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return defaultValue;
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
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, int> concurrentDictionary )
            {
                return concurrentDictionary.TryGetValue( key, out var value )
                    ? ( int? ) value
                    : null;
            }

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
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, decimal> concurrentDictionary )
            {
                return concurrentDictionary.TryGetValue( key, out var value )
                    ? ( decimal? ) value
                    : null;
            }

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
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, double> concurrentDictionary )
            {
                return concurrentDictionary.TryGetValue( key, out var value )
                    ? ( double? ) value
                    : null;
            }

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
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, DateTime> concurrentDictionary )
            {
                return concurrentDictionary.TryGetValue( key, out var value )
                    ? ( DateTime? ) value
                    : null;
            }

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
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, Guid> concurrentDictionary )
            {
                return concurrentDictionary.TryGetValue( key, out var value )
                    ? ( Guid? ) value
                    : null;
            }

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
        /// Gets the value associated with the specified key or a default value if the key is not found.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value returned if the key does not exist.</param>
        /// <returns></returns>
        public static TValue GetValueOrDefault<TKey, TValue>( this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue )
        {
            // Special logic when dealing with concurrent dictionaries to
            // solve issue https://github.com/SparkDevNetwork/Rock/issues/5852.
            if ( dictionary is ConcurrentDictionary<TKey, TValue> concurrentDictionary )
            {
                return concurrentDictionary.TryGetValue( key, out var value )
                    ? value
                    : defaultValue;
            }

            if ( dictionary.ContainsKey( key ) )
            {
                return dictionary[key];
            }
            else
            {
                return defaultValue;
            }
        }

        #endregion Dictionary<TKey, TValue> extension methods
    }
}
