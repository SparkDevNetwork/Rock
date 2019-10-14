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

using Newtonsoft.Json;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

using static Rock.Web.UI.Controls.ListItems;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a comma-delimited list
    /// </summary>
    [Serializable]
    public class CheckListFieldType : FieldType
    {
        #region Configuration

        private const string VALUES_KEY = "listItems";
        private const string REPEAT_COLUMNS = "repeatColumns";


        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( VALUES_KEY );
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

            var li = new ListItems();
            li.Label = "Values";
            li.Help = "The list of the values to display.";
            li.ValueChanged += OnQualifierUpdated;
            controls.Add( li );

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
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( VALUES_KEY, new ConfigurationValue( "Values",
                "The source of the values to display.", string.Empty ) );

            var description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, string.Empty ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is ListItems )
                {
                    configurationValues[VALUES_KEY].Value = ( ( ListItems ) controls[0] ).Value;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is NumberBox )
                {
                    configurationValues[REPEAT_COLUMNS].Value = ( ( NumberBox ) controls[1] ).Text;
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is ListItems && configurationValues.ContainsKey( VALUES_KEY ) )
                {
                    ( ( ListItems ) controls[0] ).Value = configurationValues[VALUES_KEY].Value;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is NumberBox && configurationValues.ContainsKey( REPEAT_COLUMNS ) )
                {
                    ( ( NumberBox ) controls[1] ).Text = configurationValues[REPEAT_COLUMNS].Value;
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
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( !string.IsNullOrWhiteSpace( value ) && configurationValues.ContainsKey( VALUES_KEY ) )
            {
                if ( string.IsNullOrEmpty( configurationValues[VALUES_KEY].Value ) || string.IsNullOrEmpty( value ) )
                {
                    return string.Empty;
                }
                else
                {
                    var keyValuePairs = JsonConvert.DeserializeObject<List<KeyValuePair>>( configurationValues[VALUES_KEY].Value );
                    if ( condensed )
                    {
                        return GetUrlDecodedValues( keyValuePairs, value );


                    }
                    else
                    {
                        var values = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();
                        string formattedValue = string.Empty;
                        foreach ( var keyValuePair in keyValuePairs )
                        {
                            formattedValue += string.Format( @"<div>
                                <i class='far fa{1}-square'></i> {0}
                               </div>", keyValuePair.Value, values.Any( a => a == keyValuePair.Key ) ? "-check" : "" );
                        }
                        return formattedValue;
                    }

                }
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
                ListControl editControl = new RockCheckBoxList { ID = id };
                ( ( RockCheckBoxList ) editControl ).DisplayAsCheckList = true;
                ( ( RockCheckBoxList ) editControl ).RepeatDirection = RepeatDirection.Vertical;


                if ( configurationValues.ContainsKey( REPEAT_COLUMNS ) )
                {
                    ( ( RockCheckBoxList ) editControl ).RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
                }

                if ( configurationValues.ContainsKey( VALUES_KEY ) )
                {
                    var values = JsonConvert.DeserializeObject<List<KeyValuePair>>( configurationValues[VALUES_KEY].Value );
                    if ( values != null )
                    {
                        foreach ( var val in values )
                        {
                            editControl.Items.Add( new ListItem( val.Value, val.Key.ToString() ) );
                        }
                    }
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
                ListControl cbl = ( ListControl ) control;
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
                    ListControl cbl = ( ListControl ) control;
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
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public override bool HasFilterControl()
        {
            return true;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( value != null && configurationValues.ContainsKey( VALUES_KEY ) )
            {
                var keyValuePairs = JsonConvert.DeserializeObject<List<KeyValuePair>>( configurationValues[VALUES_KEY].Value );
                var values = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();
                return AddQuotes( keyValuePairs.Where( a => values.Contains( a.Key ) ).Select( a => a.Value ).ToList().AsDelimited( "' OR '" ) );
            }
            return string.Empty;
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
                ComparisonType? comparisonType = filterValues[0].ConvertToEnumOrNull<ComparisonType>();
                if ( comparisonType.HasValue )
                {

                    string compareToValue = filterValues[1];
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, this.AttributeValueFieldName );

                    if ( !string.IsNullOrWhiteSpace( compareToValue ) )
                    {
                        List<string> selectedValues = compareToValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                        foreach ( var selectedValue in selectedValues )
                        {
                            var searchValue = "," + selectedValue + ",";
                            var qryToExtract = new AttributeValueService( new Data.RockContext() ).Queryable().Where( a => ( "," + a.Value + "," ).Contains( searchValue ) );
                            var valueExpression = FilterExpressionExtractor.Extract<AttributeValue>( qryToExtract, parameterExpression, "a" );

                            if ( comparisonType.Value != ComparisonType.Contains )
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
                    else
                    {
                        // No comparison value was specified, so we can filter if the Comparison Type using no value still makes sense
                        if ( ( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
                        {
                            // Just checking if IsBlank or IsNotBlank, so let ComparisonExpression do its thing
                            return ComparisonHelper.ComparisonExpression( comparisonType.Value, propertyExpression, AttributeConstantExpression( string.Empty ) );
                        }
                    }
                }
            }

            if ( comparison == null )
            {
                return new Rock.Data.NoAttributeFilterExpression();
            }

            return comparison;
        }

        #endregion

        #region Private

        /// <summary>
        /// Gets the URL decoded values.
        /// </summary>
        /// <param name="keyValuePairs">The key value pairs.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string GetUrlDecodedValues( List<KeyValuePair> keyValuePairs, string value )
        {
            var values = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();
            return keyValuePairs.Where( a => values.Contains( a.Key ) ).Select( a => a.Value ).ToList().AsDelimited( "," );
        }

        #endregion
    }
}