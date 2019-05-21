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
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to edit text in Markdown format and rendered as processed Markdown
    /// </summary>
    public class MarkdownFieldType : FieldType
    {
        #region Configuration

        private const string NUMBER_OF_ROWS = "numberofrows";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( NUMBER_OF_ROWS );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add number box for selecting the number of rows
            var nb = new NumberBox();
            controls.Add( nb );
            nb.AutoPostBack = true;
            nb.TextChanged += OnQualifierUpdated;
            nb.NumberType = ValidationDataType.Integer;
            nb.Label = "Rows";
            nb.Help = "The number of rows to display (default is 3).";

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

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is NumberBox )
                {
                    configurationValues[NUMBER_OF_ROWS].Value = ( (NumberBox)controls[0] ).Text;
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
            var result = value.ConvertMarkdownToHtml();

            if ( condensed )
            {
                // TrimEnd is added because ConvertMarkdownToHtml add two newlines. Described in: https://github.com/Knagis/CommonMark.NET/issues/107
                result = result?.Trim() ?? string.Empty;
                // Remove paragraph tags for values in grids and filter indicators to remove unnecessary whitespace above and below the value.
                if ( result.StartsWith("<p>") && result.EndsWith("</p>"))
                {
                    result = result.Substring( 3, result.Length - 7 );
                }

                return result.Truncate( 100 );
            }

            return result;
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
            var mdEditor = new MarkdownEditor { ID = id };
            int? rows = 3;
            bool allowHtml = false;

            if ( configurationValues != null )
            {
                if ( configurationValues.ContainsKey( NUMBER_OF_ROWS ) )
                {
                    rows = configurationValues[NUMBER_OF_ROWS].Value.AsIntegerOrNull() ?? 3;
                }
            }

            mdEditor.Rows = rows.HasValue ? rows.Value : 3;
            mdEditor.ValidateRequestMode = allowHtml ? ValidateRequestMode.Disabled : ValidateRequestMode.Enabled;

            return mdEditor;
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
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var tbValue = new RockTextBox();
            tbValue.ID = string.Format( "{0}_ctlCompareValue", id );
            tbValue.AddCssClass( "js-filter-control" );
            return tbValue;
        }

        #endregion
    }
}