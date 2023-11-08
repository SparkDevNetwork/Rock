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
using Rock.Lava.DotLiquid;

namespace Rock.Lava
{
    /// <summary>
    /// Provides functions to parse and read the attributes specified for a command or shortcode element specified in a Lava Template.
    ///
    /// Attribute names are case-insensitive, and are interpreted as lower-case whereever possible.
    /// Attributes are specified using the following format:
    /// <![CDATA[
    /// <lavatag param1:value_without_spaces param2:'value with spaces' ... >
    /// </lavatag>
    /// ]]>
    /// </summary>
    public class LavaElementAttributes
    {
        #region Factory Methods

        /// <summary>
        /// Create a new instance from the specified markup.
        /// </summary>
        /// <param name="attributesMarkup"></param>
        /// <returns></returns>
        public static LavaElementAttributes NewFromMarkup( string attributesMarkup )
        {
            return NewFromMarkup( attributesMarkup, null );
        }

        /// <summary>
        /// Create a new instance from the specified markup.
        /// </summary>
        /// <param name="attributesMarkup"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static LavaElementAttributes NewFromMarkup( string attributesMarkup, ILavaRenderContext context )
        {
            var attributes = new LavaElementAttributes();

            attributes.ParseFromMarkup( attributesMarkup, context );

            return attributes;
        }

        #endregion

        /// <summary>
        /// Create a copy of the current object.
        /// </summary>
        /// <returns></returns>
        public LavaElementAttributes Clone()
        {
            var attributes = new LavaElementAttributes();

            foreach( var kv in _settings )
            {
                attributes[kv.Key] = kv.Value;
            }

            return attributes;
        }

        private Dictionary<string, string> _settings = GetConfiguredAttributeDictionary();

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
                // This set function should be removed, because it could be misleading.
                // It performs a value copy rather than setting a reference to the supplied dictionary,
                // because the internal dictionary must use a case-insensitive key.
                AddOrIgnore( value );
            }
        }

        /// <summary>
        /// Gets the number of defined attributes.
        /// </summary>
        public int Count
        {
            get
            {
                return _settings.Count;
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
        /// Sets the value of the specified parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void SetValue( string name, string value )
        {
            if ( string.IsNullOrWhiteSpace(name) )
            {
                throw new ArgumentException( "Parameter name is invalid", nameof( name ) );
            }

            _settings[name.Trim().ToLower()] = value;
        }

        /// <summary>
        /// Gets the value of the specified parameter.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// Returns the value of the specified parameter as a string, or null if the parameter does not exist.
        /// </returns>
        public string this[string index]
        {
            get
            {
                return GetString( index );
            }
            set
            {
                SetValue( index, value );
            }
        }

        /// <summary>
        /// Adds the specified parameter and value if it does not already exist.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public bool AddOrIgnore( string name, string value )
        {
            if ( _settings.ContainsKey( name ) )
            {
                return false;
            }

            _settings[name] = value;

            return true;
        }

        /// <summary>
        /// Adds the specified parameter and value if it does not already exist.
        /// </summary>
        /// <param name="values"></param>
        public bool AddOrIgnore( IDictionary<string, string> values )
        {
            if ( values == null )
            {
                return false;
            }

            bool added = false;
            foreach ( var value in values )
            {
                added = added | AddOrIgnore( value.Key, value.Value );
            }

            return added;
        }

        /// <summary>
        /// Remove the named parameter.
        /// </summary>
        /// <param name="name"></param>
        public bool Remove( string name )
        {
            name = name?.Trim() ?? string.Empty;

            if ( !_settings.ContainsKey( name ) )
            {
                return false;
            }

            var removed = _settings.Remove( name );
            return removed;
        }

        /// <summary>
        /// Remove the named parameters.
        /// </summary>
        /// <param name="names"></param>
        /// <returns></returns>
        public bool Remove( IEnumerable<string> names )
        {
            bool removed = false;
            foreach ( var name in names )
            {
                if ( _settings.Remove( name ) )
                {
                    removed = true;
                }
            }

            return removed;
        }

        /// <summary>
        /// Gets the named parameter setting as a boolean value, or return a default value if the parameter is not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public bool GetBoolean( string name, bool defaultValue = false )
        {
            return GetBooleanOrNull( name ) ?? defaultValue;
        }

        /// <summary>
        /// Gets the named parameter setting as a boolean value, or return null if the parameter does not exist or has an invalid value.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool? GetBooleanOrNull( string name )
        {
            bool? value = null;

            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name].AsBooleanOrNull();
            }

            return value;
        }

        /// <summary>
        /// Gets the named parameter setting as a date/time value, or return a default value if the parameter is not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <remarks>
        /// The defaultValue parameter is required, because default(DateTime) returns an unexpected or invalid value for some use cases.
        /// </remarks>
        /// <returns></returns>
        public DateTime GetDateTime( string name, DateTime defaultValue )
        {
            return GetDateTimeOrNull( name ) ?? defaultValue;
        }

        /// <summary>
        /// Gets the named parameter setting as a date/time value, or return null if the parameter does not exist or has an invalid value.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DateTime? GetDateTimeOrNull( string name )
        {
            DateTime? value = null;

            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name].AsDateTime();
            }

            return value;
        }

        /// <summary>
        /// Gets the named parameter setting as an integer value, or return a default value if the parameter is not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public int GetInteger( string name, int defaultValue = 0 )
        {
            return GetIntegerOrNull( name ) ?? defaultValue;
        }

        /// <summary>
        /// Gets the named parameter setting as an integer value, or return null if the parameter does not exist or has an invalid value.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int? GetIntegerOrNull( string name )
        {
            int? value = null;

            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name].AsIntegerOrNull();
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
        public string GetString( string name, string defaultValue = "", bool trimWhiteSpace = true )
        {
            return GetStringOrNull( name, trimWhiteSpace ) ?? defaultValue;
        }

        /// <summary>
        /// Gets the named parameter setting as a string value, or return null if the parameter does not exist or has an invalid value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="trimWhiteSpace"></param>
        /// <returns></returns>
        public string GetStringOrNull( string name, bool trimWhiteSpace = true )
        {
            string value = null;

            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name] ?? string.Empty;
            }

            if ( trimWhiteSpace && value != null )
            {
                value = value.Trim();
            }

            return value;
        }

        /// <summary>
        /// Gets the named parameter setting as an Enum value, or a default value if the parameter is invalid or not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public TEnum GetEnum<TEnum>( string name, TEnum defaultValue = default( TEnum ) )
            where TEnum : struct, Enum
        {
            return GetEnumOrNull<TEnum>( name ) ?? defaultValue;
        }

        /// <summary>
        /// Gets the named parameter setting as an Enum value, or a default value if the parameter is invalid or not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TEnum? GetEnumOrNull<TEnum>( string name )
            where TEnum : struct, Enum
        {
            var stringValue = GetString( name, string.Empty );

            TEnum? value;
            if ( Enum.IsDefined( typeof( TEnum ), stringValue ) )
            {
                value = ( TEnum ) Enum.Parse( typeof( TEnum ), stringValue, true );
            }
            else
            {
                value = null;
            }

            return value;
        }

        #region Obsolete

        /// <summary>
        /// Gets the named parameter setting as a boolean value, or return a default value if the parameter is not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        [Obsolete( "Use the GetBoolean/GetBooleanOrNull methods instead." )]
        [RockObsolete( "1.15" )]
        public bool? GetBooleanValue( string name, bool? defaultValue = null )
        {
            bool? value = null;

            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name].AsBooleanOrNull();
            }

            if ( value == null )
            {
                return defaultValue;
            }

            return value;
        }

        /// <summary>
        /// Gets the named parameter setting as a date/time value, or return a default value if the parameter is not specified.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        [Obsolete( "Use the GetDateTime/GetDateTimeOrNull methods instead." )]
        [RockObsolete( "1.15" )]
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
        [Obsolete( "Use the GetInteger/GetIntegerOrNull methods instead." )]
        [RockObsolete( "1.15" )]
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
        [Obsolete("Use the GetString/GetStringOrNull methods instead.")]
        [RockObsolete( "1.15" )]
        public string GetStringValue( string name, string defaultValue = null, bool trimWhiteSpace = true )
        {
            var value = defaultValue;
            if ( _settings.ContainsKey( name ) )
            {
                value = _settings[name] ?? string.Empty;
            }

            if ( trimWhiteSpace && value != null )
            {
                value = value.Trim();
            }

            return value;
        }

        #endregion

        /// <summary>
        /// Compare the set of attribute names against a set of known names and return those that do not match.
        /// </summary>
        /// <param name="matchNamesList"></param>
        /// <returns></returns>
        public List<string> GetUnmatchedAttributes( List<string> matchNamesList )
        {
            var unmatchedNames = new List<string>();

            if ( _settings == null )
            {
                unmatchedNames.AddRange( matchNamesList );
            }
            else if ( matchNamesList != null )
            {
                unmatchedNames.AddRange( _settings.Keys.AsQueryable().Except( matchNamesList, StringComparer.OrdinalIgnoreCase ) );
            }

            return unmatchedNames;
        }

        /// <summary>
        /// Parse the attributes markup of a Lava element tag to produce a collection of settings.
        /// </summary>
        /// <param name="attributesMarkup"></param>
        /// <param name="context"></param>
        public void ParseFromMarkup( string attributesMarkup, ILavaRenderContext context )
        {
            var newSettings = GetElementAttributes( attributesMarkup, context );

            // Apply the attribute values to the current settings.
            foreach ( var key in newSettings.Keys )
            {
                _settings[key.ToLower()] = newSettings[key];
            }
        }

        #region Static methods

        /// <summary>
        /// Returns an attribute dictionary configured with case-insensitive keys.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> GetConfiguredAttributeDictionary()
        {
            return new Dictionary<string, string>( StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Parses the attributes of the Lava element tag to extract the parameter settings.
        /// Any merge fields in the attributes markup are resolved before the settings are extracted.
        /// </summary>
        /// <param name="elementAttributesMarkup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetElementAttributes( string elementAttributesMarkup, ILavaRenderContext context = null )
        {
            // Get the set of parameters using variations of the following pattern:
            // param1:'value1 with spaces' param2:value2_without_spaces param3:'value3 with "embedded quotes"' param4:'value with {{ LavaFilter | 'lavaparameter1' }}'
            // A parameter key/value pair:
            // 1. Must be at the start of the parameter string or immediately preceded by whitespace.
            // 2. Must be terminated with a space that does not form part of the parameter value.
            // 3. Must contain a colon separating the key from the value.
            // 4. Must preserve the content of Lava tags.
            // 5. Has a case-insensitive Key.

            var parameters = GetConfiguredAttributeDictionary();

            if ( string.IsNullOrWhiteSpace( elementAttributesMarkup ) )
            {
                return parameters;
            }

            // Ensure that the markup string has no leading whitespace, and is terminated by a single whitespace character.
            elementAttributesMarkup = elementAttributesMarkup.Trim() + " ";

            var renderParameters = new LavaRenderParameters { Context = context };
            var engine = context?.GetService<ILavaEngine>();

            var delimiterCount = 0;
            var parameterStartIndex = 0;
            bool addKeyValuePair = false;
            var isLavaOutput = false;
            var isLavaTag = false;
            var isLavaShortcode = false;

            for ( int i = 0; i < elementAttributesMarkup.Length; i++ )
            {
                var thisChar = elementAttributesMarkup[i];

                // Process Lava open tags.
                if ( thisChar == '{' )
                {
                    if ( !( isLavaOutput || isLavaTag || isLavaShortcode ) )
                    {
                        var nextChar = i < elementAttributesMarkup.Length - 1 ? elementAttributesMarkup[i + 1] : ' ';
                        if ( nextChar == '{' )
                        {
                            isLavaOutput = true;
                        }
                        else if ( nextChar == '%' )
                        {
                            isLavaTag = true;
                        }
                        else if ( nextChar == '|' )
                        {
                            isLavaShortcode = true;
                        }
                    }
                    continue;
                }

                // Process Lava close tags.
                if ( thisChar == '}' )
                {
                    if ( isLavaOutput || isLavaTag || isLavaShortcode )
                    {
                        var nextChar = i < elementAttributesMarkup.Length - 1 ? elementAttributesMarkup[i + 1] : ' ';
                        if ( nextChar == '}' )
                        {
                            isLavaOutput = false;
                        }
                        else if ( nextChar == '%' )
                        {
                            isLavaTag = false;
                        }
                        else if ( nextChar == '|' )
                        {
                            isLavaShortcode = false;
                        }
                    }
                    continue;
                }

                if ( isLavaOutput || isLavaTag || isLavaShortcode )
                {
                    continue;
                }

                // Process Whitespace
                if ( thisChar == ' ' )
                {
                    if ( parameterStartIndex >= 0 )
                    {
                        if ( delimiterCount % 2 == 0 )
                        {
                            // If there is no open value delimiter, interpret whitespace as a parameter separator.
                            addKeyValuePair = true;

                        }
                    }
                }
                // Process single-quote delimiter.
                else if ( thisChar == '\'' )
                {
                    delimiterCount++;
                    if ( parameterStartIndex >= 0 )
                    {
                        // If parsing a value and there is no unpaired value delimiter, interpret whitespace as a parameter separator.
                        if ( delimiterCount % 2 == 0 )
                        {
                            addKeyValuePair = true;
                        }

                    }
                }
                else
                {
                    // For any other character, assume it is the start of a new parameter.
                    if ( parameterStartIndex == -1 )
                    {
                        parameterStartIndex = i;
                    }
                }

                if ( addKeyValuePair )
                {
                    string key;
                    string value;

                    var parameter = elementAttributesMarkup.Substring( parameterStartIndex, i - parameterStartIndex + 1 );

                    var separatorIndex = parameter.IndexOf( ':' );
                    if ( separatorIndex >= 0 )
                    {
                        key = parameter.Substring( 0, separatorIndex );
                        value = parameter.Substring( separatorIndex + 1 );
                    }
                    else
                    {
                        key = string.Empty;
                        value = parameter;
                    }

                    // Strip delimiters from value.
                    value = value.Trim( '\'' );

                    // Resolve any Lava markup in the parameter value.
                    if ( engine != null )
                    {
                        var result = engine.RenderTemplate( value, renderParameters );
                        value = result.Text;
                    }
                    else if ( LavaService.RockLiquidIsEnabled )
                    {
                        // If a Lava Engine is not configured, use the legacy RockLiquid implementation.
                        if ( context != null )
                        {
                            value = value.ResolveMergeFields( context.GetMergeFields() );
                        }
                    }

                    parameters[key] = value;

                    delimiterCount = 0;
                    parameterStartIndex = -1;
                    addKeyValuePair = false;
                }
            }

            return parameters;
        }
        #endregion

        #region RockLiquid Lava implementation

        /// <summary>
        /// Parse the attributes markup of a Lava element tag to produce a collection of settings.
        /// </summary>
        /// <param name="attributesMarkup"></param>
        /// <param name="context"></param>
        public void ParseFromMarkup( string attributesMarkup, Context context )
        {
            var newSettings = GetElementAttributes( attributesMarkup, new RockLiquidRenderContext( context ) );

            // Apply the attribute values to the current settings.
            foreach ( var key in newSettings.Keys )
            {
                _settings[key] = newSettings[key];
            }
        }

        /// <summary>
        /// Parses the attributes of the Lava element tag to extract the parameter settings.
        /// Any merge fields in the attributes markup are resolved before the settings are extracted.
        /// </summary>
        /// <param name="elementAttributesMarkup">The markup.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        [Obsolete( "Use GetElementAttributes( attributesMarkup, new RockLiquidRenderContext( context ) ) instead." )]
        [RockObsolete( "1.15" )]
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
