// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class MemoFieldType : FieldType
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
            var controls = base.ConfigurationControls();

            // Add nuber box for selecting the number of rows
            var nb = new NumberBox();
            controls.Add( nb );
            nb.AutoPostBack = true;
            nb.TextChanged += OnQualifierUpdated;
            nb.NumberType = ValidationDataType.Integer;
            nb.Label = "Rows";
            nb.Help = "The number of rows to display (default is 3).";

            // Allow HTML
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Allow HTML";
            cb.Text = "Yes";
            cb.Help = "Controls whether server shold prevent HTML from being entered in this field or not.";

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
            configurationValues.Add( NUMBER_OF_ROWS, new ConfigurationValue( "Rows", "The number of rows to display (default is 3).", "" ) );
            configurationValues.Add( ALLOW_HTML, new ConfigurationValue( "Allow HTML", "Controls whether server shold prevent HTML from being entered in this field or not.", "" ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is NumberBox )
                {
                    configurationValues[NUMBER_OF_ROWS].Value = ( (NumberBox)controls[0] ).Text;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockCheckBox )
                {
                    configurationValues[ALLOW_HTML].Value = ( (RockCheckBox)controls[1] ).Checked.ToString();
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
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is NumberBox && configurationValues.ContainsKey( NUMBER_OF_ROWS ) )
                {
                    ( (NumberBox)controls[0] ).Text = configurationValues[NUMBER_OF_ROWS].Value;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is RockCheckBox && configurationValues.ContainsKey( ALLOW_HTML ) )
                {
                    ( (RockCheckBox)controls[1] ).Checked = configurationValues[ALLOW_HTML].Value.AsBoolean();
                }
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condesed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return base.FormatValueAsHtml( parentControl, value, configurationValues, condensed ).ConvertCrLfToHtmlBr();
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
            RockTextBox tb = new RockTextBox { ID = id, TextMode = TextBoxMode.MultiLine };
            int? rows = 3;
            bool allowHtml = false;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( NUMBER_OF_ROWS ) )
                {
                    rows = configurationValues[NUMBER_OF_ROWS].Value.AsIntegerOrNull() ?? 3;
                }
                if ( configurationValues.ContainsKey( ALLOW_HTML ) )
                {
                    allowHtml = configurationValues[ALLOW_HTML].Value.AsBoolean();
                }
            }

            tb.Rows = rows.HasValue ? rows.Value : 3;
            tb.ValidateRequestMode = allowHtml ? ValidateRequestMode.Disabled : ValidateRequestMode.Enabled;

            return tb;
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

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            var tbValue = new RockTextBox();
            tbValue.ID = string.Format( "{0}_ctlCompareValue", id );
            tbValue.AddCssClass( "js-filter-control" );
            return tbValue;
        }

        #endregion

    }
}