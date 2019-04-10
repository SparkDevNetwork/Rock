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
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a text value
    /// </summary>
    [Serializable]
    public class TextFieldType : FieldType
    {

        #region Configuration

        private const string IS_PASSWORD_KEY = "ispassword";
        private const string MAX_CHARACTERS = "maxcharacters";
        private const string SHOW_COUNT_DOWN = "showcountdown";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( IS_PASSWORD_KEY );
            configKeys.Add( MAX_CHARACTERS );
            configKeys.Add( SHOW_COUNT_DOWN );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add checkbox for deciding if the textbox is used for storing a password
            var cbIsPasswordField = new RockCheckBox();
            controls.Add( cbIsPasswordField );
            cbIsPasswordField.AutoPostBack = true;
            cbIsPasswordField.CheckedChanged += OnQualifierUpdated;
            cbIsPasswordField.Label = "Password Field";
            cbIsPasswordField.Text = "Yes";
            cbIsPasswordField.Help = "When set, edit field will be masked.";

            // Add number box for selecting the maximum number of characters
            var nbMaxCharacters = new NumberBox();
            controls.Add( nbMaxCharacters );
            nbMaxCharacters.AutoPostBack = true;
            nbMaxCharacters.TextChanged += OnQualifierUpdated;
            nbMaxCharacters.NumberType = ValidationDataType.Integer;
            nbMaxCharacters.Label = "Max Characters";
            nbMaxCharacters.Help = "The maximum number of characters to allow. Leave this field empty to allow for an unlimited amount of text.";

            // Add checkbox indicating whether to show the count down.
            var cbShowCountDown = new RockCheckBox();
            controls.Add( cbShowCountDown );
            cbShowCountDown.AutoPostBack = true;
            cbShowCountDown.CheckedChanged += OnQualifierUpdated;
            cbShowCountDown.Label = "Show Character Limit Countdown";
            cbShowCountDown.Text = "Yes";
            cbShowCountDown.Help = "When set, displays a countdown showing how many characters remain (for the Max Characters setting).";

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
            configurationValues.Add( IS_PASSWORD_KEY, new ConfigurationValue( "Password Field", "When set, edit field will be masked.", "" ) );
            configurationValues.Add( MAX_CHARACTERS, new ConfigurationValue( "Max Characters", "The maximum number of characters to allow. Leave this field empty to allow for an unlimited amount of text.", "" ) );
            configurationValues.Add( SHOW_COUNT_DOWN, new ConfigurationValue( "Show Character Limit Countdown", "When set, displays a countdown showing how many characters remain (for the Max Characters setting).", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 )
                {
                    CheckBox cbIsPasswordField = controls[0] as CheckBox;
                    if ( cbIsPasswordField != null )
                    {
                        configurationValues[IS_PASSWORD_KEY].Value = cbIsPasswordField.Checked.ToString();
                    }
                }

                if ( controls.Count > 1 )
                {
                    NumberBox nbMaxCharacters = controls[1] as NumberBox;
                    if ( nbMaxCharacters != null )
                    {
                        configurationValues[MAX_CHARACTERS].Value = nbMaxCharacters.Text;
                    }
                }

                if ( controls.Count > 2 )
                {
                    CheckBox cbShowCountDown = controls[2] as CheckBox;
                    if ( cbShowCountDown != null )
                    {
                        configurationValues[SHOW_COUNT_DOWN].Value = cbShowCountDown.Checked.ToString();
                    }
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Determines whether the Attribute Configuration for this field has IsPassword = True
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public bool IsPassword( Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( configurationValues != null && configurationValues.ContainsKey( IS_PASSWORD_KEY ) )
            {
                return configurationValues[IS_PASSWORD_KEY].Value.AsBoolean();
            }

            return false;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count > 0 && configurationValues != null )
            {
                if ( controls.Count > 0 && configurationValues.ContainsKey( IS_PASSWORD_KEY ) )
                {
                    CheckBox cbIsPasswordField = controls[0] as CheckBox;
                    if ( cbIsPasswordField != null )
                    {
                        cbIsPasswordField.Checked = configurationValues[IS_PASSWORD_KEY].Value.AsBoolean();
                    }
                }

                if ( controls.Count > 1 && configurationValues.ContainsKey( MAX_CHARACTERS ) )
                {
                    NumberBox nbMaxCharacters = controls[1] as NumberBox;
                    if ( nbMaxCharacters != null )
                    {
                        nbMaxCharacters.Text = configurationValues[MAX_CHARACTERS].Value;
                    }
                }

                if ( controls.Count > 2 && configurationValues.ContainsKey( SHOW_COUNT_DOWN ))
                {
                    CheckBox cbShowCountDown = controls[2] as CheckBox;
                    if ( cbShowCountDown != null )
                    {
                        cbShowCountDown.Checked = configurationValues[SHOW_COUNT_DOWN].Value.AsBoolean();
                    }
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
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( configurationValues != null &&
                configurationValues.ContainsKey( IS_PASSWORD_KEY ) &&
                configurationValues[IS_PASSWORD_KEY].Value.AsBoolean() )
            {
                return "********";
            }

            if ( condensed )
            {
                return value.Truncate( 100 );
            }

            return value;
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // use un-condensed formatted value as the sort value
            return this.FormatValue( parentControl, value, configurationValues, false );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // NOTE: this really should not be encoding the value. FormatValueAsHtml method is really designed to wrap a value with appropriate html (i.e. convert an email into a mailto anchor tag)
            // but keeping it here for backward compatibility.
            return System.Web.HttpUtility.HtmlEncode( FormatValue( parentControl, value, configurationValues, condensed ) );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // NOTE: this really should not be encoding the value. FormatValueAsHtml method is really designed to wrap a value with appropriate html (i.e. convert an email into a mailto anchor tag)
            // but keeping it here for backward compatibility.
            return System.Web.HttpUtility.HtmlEncode( FormatValue( parentControl, entityTypeId, entityId, value, configurationValues, condensed ) );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The id.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            RockTextBox tb = base.EditControl( configurationValues, id ) as RockTextBox;

            if ( configurationValues != null )
            {

                if ( configurationValues.ContainsKey( IS_PASSWORD_KEY ) &&
                    configurationValues[IS_PASSWORD_KEY].Value.AsBoolean() )
                {
                    tb.TextMode = TextBoxMode.Password;
                }

                if ( configurationValues.ContainsKey( MAX_CHARACTERS ) )
                {
                    int? maximumLength = configurationValues[MAX_CHARACTERS].Value.AsIntegerOrNull();
                    if ( maximumLength.HasValue )
                    {
                        tb.MaxLength = maximumLength.Value;
                    }
                }

                if ( configurationValues.ContainsKey( SHOW_COUNT_DOWN ) )
                {
                    tb.ShowCountDown = configurationValues[SHOW_COUNT_DOWN].Value.AsBoolean();
                }
            }
            return tb;
        }

        #endregion

        #region FilterControl

        /// <summary>
        /// Gets the filter compare control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            if ( filterMode == FilterMode.SimpleFilter )
            {
                // hide the compare control for SimpleFilter mode
                RockDropDownList ddlCompare = ComparisonHelper.ComparisonControl( FilterComparisonType, required );
                ddlCompare.ID = string.Format( "{0}_ddlCompare", id );
                ddlCompare.AddCssClass( "js-filter-compare" );
                ddlCompare.Visible = false;
                return ddlCompare;
            }
            else
            {
                return base.FilterCompareControl( configurationValues, id, required, filterMode );
            }
        }

        /// <summary>
        /// Determines whether [has filter control].
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
        }

        /// <summary>
        /// Gets the filter values.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
        {
            // If this is a simple filter, only return values if something was actually entered into the filter's text field
            var values = base.GetFilterValues( filterControl, configurationValues, filterMode );
            if ( filterMode == FilterMode.SimpleFilter &&
                values.Count == 2 &&
                values[0].ConvertToEnum<ComparisonType>() == ComparisonType.Contains &&
                values[1] == "" )
            {
                return new List<string>();
            }

            return values;
        }

        /// <summary>
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            bool filterValueControlVisible = true;
            var filterField = control.FirstParentControlOfType<FilterField>();
            if ( filterField != null && filterField.HideFilterCriteria )
            {
                filterValueControlVisible = false;
            }

            if ( filterMode == FilterMode.SimpleFilter && filterValueControlVisible )
            {
                // hard code to Contains when in SimpleFilter mode and the FilterValue control is visible
                return ComparisonType.Contains.ConvertToInt().ToString();
            }
            else
            {
                return base.GetFilterCompareValue( control, filterMode );
            }
        }

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