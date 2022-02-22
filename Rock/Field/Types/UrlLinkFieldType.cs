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
using System.Web.UI;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a numeric value
    /// </summary>
    [Serializable]
    [Rock.Attribute.RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
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

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues"></param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var shouldAlwaysShowCondensed = configurationValues.GetValueOrNull( ConfigurationKey.ShouldAlwaysShowCondensed ).AsBoolean();

            if ( string.IsNullOrWhiteSpace( value ) )
            {
                return string.Empty;
            }
            else
            {
                if ( condensed || shouldAlwaysShowCondensed )
                {
                    return value;
                }
                else
                {
                    return string.Format( "<a href='{0}'>{0}</a>", value );
                }
            }
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

    }
}