﻿// <copyright>
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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    [Serializable]
    public class SelectSingleFieldType : FieldType
    {

        #region Configuration

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( "values" );
            configKeys.Add( "fieldtype" );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var tb = new RockTextBox();
            controls.Add( tb );
            tb.TextMode = TextBoxMode.MultiLine;
            tb.Rows = 3;
            tb.AutoPostBack = true;
            tb.TextChanged += OnQualifierUpdated;
            tb.Label = "Values";
            tb.Help = "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column.";

            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.Items.Add( new ListItem( "Drop Down List", "ddl" ) );
            ddl.Items.Add( new ListItem( "Radio Buttons", "rb" ) );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Control Type";
            ddl.Help = "The type of control to use for selecting a single value from the list.";
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
            configurationValues.Add( "values", new ConfigurationValue( "Values",
                "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column <span class='tip tip-lava'></span>.", "" ) );
            configurationValues.Add( "fieldtype", new ConfigurationValue( "Control Type", 
                "The type of control to use for selecting a single value from the list.", "ddl" ) );

            if ( controls != null && controls.Count == 2 )
            {
                if ( controls[0] != null && controls[0] is TextBox )
                    configurationValues["values"].Value = ( ( TextBox )controls[0] ).Text;

                if ( controls[1] != null && controls[1] is DropDownList )
                    if ( ( ( DropDownList )controls[1] ).SelectedValue == "rb" )
                        configurationValues["fieldtype"].Value = "rb";
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
            if ( controls != null && controls.Count == 2 && configurationValues != null)
            {
                if ( controls[0] != null && controls[0] is TextBox && configurationValues.ContainsKey("values"))
                    ( ( TextBox )controls[0] ).Text = configurationValues["values"].Value;

                if ( controls[1] != null && controls[1] is DropDownList && configurationValues.ContainsKey("fieldtype") )
                    ( ( DropDownList )controls[1] ).SelectedValue = configurationValues["fieldtype"].Value;
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
            if ( !string.IsNullOrWhiteSpace( value ) && configurationValues.ContainsKey( "values" ) )
            {
                var configuredValues = Helper.GetConfiguredValues( configurationValues );
                var selectedValues = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                return configuredValues
                    .Where( v => selectedValues.Contains( v.Key ) )
                    .Select( v => v.Value )
                    .ToList()
                    .AsDelimited( "," );
            }

            return base.FormatValue( parentControl, value, configurationValues, condensed );
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
            return value;
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

                if ( configurationValues.ContainsKey( "fieldtype" ) && configurationValues["fieldtype"].Value == "rb" )
                {
                    editControl = new RockRadioButtonList { ID = id }; 
                    ( (RadioButtonList)editControl ).RepeatDirection = RepeatDirection.Horizontal;
                }
                else
                {
                    editControl = new RockDropDownList { ID = id };
                    editControl.Items.Add( new ListItem() );
                }

                foreach( var keyVal in Helper.GetConfiguredValues( configurationValues ) )
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
            if ( control != null && control is ListControl )
            {
                return ( (ListControl)control ).SelectedValue;
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
                if ( control != null && control is ListControl )
                {
                    ( (ListControl)control ).SetValue( value );
                }
            }
        }

        #endregion

        #region Filter Control
         
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
            var lbl = new Label();
            lbl.ID = string.Format( "{0}_lIs", id );
            lbl.AddCssClass( "data-view-filter-label" );
            lbl.Text = "Is";


            // hide the compare control when in SimpleFilter mode
            lbl.Visible = filterMode != FilterMode.SimpleFilter;
            
            return lbl;
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
            if ( configurationValues != null && configurationValues.ContainsKey( "values" ) )
            {
                var cbList = new RockCheckBoxList();
                cbList.ID = string.Format( "{0}_cbList", id );
                cbList.AddCssClass( "js-filter-control" );
                cbList.RepeatDirection = RepeatDirection.Horizontal;

                foreach ( var keyVal in Helper.GetConfiguredValues( configurationValues ) )
                {
                    cbList.Items.Add( new ListItem( keyVal.Value, keyVal.Key ) );
                }

                if ( cbList.Items.Count > 0 )
                {
                    return cbList;
                }
            }

            return null;
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
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override string GetFilterCompareValue( Control control, FilterMode filterMode )
        {
            return null;
        }

        /// <summary>
        /// Gets the equal to compare value (types that don't support an equalto comparison (i.e. singleselect) should return null
        /// </summary>
        /// <returns></returns>
        public override string GetEqualToCompareValue()
        {
            return null;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var values = new List<string>();

            if ( control != null && control is CheckBoxList )
            {
                CheckBoxList cbl = (CheckBoxList)control;
                foreach ( ListItem li in cbl.Items )
                {
                    if ( li.Selected )
                    {
                        values.Add( li.Value );
                    }
                }
            }

            return values.AsDelimited( "," );
        }

        /// <summary>
        /// Sets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterCompareValue( Control control, string value )
        {
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is CheckBoxList && value != null )
            {
                var values = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                CheckBoxList cbl = (CheckBoxList)control;
                foreach ( ListItem li in cbl.Items )
                {
                    li.Selected = values.Contains( li.Value );
                }
            }
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var configuredValues = Helper.GetConfiguredValues( configurationValues );
            var selectedValues = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            return configuredValues
                .Where( v => selectedValues.Contains( v.Key ) )
                .Select( v => "'" + v.Value + "'" )
                .ToList()
                .AsDelimited( " or " );
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
            var format = "return Rock.reporting.formatFilterForSelectSingleField('{0}', $selectedContent);";
            return string.Format( format, titleJs );
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
        /// <remarks>
        /// Used only by enums ( See the EntityHelper.GetEntityFields() method )
        /// </remarks>
        public override Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            List<string> selectedValues = filterValues[0].Split( new char[] {','}, StringSplitOptions.RemoveEmptyEntries ).ToList();
            if ( selectedValues.Any() )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );

                object constantValue;
                if ( propertyType.IsEnum )
                {
                   constantValue = Enum.Parse( propertyType, selectedValues[0] );
                }
                else
                {
                    constantValue = selectedValues[0] as string;
                }

                 ConstantExpression constantExpression = Expression.Constant( constantValue );
                Expression comparison = Expression.Equal( propertyExpression, constantExpression );

                foreach ( string selectedValue in selectedValues.Skip( 1 ) )
                {
                    constantExpression = Expression.Constant( Enum.Parse( propertyType, selectedValue ) );
                    comparison = Expression.Or( comparison, Expression.Equal( propertyExpression, constantExpression ) );
                }

                return comparison;
            }

            return null;
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
            if ( filterValues.Count == 1 )
            {
                List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                if ( selectedValues.Any() )
                {
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
                    ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof(List<string>) );
                    return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof(string) } ), propertyExpression );
                }
            }

            return null;
        }

        #endregion

    }
}