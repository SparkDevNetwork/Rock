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

using Newtonsoft.Json.Linq;

using Rock.Cms.ThemeFields;
using Rock.Enums.Cms;

namespace Rock.Cms
{
    /// <summary>
    /// Defines the structure of a theme definition file that is included in
    /// next generation themes.
    /// </summary>
    internal class ThemeDefinition
    {
        #region Properties

        /// <summary>
        /// The name of the theme.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// A description that describes that this theme provides or should be
        /// used for.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The fields that can be displayed in the UI to customize the theme.
        /// </summary>
        public List<ThemeField> Fields { get; private set; }

        /// <summary>
        /// The purpose of the theme. This is used to determine how any
        /// compilation works as well as other features.
        /// </summary>
        public ThemePurpose Purpose { get; private set; }

        /// <summary>
        /// The font icon sets that are supported by this theme.
        /// </summary>
        public ThemeIconSet AvailableIconSets { get; private set; }

        #endregion

        #region Parsing Methods

        /// <summary>
        /// Parse the JSON content of a theme definition file. If the JSON is
        /// not valid then an exception will be thrown.
        /// </summary>
        /// <param name="json">The JSON contained in the theme definition file.</param>
        /// <returns>An new instance of <see cref="ThemeDefinition"/>.</returns>
        public static ThemeDefinition Parse( string json )
        {
            if ( json == null )
            {
                throw new ArgumentNullException( nameof( json ) );
            }

            // We have to manually parse the theme definition because it uses
            // discriminated unions for the fields. While it is certainly possible
            // to do that with custom deserializers, this seemed easier for now.
            var jRoot = json.FromJsonOrNull<JObject>();
            var name = jRoot?.GetValue( "name" )?.ToString();
            var purpose = ParsePurposeString( jRoot?.GetValue( "purpose" )?.ToString() );
            var availableIconSets = ParseIconSetsString( jRoot?.GetValue( "availableIconSets" ) );

            if ( jRoot == null )
            {
                throw new FormatException( "Invalid theme definition." );
            }

            if ( name.IsNullOrWhiteSpace() )
            {
                throw new FormatException( "Theme is missing 'name' property." );
            }

            return new ThemeDefinition
            {
                Name = name,
                Description = jRoot.GetValue( "description" )?.ToString() ?? string.Empty,
                Fields = ParseFields( jRoot.GetValue( "fields" ) ),
                Purpose = purpose,
                AvailableIconSets = availableIconSets
            };
        }

        /// <summary>
        /// Attempts to parse the JSON content of a theme definition file.
        /// </summary>
        /// <param name="json">The JSON contained in the theme definition file.</param>
        /// <param name="theme">On return will contain either a new instance of <see cref="ThemeDefinition"/> if successful; otherwise <c>null</c>.</param>
        /// <returns><c>true</c> if the JSON was valid and parsed; otherwise <c>false</c>.</returns>
        public static bool TryParse( string json, out ThemeDefinition theme )
        {
            try
            {
                theme = Parse( json );
                return true;
            }
            catch
            {
                theme = null;
                return false;
            }
        }

        /// <summary>
        /// Parses a string of text as a <see cref="ThemePurpose"/>.
        /// </summary>
        /// <param name="purposeString">The string that contains the purpose text value.</param>
        /// <returns>An instance of <see cref="ThemePurpose"/> that represents the value.</returns>
        private static ThemePurpose ParsePurposeString( string purposeString )
        {
            if ( purposeString == "web" || purposeString == null )
            {
                return ThemePurpose.Web;
            }
            else if ( purposeString == "checkin" )
            {
                return ThemePurpose.Checkin;
            }
            else
            {
                throw new FormatException( $"Theme purpose '{purposeString}' is not valid." );
            }
        }

        /// <summary>
        /// Parses a list of strings as a <see cref="ThemeIconSet"/>.
        /// </summary>
        /// <param name="iconSetsToken">The list of strings that contains the icon set text values.</param>
        /// <returns>An instance of <see cref="ThemeIconSet"/> that represents the value.</returns>
        private static ThemeIconSet ParseIconSetsString( JToken iconSetsToken )
        {
            ThemeIconSet iconSets = 0;

            if ( iconSetsToken != null )
            {
                if ( iconSetsToken.Type != JTokenType.Array )
                {
                    throw new FormatException( $"Property 'availableIconSets' must be an array." );
                }

                var iconSetsStrings = iconSetsToken.ToObject<List<string>>();

                foreach ( var iconSetString in iconSetsStrings )
                {
                    if ( iconSetString == "fontawesome" )
                    {
                        iconSets |= ThemeIconSet.FontAwesome;
                    }
                    else if ( iconSetString == "tabler" )
                    {
                        iconSets |= ThemeIconSet.Tabler;
                    }
                    else
                    {
                        throw new FormatException( $"Theme icon set '{iconSetString}' is not valid." );
                    }
                }
            }
            else
            {
                return ThemeIconSet.FontAwesome;
            }

            return iconSets;
        }

        /// <summary>
        /// Parses the raw tokens that represent a set of fields contained in
        /// either the theme definition or a field panel.
        /// </summary>
        /// <param name="jFields">The token that represents the raw field data.</param>
        /// <returns>A list of <see cref="ThemeField"/> instances.</returns>
        internal static List<ThemeField> ParseFields( JToken jFields )
        {
            if ( jFields == null )
            {
                return new List<ThemeField>();
            }

            if ( !( jFields is JArray jFieldsArray ) )
            {
                throw new FormatException( $"Theme 'fields' property was expected to be an array but was {jFields.Type}." );
            }

            var fields = new List<ThemeField>();

            foreach ( var jField in jFieldsArray )
            {
                if ( !( jField is JObject jFieldObject ) )
                {
                    throw new FormatException( $"Theme field was expected to be an object but was {jField.Type}." );
                }

                fields.Add( ParseField( jFieldObject ) );
            }

            return fields;
        }

        /// <summary>
        /// Parses a single field for use in a theme definition. This will
        /// return one of the concrete subclasses of <see cref="ThemeField"/>.
        /// </summary>
        /// <param name="jField">The raw details that describe the field to parse.</param>
        /// <returns>A concrete subclass of <see cref="ThemeField"/>.</returns>
        internal static ThemeField ParseField( JObject jField )
        {
            var type = jField.GetValue( "type" )?.ToString();

            if ( type == "literal" )
            {
                return new LiteralThemeField( jField, ThemeFieldType.Literal );
            }
            else if ( type == "color" )
            {
                return new ColorThemeField( jField, ThemeFieldType.Color );
            }
            else if ( type == "image" )
            {
                return new ImageThemeField( jField, ThemeFieldType.Image );
            }
            else if ( type == "text" )
            {
                return new TextThemeField( jField, ThemeFieldType.Text );
            }
            else if ( type == "file" )
            {
                return new FileThemeField( jField, ThemeFieldType.File );
            }
            else if ( type == "switch" )
            {
                return new SwitchThemeField( jField, ThemeFieldType.Switch );
            }
            else if ( type == "heading" )
            {
                return new HeadingThemeField( jField, ThemeFieldType.Heading );
            }
            else if ( type == "spacer" )
            {
                return new SpacerThemeField( jField, ThemeFieldType.Spacer );
            }
            else if ( type == "panel" )
            {
                return new PanelThemeField( jField, ThemeFieldType.Panel );
            }
            else
            {
                throw new FormatException( $"Unknown field type '{type}' found." );
            }
        }

        #endregion
    }
}
