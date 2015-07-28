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
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a rating
    /// </summary>
    [Serializable]
    public class RatingFieldType : FieldType
    {

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( "max" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var nb = new NumberBox();
            controls.Add( nb );
            nb.NumberType = System.Web.UI.WebControls.ValidationDataType.Integer;
            nb.AutoPostBack = true;
            nb.TextChanged += OnQualifierUpdated;
            nb.Label = "Max Rating";
            nb.Help = "The number of stars ( max rating ) that should be displayed.";
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
            configurationValues.Add( "max", new ConfigurationValue( "Max Rating", "The number of stars ( max rating ) that should be displayed.", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is NumberBox )
                    configurationValues["max"].Value = ( (NumberBox)controls[0] ).Text;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null &&
                controls[0] != null && controls[0] is NumberBox && configurationValues.ContainsKey( "max" ) )
                ( (NumberBox)controls[0] ).Text = configurationValues["max"].Value;
        }

        /// <summary>
        /// Gets the maximum rating.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        private int GetMaxRating( Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( configurationValues != null && configurationValues.ContainsKey( "max" ) )
            {
                int max = configurationValues["max"].Value.AsInteger();
                if ( max > 0)
                {
                    return max;
                }
            }

            return 5;
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condsed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            int rating = value.AsInteger();
            var sb = new StringBuilder();
            for ( int i = 1; i <= GetMaxRating( configurationValues ); i++ )
            {
                sb.AppendFormat( "<i class='fa fa-star{0}'></i>", i > rating ? "-o" : "" );
            }

            return sb.ToString();
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            int max = GetMaxRating(configurationValues);
            return new RockRating { ID = id, Max = max }; 
        }


        #endregion

        #region Filter Control

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public override ComparisonType FilterComparisonType
        {
            get { return ComparisonHelper.NumericFilterComparisonTypes; }
        }

        /// <summary>
        /// Gets the filter value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required"></param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var panel = new Panel();
            panel.ID = string.Format( "{0}_ctlComparePanel", id );
            panel.AddCssClass( "js-filter-control" );

            var control = EditControl( configurationValues, id );
            control.ID = string.Format( "{0}_ctlCompareValue", id );
            panel.Controls.Add( control );

            return panel;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return base.GetFilterValueValue( control.Controls[0], configurationValues );
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            base.SetFilterValueValue( control.Controls[0], configurationValues, value );
        }

        /// <summary>
        /// Gets the filter format script.
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        /// <remarks>
        /// This script must set a javascript variable named 'result' to a friendly string indicating value of filter controls
        /// a '$selectedContent' should be used to limit script to currently selected filter fields
        /// </remarks>
        public override string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            return string.Format( "result = '{0} ' + $('select', $selectedContent).find(':selected').text() + ( $('.js-filter-control', $selectedContent).filter(':visible').length ?  (' \\'' +  $('input', $selectedContent).val()  + '\\'') : '' )", titleJs );
        }

        #endregion

    }
}