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
using System.Dynamic;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Rock
{
    /// <summary>
    /// Extension methods related to converting things to and from JSON.
    /// </summary>
    public static class JsonExtensions
    {
        #region Fields

        /// <summary>
        /// Contains the singleton serialize settings that match the specified
        /// options key.
        /// </summary>
        private static readonly Dictionary<string, JsonSerializerSettings> _jsonSerializeSettingsCache = new Dictionary<string, JsonSerializerSettings>();

        #endregion

        #region Constructors

        /// <summary>
        /// Handles initialization of any data required by the <see cref="JsonExtensions"/> class.
        /// </summary>
        static JsonExtensions()
        {
            // This effectively forces the cache to be initialized under single
            // threaded conditions so we don't have to worry about concurrency.
            GetSerializeSettings( false, false, false );
            GetSerializeSettings( false, false, true );
            GetSerializeSettings( false, true, false );
            GetSerializeSettings( false, true, true );
            GetSerializeSettings( true, false, false );
            GetSerializeSettings( true, false, true );
            GetSerializeSettings( true, true, false );
            GetSerializeSettings( true, true, true );
        }

        #endregion

        #region JSON Extensions

        /// <summary>
        /// Converts object to JSON string. The output is not indented.
        /// </summary>
        /// <remarks>
        /// Public properties are serialized, but public fields are ignored.
        /// </remarks>
        /// <param name="obj">Object.</param>
        /// <returns></returns>
        public static string ToJson( this object obj )
        {
            return ToJson( obj, false, false );
        }

        /// <summary>
        /// Converts object to JSON string
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="indentOutput"><c>true</c> if the output should be indented for easy reading; otherwise <c>false</c>.</param>
        /// <returns></returns>
        public static string ToJson( this object obj, bool indentOutput )
        {
            return ToJson( obj, indentOutput, false );
        }

        /// <summary>
        /// Converts object to JSON string with an option to ignore errors
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="indentOutput"><c>true</c> if the output should be indented for easy reading; otherwise <c>false</c>.</param>
        /// <param name="ignoreErrors">if set to <c>true</c> errors will be ignored.</param>
        /// <returns></returns>
        public static string ToJson( this object obj, bool indentOutput, bool ignoreErrors )
        {
            var settings = GetSerializeSettings( indentOutput, ignoreErrors, false );

            return JsonConvert.SerializeObject( obj, settings );
        }

        /// <summary>
        /// Converts object to JSON string with an option to ignore errors
        /// </summary>
        /// <param name="obj">Object.</param>
        /// <param name="indentOutput"><c>true</c> if the output should be indented for easy reading; otherwise <c>false</c>.</param>
        /// <param name="ignoreErrors">if set to <c>true</c> errors will be ignored.</param>
        /// <returns></returns>
        /// <remarks>
        /// This only converts POCO and anonymous object property names into
        /// camel case. It will not convert Dictionary keys as those should be
        /// preserved since they have specified the explicit key to use.
        /// 
        /// Marked as internal until there is decision on method name and parameters. -dsh
        /// </remarks>
        public static string ToCamelCaseJson( this object obj, bool indentOutput, bool ignoreErrors )
        {
            var settings = GetSerializeSettings( indentOutput, ignoreErrors, true );

            return JsonConvert.SerializeObject( obj, settings );
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into T.  If it can't be deserialized, returns null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static T FromJsonOrNull<T>( this string val )
        {
            try
            {
                return val.FromJsonOrThrow<T>();
            }
            catch
            {
                return default( T );
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into T. If it can't be deserialized, it will throw an exception
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static T FromJsonOrThrow<T>( this string val )
        {
            try
            {
                if ( string.IsNullOrWhiteSpace( val ) )
                {
                    return default( T );
                }
                else
                {
                    return JsonConvert.DeserializeObject<T>( val );
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Unable to deserialize to {typeof( T ).Name}. {ex}", ex );
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into either a <see cref="ExpandoObject" /> or a list of <see cref="ExpandoObject" />.  If it can't be deserialized, return null
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static object FromJsonDynamicOrNull( this string val )
        {
            try
            {
                return val.FromJsonDynamic();
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( $"Unable to deserialize to dynamic. {ex}" );
                return null;
            }
        }

        /// <summary>
        /// Attempts to deserialize a JSON string into either a <see cref="ExpandoObject" /> or a list of <see cref="ExpandoObject" />. If it can't be deserialized, throws an exception
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns></returns>
        public static object FromJsonDynamic( this string val )
        {
            var converter = new ExpandoObjectConverter();
            object dynamicObject = null;

            // keep track of which exception most applies. 
            Exception singleObjectException = null;
            Exception arrayObjectException = null;

            try
            {
                // first try to deserialize as straight ExpandoObject
                dynamicObject = JsonConvert.DeserializeObject<ExpandoObject>( val, converter );
            }
            catch ( Exception firstException )
            {
                try
                {
                    singleObjectException = firstException;
                    dynamicObject = JsonConvert.DeserializeObject<List<ExpandoObject>>( val, converter );

                }
                catch ( Exception secondException )
                {
                    try
                    {
                        arrayObjectException = secondException;

                        // if it didn't deserialize as a List of ExpandoObject, try it as a List of plain objects
                        dynamicObject = JsonConvert.DeserializeObject<List<object>>( val, converter );
                    }
                    catch
                    {
                        // if both the attempt to deserialize an object and an object list fail, it probably isn't valid JSON, so throw the singleObjectException
                        if ( singleObjectException != null )
                        {
                            throw singleObjectException;
                        }
                        else
                        {
                            throw arrayObjectException;
                        }
                    }
                }
            }

            return dynamicObject;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the cached serializer settings for the given serializer options.
        /// </summary>
        /// <param name="indentOutput"><c>true</c> if output should be indented.</param>
        /// <param name="ignoreErrors"><c>true</c> if errors should be silently ignored.</param>
        /// <param name="camelCase"><c>true</c> if key names should be in camel case.</param>
        /// <returns>A cached instance of <see cref="JsonSerializerSettings"/> to be used.</returns>
        internal static JsonSerializerSettings GetSerializeSettings( bool indentOutput, bool ignoreErrors, bool camelCase )
        {
            var settingsKey = $"{indentOutput}_{ignoreErrors}_{camelCase}";

            // Reading the dictionary is thread-safe.
            if ( !_jsonSerializeSettingsCache.TryGetValue( settingsKey, out var settings ) )
            {
                // This is only the case during class initialization on a single thread.
                settings = CreateSerializerSettings( indentOutput, ignoreErrors, camelCase );

                _jsonSerializeSettingsCache[settingsKey] = settings;
            }

            return settings;
        }

        /// <summary>
        /// Create a serializer settings for the given serializer options.
        /// </summary>
        /// <param name="indentOutput"><c>true</c> if output should be indented.</param>
        /// <param name="ignoreErrors"><c>true</c> if errors should be silently ignored.</param>
        /// <param name="camelCase"><c>true</c> if key names should be in camel case.</param>
        /// <returns>An instance of <see cref="JsonSerializerSettings"/> to be used.</returns>
        internal static JsonSerializerSettings CreateSerializerSettings( bool indentOutput, bool ignoreErrors, bool camelCase )
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = indentOutput ? Formatting.Indented : Formatting.None
            };

            if ( ignoreErrors )
            {
                settings.Error += new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>( ( s, e ) =>
                {
                    e.ErrorContext.Handled = true;
                } );
            }

            if ( camelCase )
            {
                settings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
                {
                    NamingStrategy = new Newtonsoft.Json.Serialization.CamelCaseNamingStrategy
                    {
                        // Do not process dictionaries, this messes up attribute keys
                        // and generally with a dictionary they are specifying a specific
                        // key that it should be anyway.
                        ProcessDictionaryKeys = false,
                        OverrideSpecifiedNames = true
                    }
                };
            }

            return settings;
        }

        #endregion
    }
}
