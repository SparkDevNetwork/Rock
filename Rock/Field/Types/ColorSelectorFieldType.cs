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
using System.Text;
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of System.Drawing.Color options
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms | Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M4.48,7.12A.87.87,0,0,0,3.6,8a.88.88,0,1,0,.88-.88Zm8-4.53A7.05,7.05,0,0,0,6.59,1.14,6.94,6.94,0,0,0,1.14,6.56,7,7,0,0,0,3,12.87,6.58,6.58,0,0,0,7.58,15l.72,0a2,2,0,0,0,1.53-1.13,2.3,2.3,0,0,0,0-2.09,1,1,0,0,1,.05-1,1,1,0,0,1,.87-.5h2A2.26,2.26,0,0,0,15,8,7,7,0,0,0,12.45,2.59Zm.29,6.28h-2a2.34,2.34,0,0,0-2,1.13,2.37,2.37,0,0,0-.09,2.32,1,1,0,0,1,0,.9.79.79,0,0,1-.56.43A5.05,5.05,0,0,1,3.94,12,5.71,5.71,0,0,1,2.43,6.85,5.57,5.57,0,0,1,6.84,2.47a5.73,5.73,0,0,1,4.77,1.18A5.65,5.65,0,0,1,13.69,8,.94.94,0,0,1,12.74,8.87ZM5.35,4.48a.87.87,0,1,0,.88.87A.85.85,0,0,0,5.35,4.48ZM8,3.62a.88.88,0,1,0,.87.87A.86.86,0,0,0,8,3.62Zm2.62.86a.87.87,0,1,0,.88.87A.85.85,0,0,0,10.6,4.48Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.COLOR_SELECTOR )]
    public class ColorSelectorFieldType : FieldType, ISplitMultiValueFieldType
    {
        private static readonly char ValueDelimiter = '|';

        #region Configuration

        /// <summary>
        /// Color Selector FieldType Configuration Keys
        /// </summary>
        public static class ConfigurationKey
        {
            /// <summary>
            /// The key for colors
            /// </summary>
            public const string Colors = "colors";

            /// <summary>
            /// The key for allowing multiple color selection
            /// </summary>
            public const string AllowMultiple = "allowMultiple";
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldValues = oldPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.Colors ) ?? string.Empty;
            var newValues = newPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.Colors ) ?? string.Empty;

            if ( oldValues != newValues )
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetSelectedValues( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetCondensedHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetSelectedValues( privateValue, privateConfigurationValues ).EncodeHtml();
        }

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace()
                 || privateConfigurationValues?.ContainsKey( ConfigurationKey.Colors ) != true
                 || privateConfigurationValues[ConfigurationKey.Colors].IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var colors = DeserializeColors( privateConfigurationValues[ConfigurationKey.Colors] );
            var values = DeserializeValue( privateValue );
            var htmlStringBuilder = new StringBuilder();
            var jsLines = new List<string>();

            htmlStringBuilder.AppendLine( "<div class='color-selector-items'>" );

            var selectedColors = GetSelectedColors( colors, values );
            foreach ( var color in selectedColors )
            {
                var guid = Guid.NewGuid();
                var containerId = $"color-selector-item-container-{guid}";
                var itemId = $"color-selector-item-{guid}";

                htmlStringBuilder.AppendLine( $"<div id='{containerId}' class='color-selector-item-container'>" );
                htmlStringBuilder.AppendLine( $"<div id='{itemId}' class='color-selector-item readonly' style='background-color: {color};'>" );
                htmlStringBuilder.AppendLine( "</div>" );
                htmlStringBuilder.AppendLine( "</div>" );

                // These JS statements will be added in a single script tag at the end.
                jsLines.Add( $"Rock.controls.colorSelector.setCamouflagedClass('#{itemId}', '#{containerId}');" );
            }

            htmlStringBuilder.AppendLine( "</div>" );

            if ( jsLines.Any() )
            {
                htmlStringBuilder.AppendLine( "<script>" );
                htmlStringBuilder.AppendLine( "$(function() {" );
                foreach ( var jsLine in jsLines )
                {
                    htmlStringBuilder.AppendLine( jsLine );
                }
                htmlStringBuilder.AppendLine( "});" );
                htmlStringBuilder.AppendLine( "</script>" );
            }

            return htmlStringBuilder.ToString();
        }

        #endregion

        #region Edit Control

        #endregion

        #region ISplitMultiValueFieldType

        /// <inheritdoc/>
        public ICollection<string> SplitMultipleValues( string privateValue )
        {
            return DeserializeValue( privateValue );
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the selected values. (These are not the keys!)
        /// </summary>
        /// <param name="privateValue">The private value.</param>
        /// <param name="privateConfigurationValues">The private configuration values.</param>
        /// <returns></returns>
        private string GetSelectedValues( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( privateValue.IsNullOrWhiteSpace()
                 || !privateConfigurationValues.ContainsKey( ConfigurationKey.Colors )
                 || privateConfigurationValues[ConfigurationKey.Colors].IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            var colors = DeserializeColors( privateConfigurationValues[ConfigurationKey.Colors] );
            var values = DeserializeValue( privateValue );

            return GetSelectedColors( colors, values ).AsDelimited( "," );
        }

        private ListItems GetColorsConfigurationControl( List<Control> controls )
        {
            if ( controls?.Any() == true && controls[0] is ListItems listItems )
            {
                return listItems;
            }

            return null;
        }

        /// <summary>
        /// Gets the selected colors.
        /// </summary>
        /// <param name="colors">The available colors.</param>
        /// <param name="values">The selected colors.</param>
        /// <returns>The selected colors.</returns>
        private List<string> GetSelectedColors( List<string> colors, List<string> values )
        {
            return colors
                .Where( color => values.Any( value => string.Equals( value, color, StringComparison.OrdinalIgnoreCase ) ) )
                .ToList();
        }

        /// <summary>
        /// Deserializes a list of color values.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The deserialized color values.</returns>
        private List<string> DeserializeValue( string input )
        {
            if ( input.IsNullOrWhiteSpace() )
            {
                return new List<string>();
            }

            return input.Split( new char[] { ValueDelimiter }, StringSplitOptions.RemoveEmptyEntries ).ToList();
        }

        /// <summary>
        /// Serializes a list of color values.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The serialized color values.</returns>
        private string SerializeValue( List<string> input )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            return string.Join( $"{ValueDelimiter}", input );
        }

        /// <summary>
        /// Returns a boolean from a serialized config value that indicates if multiple selection is allowed.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A value indicating if multiple selection is allowed.</returns>
        private bool DeserializeAllowMultiple( string input )
        {
            return input.AsBoolean();
        }

        /// <summary>
        /// Deserializes a list of colors.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The deserialized colors.</returns>
        private List<string> DeserializeColors( string input )
        {
            if ( input.IsNullOrWhiteSpace() )
            {
                return new List<string>();
            }

            return input.Split( new char[] { ValueDelimiter }, StringSplitOptions.RemoveEmptyEntries ).ToList();
        }

        /// <summary>
        /// Serializes a list of colors.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>The serialized colors.</returns>
        private string SerializeColors( List<string> input )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            return string.Join( $"{ValueDelimiter}", input );
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <inheritdoc/>
        public override List<string> ConfigurationKeys()
        {
            return new List<string>
            {
                ConfigurationKey.Colors,
                ConfigurationKey.AllowMultiple
            };
        }

        /// <inheritdoc/>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var li = new ListItems
            {
                Label = "Colors",
                Help = "The hex colors to select from."
            };

            // Update the default value color selector whenever items in the colors list change.
            li.ValueChanged += OnQualifierUpdated;
            controls.Add( li );

            return controls;
        }

        /// <inheritdoc/>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>
            {
                { ConfigurationKey.Colors, new ConfigurationValue( "Colors", "The hex colors to select from.", string.Empty ) }
            };

            var colorsControl = GetColorsConfigurationControl( controls );
            if ( colorsControl != null )
            {
                var keyValuePairs = colorsControl.Value.FromJsonOrNull<List<ListItems.KeyValuePair>>() ?? new List<ListItems.KeyValuePair>();
                var colors = keyValuePairs.Select( kvp => kvp.Value ).ToList();
                configurationValues[ConfigurationKey.Colors].Value = SerializeColors( colors );
            }

            return configurationValues;
        }

        /// <inheritdoc/>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls == null && configurationValues == null )
            {
                return;
            }

            if ( configurationValues.ContainsKey( ConfigurationKey.Colors ) )
            {
                var colorsControl = GetColorsConfigurationControl( controls );

                if ( colorsControl != null )
                {
                    var colors = DeserializeColors( configurationValues[ConfigurationKey.Colors].Value );
                    var keyValuePairs = colors.Select( color => new ListItems.KeyValuePair
                        {
                            Value = color,
                            Key = Guid.NewGuid()
                        } )
                        .ToList();
                    colorsControl.Value = keyValuePairs.ToJson();
                }
            }
        }

        /// <inheritdoc/>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <inheritdoc/>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            if ( configurationValues == null )
            {
                return null;
            }

            var editControl = new ColorSelector
            {
                ID = id,
                AutoPostBack = false,
                AllowMultiple = DeserializeAllowMultiple( configurationValues.GetValueOrNull( ConfigurationKey.AllowMultiple ) )
            };

            if ( configurationValues.ContainsKey( ConfigurationKey.Colors ) )
            {
                var colors = DeserializeColors( configurationValues.GetValueOrNull( ConfigurationKey.Colors ) );

                foreach ( var color in colors )
                {
                    editControl.Items.Add( new ListItem
                    {
                        Text = color,
                        Value = color
                    } );
                }
            }

            return editControl;
        }

        /// <inheritdoc/>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control == null || !( control is ListControl editControl ) )
            {
                return null;
            }

            var values = new List<string>();

            // Explicit ListItem type is required here since ListControl.Items implements the non-generic IEnumerable interface.
            foreach ( ListItem li in editControl.Items )
            {
                if ( li.Selected )
                {
                    values.Add( li.Value );
                }
            }

            return SerializeValue( values );
        }

        /// <inheritdoc/>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value == null || !( control is ListControl editControl ) )
            {
                return;
            }

            var values = DeserializeValue( value );

            // Explicit ListItem type is required here since ListControl.Items implements the non-generic IEnumerable interface.
            foreach ( ListItem li in editControl.Items )
            {
                li.Selected = values.Contains( li.Value );
            }
        }

#endif
        #endregion
    }
}