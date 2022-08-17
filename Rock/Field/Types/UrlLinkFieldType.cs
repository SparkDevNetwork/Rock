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
using System.Web.UI;

using Rock.Attribute;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a numeric value
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><g><path d=""M14.08,3.32a3.15,3.15,0,0,0-2.16-.92,3.08,3.08,0,0,0-2.28.92L8.38,4.56a4.07,4.07,0,0,1,.88.61l1.11-1.11a2.07,2.07,0,0,1,1.48-.61A2.1,2.1,0,0,1,13.34,7L10.86,9.51a2.16,2.16,0,0,1-3,0A2.13,2.13,0,0,1,7.28,8,2.1,2.1,0,0,1,7.58,7a1.4,1.4,0,0,0-1-.36h0a3.14,3.14,0,0,0,5,3.65l2.48-2.48A3.32,3.32,0,0,0,15,5.55,3.24,3.24,0,0,0,14.08,3.32Zm-8.45,8.6a2.07,2.07,0,0,1-1.48.61A2.1,2.1,0,0,1,2.66,9L5.14,6.47a2.16,2.16,0,0,1,3,0A2.13,2.13,0,0,1,8.72,8,2.1,2.1,0,0,1,8.42,9a1.45,1.45,0,0,0,.95.36h.05a3.14,3.14,0,0,0-2.8-4.57,3.12,3.12,0,0,0-2.22.92L1.92,8.22A3.15,3.15,0,0,0,4,13.6a3.21,3.21,0,0,0,2.4-.92l1.24-1.24a4.46,4.46,0,0,1-.88-.61Z""/></g></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.URL_LINK )]
    public class UrlLinkFieldType : FieldType
    {
        /// <summary>
        /// URL Link FieldType Configuration Keys
        /// </summary>
        public static class ConfigurationKey
        {
            /// <summary>
            /// The key for should require a trailing forward slash.
            /// </summary>
            public const string ShouldRequireTrailingForwardSlash = "ShouldRequireTrailingForwardSlash";

            /// <summary>
            /// The key for should always show condensed.
            /// </summary>
            public const string ShouldAlwaysShowCondensed = "ShouldAlwaysShowCondensed";
        }

        #region Configuration
        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            return new List<string>
            {
                ConfigurationKey.ShouldRequireTrailingForwardSlash,
                ConfigurationKey.ShouldAlwaysShowCondensed
            };
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = new List<Control>();

            var cbShouldRequireTrailingForwardSlash = new RockCheckBox();
            controls.Add( cbShouldRequireTrailingForwardSlash );
            cbShouldRequireTrailingForwardSlash.AutoPostBack = true;
            cbShouldRequireTrailingForwardSlash.Label = "Ensure Trailing Forward Slash";
            cbShouldRequireTrailingForwardSlash.Help = "When set, the URL must end with a forward slash (/) to be valid.";

            var cbShouldAlwaysShowCondensed = new RockCheckBox();
            controls.Add( cbShouldAlwaysShowCondensed );
            cbShouldAlwaysShowCondensed.AutoPostBack = true;
            cbShouldAlwaysShowCondensed.Label = "Should Always Show Condensed";
            cbShouldAlwaysShowCondensed.Help = "When set, the URL will always be returned as a raw value.";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ConfigurationKey.ShouldRequireTrailingForwardSlash, new ConfigurationValue( "Ensure Trailing Forward Slash",
                "When set, the URL must end with a forward slash (/) to be valid.", "false" ) );

            configurationValues.Add( ConfigurationKey.ShouldAlwaysShowCondensed,
                new ConfigurationValue( "Should Always Show Condensed", "When set, the URL will always be returned as a raw value.", "false" ) );

            if ( controls != null && controls.Count == 2 )
            {
                var cbShouldRequireTrailingForwardSlash = controls[0] as RockCheckBox;
                configurationValues[ConfigurationKey.ShouldRequireTrailingForwardSlash].Value = cbShouldRequireTrailingForwardSlash.Checked.ToString();

                var cbShouldAlwaysShowCondensed = controls[1] as RockCheckBox;
                configurationValues[ConfigurationKey.ShouldAlwaysShowCondensed].Value = cbShouldAlwaysShowCondensed.Checked.ToString();
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 2 && configurationValues != null )
            {
                var cbShouldRequireTrailingForwardSlash = controls[0] as RockCheckBox;

                if ( configurationValues.ContainsKey( ConfigurationKey.ShouldRequireTrailingForwardSlash ) )
                {
                    cbShouldRequireTrailingForwardSlash.Checked = configurationValues[ConfigurationKey.ShouldRequireTrailingForwardSlash].Value.AsBoolean();
                }

                var cbShouldAlwaysShowCondensed = controls[1] as RockCheckBox;

                if ( configurationValues.ContainsKey( ConfigurationKey.ShouldAlwaysShowCondensed ) )
                {
                    cbShouldAlwaysShowCondensed.Checked = configurationValues[ConfigurationKey.ShouldAlwaysShowCondensed].Value.AsBoolean();
                }
            }
        }
        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetHtmlValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var shouldAlwaysShowCondensed = privateConfigurationValues.GetValueOrNull( ConfigurationKey.ShouldAlwaysShowCondensed ).AsBoolean();

            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return string.Empty;
            }
            else
            {
                if ( shouldAlwaysShowCondensed )
                {
                    return privateValue;
                }
                else
                {
                    return string.Format( "<a href='{0}'>{0}</a>", privateValue );
                }
            }
        }

        /// <inheritdoc/>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var shouldAlwaysShowCondensed = configurationValues.GetValueOrNull( ConfigurationKey.ShouldAlwaysShowCondensed ).AsBoolean();
            var showCondensed = condensed || shouldAlwaysShowCondensed;

            // Original implementation returned HTML formatted string when not condensed
            // and the plain text string when condensed.
            return !showCondensed
               ? GetHtmlValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
               : GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }


        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var shouldRequireTrailingForwardSlash = configurationValues.GetValueOrNull( ConfigurationKey.ShouldRequireTrailingForwardSlash )?.AsBoolean();

            return new UrlLinkBox { ID = id, ShouldRequireTrailingForwardSlash = shouldRequireTrailingForwardSlash ?? false };
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value"></param>
        /// <param name="required"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool IsValid( string value, bool required, out string message )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                Uri validatedUri;
                if ( Uri.TryCreate( value, UriKind.Absolute, out validatedUri ) )
                {
                    message = "The link provided is not valid";
                    return true;
                }
            }

            return base.IsValid( value, required, out message );
        }

        #endregion

        #region FilterControl

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override Model.ComparisonType FilterComparisonType
        {
            get
            {
                return ComparisonHelper.StringFilterComparisonTypes;
            }
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override bool IsPersistedValueInvalidated( Dictionary<string, string> oldPrivateConfigurationValues, Dictionary<string, string> newPrivateConfigurationValues )
        {
            var oldShouldAlwaysShowCondensed = oldPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.ShouldAlwaysShowCondensed )?.AsBoolean() ?? false;
            var newShouldAlwaysShowCondensed = newPrivateConfigurationValues.GetValueOrNull( ConfigurationKey.ShouldAlwaysShowCondensed )?.AsBoolean() ?? false;

            if ( oldShouldAlwaysShowCondensed != newShouldAlwaysShowCondensed )
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}