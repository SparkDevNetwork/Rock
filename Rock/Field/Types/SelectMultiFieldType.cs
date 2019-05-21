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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a comma-delimited list
    /// </summary>
    [Serializable]
    public class SelectMultiFieldType : FieldType
    {
        #region Configuration

        private const string VALUES_KEY = "values";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";
        private const string REPEAT_COLUMNS = "repeatColumns";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( VALUES_KEY );
            configKeys.Add( ENHANCED_SELECTION_KEY );
            configKeys.Add( REPEAT_COLUMNS );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var tbValues = new RockTextBox();
            tbValues.TextMode = TextBoxMode.MultiLine;
            tbValues.Rows = 3;
            tbValues.AutoPostBack = true;
            tbValues.TextChanged += OnQualifierUpdated;
            tbValues.Label = "Values";
            tbValues.Help = "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>.";
            controls.Add( tbValues );

            // option for Displaying an enhanced 'chosen' value picker
            var cbEnhanced = new RockCheckBox();
            cbEnhanced.AutoPostBack = true;
            cbEnhanced.CheckedChanged += OnQualifierUpdated;
            cbEnhanced.Label = "Enhance For Long Lists";
            cbEnhanced.Text = "Yes";
            cbEnhanced.Help = "When set, will render a searchable selection of options.";
            controls.Add( cbEnhanced );

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Columns";
            tbRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no upper limit enforced here however the block this is used in might add contraints due to available space.";
            tbRepeatColumns.MinimumValue = "0";
            tbRepeatColumns.AutoPostBack = true;
            tbRepeatColumns.TextChanged += OnQualifierUpdated;
            controls.Add( tbRepeatColumns );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = base.ConfigurationValues( controls );

            string description = "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>.";
            configurationValues.Add( VALUES_KEY, new ConfigurationValue( "Values", description, string.Empty ) );

            description = "When set, will render a searchable selection of options.";
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", description, string.Empty ) );

            description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue("Repeat Columns", description, string.Empty ) );

            if ( controls != null && controls.Count > 2 )
            {
                var tbValues = controls[0] as RockTextBox;
                var cbEnhanced = controls[1] as RockCheckBox;
                var tbRepeatColumns = controls[2] as NumberBox;

                tbRepeatColumns.Visible = !cbEnhanced.Checked;

                configurationValues[VALUES_KEY].Value = tbValues.Text;
                configurationValues[ENHANCED_SELECTION_KEY].Value = cbEnhanced.Checked.ToString();
                configurationValues[REPEAT_COLUMNS].Value = tbRepeatColumns.Visible ? tbRepeatColumns.Text : string.Empty;
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
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null && controls.Count > 2 && configurationValues != null )
            {
                var tbValues = controls[0] as RockTextBox;
                var cbEnhanced = controls[1] as RockCheckBox;
                var tbRepeatColumns = controls[2] as NumberBox;

                tbValues.Text = configurationValues.ContainsKey( VALUES_KEY ) ? configurationValues[VALUES_KEY].Value : string.Empty;
                cbEnhanced.Checked = configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) ? configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() : cbEnhanced.Checked;
                tbRepeatColumns.Text = configurationValues.ContainsKey( REPEAT_COLUMNS ) ? configurationValues[REPEAT_COLUMNS].Value : string.Empty;
                tbRepeatColumns.Visible = !cbEnhanced.Checked;
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
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( !string.IsNullOrWhiteSpace( value ) && configurationValues.ContainsKey( VALUES_KEY ) )
            {
                var configuredValues = Helper.GetConfiguredValues( configurationValues );
                var selectedValues = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                return configuredValues
                    .Where( v => selectedValues.Contains( v.Key ) )
                    .Select( v => v.Value )
                    .ToList()
                    .AsDelimited( ", " );
            }

            return base.FormatValue( parentControl, value, configurationValues, condensed );
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
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            if ( configurationValues != null )
            {
                ListControl editControl = null;

                if ( configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) && configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() )
                {
                    editControl = new RockListBox { ID = id };
                    ( (RockListBox)editControl ).DisplayDropAsAbsolute = true;
                }
                else
                {
                    editControl = new RockCheckBoxList { ID = id };
                    ( (RockCheckBoxList)editControl ).RepeatDirection = RepeatDirection.Horizontal;

                    if ( configurationValues.ContainsKey( REPEAT_COLUMNS ) )
                    {
                        ( ( RockCheckBoxList ) editControl ).RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
                    }
                }

                foreach( var keyVal in Helper.GetConfiguredValues( configurationValues ))
                {
                    editControl.Items.Add( new ListItem( keyVal.Value, keyVal.Key ) );
                }

                if ( editControl.Items.Count > 0 )
                {
                    return editControl;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            if ( control != null && control is ListControl )
            {
                ListControl cbl = (ListControl)control;
                foreach ( ListItem li in cbl.Items )
                {
                    if ( li.Selected )
                    {
                        values.Add( li.Value );
                    }
                }

                return values.AsDelimited<string>( "," );
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null )
            {
                List<string> values = new List<string>();
                values.AddRange( value.Split( ',' ) );

                if ( control != null && control is ListControl )
                {
                    ListControl cbl = (ListControl)control;
                    foreach ( ListItem li in cbl.Items )
                    {
                        li.Selected = values.Contains( li.Value );
                    }
                }
            }
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
            get
            {
                return ComparisonHelper.ContainsFilterComparisonTypes;
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
            // call the base which render SelectMulti's List
            return base.FilterValueControl( configurationValues, id, required, filterMode );
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            // call the base which will get SelectMulti's List values
            return base.GetFilterValueValue( control, configurationValues );
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            // call the base which will set SelectMulti's List values
            base.SetFilterValueValue( control, configurationValues, value );
        }

        /// <summary>
        /// Gets a filter expression for an entity property value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public override Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            // probably won't happen since MultiFieldType would only be a Attribute FieldType
            return base.PropertyFilterExpression( configurationValues, filterValues, parameterExpression, propertyName, propertyType );
        }

        /// <summary>
        /// Gets a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            Expression comparison = null;
            if ( filterValues.Count > 1 )
            {
                //// OR up the where clauses for each of the selected values 
                // and make sure to wrap commas around things so we don't collide with partial matches
                // so it'll do something like this:
                //
                // WHERE ',' + Value + ',' like '%,bacon,%'
                // OR ',' + Value + ',' like '%,lettuce,%'
                // OR ',' + Value + ',' like '%,tomato,%'

                // should be either "Contains" or "Not Contains"
                ComparisonType comparisonType = filterValues[0].ConvertToEnum<ComparisonType>( ComparisonType.Contains );

                // No comparison value was specified, so we can filter if the Comparison Type using no value still makes sense
                if ( ( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                {
                    // Just checking if IsBlank or IsNotBlank, so let ComparisonExpression do its thing
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, this.AttributeValueFieldName );
                    return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, AttributeConstantExpression( string.Empty ) );
                }

                List<string> selectedValues = filterValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                foreach ( var selectedValue in selectedValues )
                {
                    var searchValue = "," + selectedValue + ",";
                    var qryToExtract = new AttributeValueService( new Data.RockContext() ).Queryable().Where( a => ( "," + a.Value + "," ).Contains( searchValue ) );
                    var valueExpression = FilterExpressionExtractor.Extract<AttributeValue>( qryToExtract, parameterExpression, "a" );

                    if ( comparisonType != ComparisonType.Contains )
                    {
                        valueExpression = Expression.Not( valueExpression );
                    }

                    if ( comparison == null )
                    {
                        comparison = valueExpression;
                    }
                    else
                    {
                        comparison = Expression.Or( comparison, valueExpression );
                    }
                }
            }

            if ( comparison == null )
            {
                return new NoAttributeFilterExpression();
            }

            return comparison;
        }

        #endregion
    }
}