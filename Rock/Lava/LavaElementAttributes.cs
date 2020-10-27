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
using System.Linq;
using System.Text.RegularExpressions;
using DotLiquid;

namespace Rock.Lava
{
    /// <summary>
    /// Provides functions to parse and read the attributes specified for a command or shortcode element specified in a Lava Template.
    /// Attributes are specified using the following format:
    /// <![CDATA[
    /// <lavatag param1:value_without_spaces param2:'value with spaces' ... >
    /// </lavatag>
    /// ]]>
    /// </summary>
    public class LavaElementAttributes
    {
        private Dictionary<string, string> _settings = new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );

        /// <summary>
        /// A dictionary of parameter names and values specified as Command settings.
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;

                if ( _settings == null )
                {
                    _settings = new Dictionary<string, string>();
                }
            }
        }

        /// <summary>
        /// Returns a flag indicating if a value exists for the specified parameter name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasValue( string name )
        {
            return _settings.ContainsKey( name );
        }

        /// <summary>
        /// Gets the named parameter setting as a date/time value, or return a default value if the parameter is not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public DateTime? GetDateTimeValue( string name, DateTime? defaultValue = null )
        {
            DateTime? value = null;

            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name].AsDateTime();
            }

            if ( value == null )
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Gets the named parameter setting as an integer value, or return a default value if the parameter is not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int? GetIntegerValue( string name, int? defaultValue = null )
        {
            int? value = null;

            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name].AsIntegerOrNull();
            }

            if ( value == null )
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Gets the named parameter setting as a string value, or return a default value if the parameter is not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <param name="trimWhiteSpace"></param>
        /// <returns></returns>
        public string GetStringValue( string name, string defaultValue = null, bool trimWhiteSpace = true )
        {
            var value = string.Empty;

            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name] ?? string.Empty;
            }

            if ( trimWhiteSpace )
            {
                value = value.Trim();
            }

            return value;
        }

        /// <summary>
        /// Compare the set of attribute names against a set of valid names and return those that do not match.
        /// </summary>
        /// <param name="validNames"></param>
        /// <returns></returns>
        public List<string> GetUnknownAttributes( List<string> validNames )
        {
            var unknownNames = new List<string>();

            if ( _settings == null )
            {
                unknownNames.AddRange( validNames );
            }
            else if ( validNames != null )
            {
                unknownNames.AddRange( _settings.Keys.AsQueryable().Except( validNames, StringComparer.OrdinalIgnoreCase ) );
            }

            return unknownNames;
        }

        /// <summary>
        /// Parse the attributes markup of a Lava element tag to produce a collection of settings.
        /// </summary>
        /// <param name="attributesMarkup"></param>
        /// <param name="context"></param>
        public void ParseFromMarkup( string attributesMarkup, Context context )
        {
            _settings = GetElementAttributes( attributesMarkup, context );
        }

        #region Static methods

        /// <summary>
        /// Parses the attributes of the Lava element tag to extract the parameter settings.
        /// Any merge fields in the attributes markup are resolved before the settings are extracted.
        /// </summary>
        /// <param name="elementAttributesMarkup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetElementAttributes( string elementAttributesMarkup, Context context = null )
        {
            // First, resolve any Lava merge fields that exist in the element attributes markup.
            if ( context != null )
            {
                var internalMergeFields = new Dictionary<string, object>();

                // Get variables defined in the current scope, then from the outer block or container.
                foreach ( var scope in context.Scopes )
                {
                    foreach ( var item in scope )
                    {
                        internalMergeFields.AddOrReplace( item.Key, item.Value );
                    }
                }

                foreach ( var environment in context.Environments )
                {
                    foreach ( var item in environment )
                    {
                        internalMergeFields.AddOrReplace( item.Key, item.Value );
                    }
                }

                elementAttributesMarkup = elementAttributesMarkup.ResolveMergeFields( internalMergeFields );
            }

            // Get the set of parameters using variations of the following pattern:
            // param1:'value1 with spaces' param2:value2_without_spaces param3:'value3 with spaces'
            var parms = new Dictionary<string, string>();

            var markupItems = Regex.Matches( elementAttributesMarkup, @"(\S*?:('[^']+'|[\\d.]+|[\S*]+))" )
                .Cast<Match>()
                .Select( m => m.Value )
                .ToList();

            foreach ( var item in markupItems )
            {
                var itemParts = item.ToString().Split( new char[] { ':' }, 2 );
                if ( itemParts.Length > 1 )
                {
                    if ( itemParts[1].Trim()[0] == '\'' )
                    {
                        parms.AddOrReplace( itemParts[0].Trim(), itemParts[1].Trim().Substring( 1, itemParts[1].Length - 2 ) );
                    }
                    else
                    {
                        parms.AddOrReplace( itemParts[0].Trim(), itemParts[1].Trim() );
                    }
                }
            }

            return parms;
        }

        #endregion
    }
}
