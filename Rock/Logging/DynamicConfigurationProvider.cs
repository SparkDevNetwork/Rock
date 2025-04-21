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

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json.Linq;

namespace Rock.Logging
{
    /// <summary>
    /// Custom configuraiton provider that uses an internal dictionary to
    /// store the settings. This will also notify the configuration system
    /// that a change has been made.
    /// </summary>
    internal class DynamicConfigurationProvider : ConfigurationProvider, IConfigurationSource
    {
        /// <inheritdoc/>
        IConfigurationProvider IConfigurationSource.Build( IConfigurationBuilder builder ) => this;

        /// <inheritdoc/>
        public override void Set( string key, string value )
        {
            base.Set( key, value );

            OnReload();
        }

        /// <summary>
        /// Loads the settings from the JSON object. If the JSON can not be
        /// parsed then no changes are made.
        /// </summary>
        /// <param name="json">The json string to be parsed.</param>
        /// <param name="resetAllSettings">if set to <c>true</c> then all current settings are cleared.</param>
        /// <returns><c>true</c> if the object was parsed correctly, <c>false</c> otherwise.</returns>
        public bool LoadFromJson( string json, bool resetAllSettings )
        {
            if ( string.IsNullOrEmpty( json ) )
            {
                return false;
            }

            var data = resetAllSettings
                ? new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase )
                : new Dictionary<string, string>( Data, StringComparer.OrdinalIgnoreCase );

            try
            {
                var config = Newtonsoft.Json.JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>( json );

                if ( config != null )
                {
                    ParseConfigObject( data, config, new List<string>() );
                }
            }
            catch ( Exception ex )
            {
                System.Diagnostics.Debug.WriteLine( ex.Message );
                return false;
            }

            Data = data;
            OnReload();

            return true;
        }

        /// <summary>
        /// Parses the configuration object into a format understood by this
        /// class. We store keys as single-level, but separated by <c>:</c>. So
        /// the <c>Logging</c> key that contains the <c>LogLevel</c> key which
        /// contains the <c>Default</c> key would be stored as
        /// <c>Logging:LogLevel:Default</c>.
        /// </summary>
        /// <param name="data">The data to store the keys and values in.</param>
        /// <param name="configObject">The configuration object to be parsed.</param>
        /// <param name="parentKeys">The parent keys of this object level.</param>
        private static void ParseConfigObject( IDictionary<string, string> data, JObject configObject, IReadOnlyList<string> parentKeys )
        {
            foreach ( var pair in configObject )
            {
                if ( pair.Value is JObject childObject )
                {
                    var keys = new List<string>( parentKeys )
                    {
                        pair.Key
                    };

                    ParseConfigObject( data, childObject, keys );
                }
                else if ( pair.Value is JValue value && value.Value is string stringValue )
                {
                    var keys = new List<string>( parentKeys )
                    {
                        pair.Key
                    };

                    var key = string.Join( ":", keys );

                    if ( !data.ContainsKey( key ) )
                    {
                        data.Add( key, stringValue );
                    }
                }
            }
        }
    }
}
