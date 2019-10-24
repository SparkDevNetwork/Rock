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

using Rock.Security;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a text value
    /// </summary>
    [Serializable]
    public class EncryptedTextFieldType : TextFieldType
    {

        #region Configuration

        private const string NUMBER_OF_ROWS = "numberofrows";
        private const string ALLOW_HTML = "allowhtml";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            // Process TextFieldType first.
            var configKeys = base.ConfigurationKeys();

            configKeys.Add( NUMBER_OF_ROWS );
            configKeys.Add( ALLOW_HTML );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            // Process TextFieldType first.
            var controls = base.ConfigurationControls();

            // Add number box for selecting the number of rows
            var nb = new NumberBox();
            controls.Add( nb );
            nb.AutoPostBack = true;
            nb.TextChanged += OnQualifierUpdated;
            nb.NumberType = ValidationDataType.Integer;
            nb.Label = "Rows";
            nb.Help = "The number of rows to display (note selecting a value greater than 1 will override the Password Field setting).";

            // Allow HTML
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Allow HTML";
            cb.Text = "Yes";
            cb.Help = "Controls whether server should prevent HTML from being entered in this field or not.";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            // Process TextFieldType first.
            var configurationValues = base.ConfigurationValues( controls );

            configurationValues.Add( NUMBER_OF_ROWS, new ConfigurationValue( "Rows", "The number of rows to display (note selecting a value greater than 1 will override the Password Field setting).", "" ) );
            configurationValues.Add( ALLOW_HTML, new ConfigurationValue( "Allow HTML", "Controls whether server should prevent HTML from being entered in this field or not.", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 3 && controls[3] != null && controls[3] is NumberBox )
                {
                    configurationValues[NUMBER_OF_ROWS].Value = ( (NumberBox)controls[3] ).Text;
                }
                if ( controls.Count > 4 && controls[4] != null && controls[4] is RockCheckBox )
                {
                    configurationValues[ALLOW_HTML].Value = ( (RockCheckBox)controls[4] ).Checked.ToString();
                }
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
            // Process TextFieldType first.
            base.SetConfigurationValues( controls, configurationValues );
            
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 3 && controls[3] != null && controls[3] is NumberBox && configurationValues.ContainsKey( NUMBER_OF_ROWS ) )
                {
                    ( (NumberBox)controls[3] ).Text = configurationValues[NUMBER_OF_ROWS].Value;
                }

                if ( controls.Count > 4 && controls[4] != null && controls[4] is RockCheckBox && configurationValues.ContainsKey( ALLOW_HTML ) )
                {
                    ( (RockCheckBox)controls[4] ).Checked = configurationValues[ALLOW_HTML].Value.AsBoolean();
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
            return base.FormatValue( parentControl, Encryption.DecryptString( value ), configurationValues, condensed );
        }

        /// <summary>
        /// Setting to determine whether the value from this control is sensitive.  This is used for determining
        /// whether or not the value of this attribute is logged when changed.
        /// </summary>
        /// <returns>
        ///   <c>false</c> By default, any field is not sensitive.
        /// </returns>
        public override bool IsSensitive()
        {
            return true;
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
            // Process TextFieldType first.
            var tb = base.EditControl( configurationValues, id ) as RockTextBox;

            if ( tb != null && configurationValues != null )
            {
                if ( configurationValues.ContainsKey( NUMBER_OF_ROWS ) )
                {
                    int? rows = configurationValues[NUMBER_OF_ROWS].Value.AsIntegerOrNull() ?? 1;
                    if ( rows.HasValue && rows.Value > 1 )
                    {
                        tb.TextMode = TextBoxMode.MultiLine;
                        tb.Rows = rows.Value;
                    }
                }
                if ( configurationValues.ContainsKey( ALLOW_HTML ) )
                {
                    tb.ValidateRequestMode = configurationValues[ALLOW_HTML].Value.AsBoolean() ?
                        ValidateRequestMode.Disabled : ValidateRequestMode.Enabled;
                }

            }

            return tb;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return Encryption.EncryptString( base.GetEditValue( control, configurationValues ) );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            base.SetEditValue( control, configurationValues, Encryption.DecryptString( value ) );
        }

        #endregion

        #region Filter Control

        // Note: Even though this is a 'text' type field, the comparisons like 'Starts with', 'Contains', etc. can't be performed
        // on the encrypted text. Every time the same value is encrypted, the value is different. So a binary comparison cannot be performed.

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override System.Web.UI.Control FilterControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, Rock.Reporting.FilterMode filterMode )
        {
            // This field type does not support filtering
            return null;
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return false;
        }

        #endregion

    }
}