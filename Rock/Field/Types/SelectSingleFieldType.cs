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
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
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
                "The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column.", "" ) );
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
            if ( !string.IsNullOrWhiteSpace(value) && configurationValues.ContainsKey( "values" ) )
            {
                var listItems = configurationValues["values"].Value.GetListItems();
                if ( listItems != null )
                {
                    var valueList = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    return listItems.Where( a => valueList.Contains( a.Value ) ).ToList().AsDelimited( "," );
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

                if ( configurationValues.ContainsKey( "values" ) )
                {
                    string listSource = configurationValues["values"].Value;

                    if ( listSource.ToUpper().Contains( "SELECT" ) && listSource.ToUpper().Contains( "FROM" ) )
                    {
                        var tableValues = new List<string>();
                        DataTable dataTable = Rock.Data.DbService.GetDataTable( listSource, CommandType.Text, null );
                        if ( dataTable != null && dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "Text" ) )
                        {
                            foreach ( DataRow row in dataTable.Rows )
                            {
                                editControl.Items.Add( new ListItem( row["text"].ToString(), row["value"].ToString() ) );
                            }
                        }
                    }

                    else
                    {
                        foreach ( var listItem in listSource.GetListItems() )
                        {
                            editControl.Items.Add( listItem );
                        }
                    }

                    if ( editControl.Items.Count > 0 )
                    {
                        return editControl;
                    }
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
                return ( (ListControl)control ).SelectedValue;

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
                    ( (ListControl)control ).SelectedValue = value;
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Control FilterControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            if ( configurationValues != null && configurationValues.ContainsKey( "values" ) )
            {
                var cbList = new RockCheckBoxList();
                cbList.ID = string.Format( "{0}_cbList", id );
                cbList.AddCssClass( "js-filter-control" );
                cbList.RepeatDirection = RepeatDirection.Horizontal;

                string listSource = configurationValues["values"].Value;

                if ( listSource.ToUpper().Contains( "SELECT" ) && listSource.ToUpper().Contains( "FROM" ) )
                {
                    var tableValues = new List<string>();
                    DataTable dataTable = Rock.Data.DbService.GetDataTable( listSource, CommandType.Text, null );
                    if ( dataTable != null && dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "Text" ) )
                    {
                        foreach ( DataRow row in dataTable.Rows )
                        {
                            cbList.Items.Add( new ListItem( row["text"].ToString(), row["value"].ToString() ) );
                        }
                    }
                }

                else
                {
                    foreach ( var listItem in listSource.GetListItems() )
                    {
                        cbList.Items.Add( listItem );
                    }
                }

                if ( cbList.Items.Count > 0 )
                {
                    return cbList;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControl"></param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var values = new List<string>();

            List<string> cblValues = new List<string>();

            if ( filterControl != null && filterControl is CheckBoxList )
            {
                CheckBoxList cbl = (CheckBoxList)filterControl;
                foreach ( ListItem li in cbl.Items )
                {
                    if ( li.Selected )
                    {
                        cblValues.Add( li.Value );
                    }
                }

                values.Add( cblValues.AsDelimited<string>( "," ) );
            }

            return values;
        }

        /// <summary>
        /// Sets the filter value.
        /// </summary>
        /// <param name="filterControl"></param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues"></param>
        public override void SetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterControl != null && filterControl is CheckBoxList && filterValues.Any() )
            {
                string value = filterValues[0];
                if ( value != null )
                {
                    List<string> values = new List<string>();
                    values.AddRange( value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) );

                    CheckBoxList cbl = (CheckBoxList)filterControl;
                    foreach ( ListItem li in cbl.Items )
                    { 
                        li.Selected = values.Contains( li.Value );
                    }
                }
            }
        }

        public override Expression FilterExpression( IService serviceInstance, ParameterExpression parameterExpression, List<string> filterValues )
        {
            if ( filterValues.Count == 1 )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );

                List<string> selectedValues = filterValues[0].Split( new char[] {','}, StringSplitOptions.RemoveEmptyEntries ).ToList();
                if ( selectedValues.Any() )
                {
                    ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                    return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
                }
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Gets information about how to configure a filter UI for this type of field. Used primarily for dataviews
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public override Reporting.EntityField GetFilterConfig( Rock.Web.Cache.AttributeCache attribute )
        {
            var filterConfig = base.GetFilterConfig( attribute );
            filterConfig.ControlCount = 1;
            filterConfig.FilterFieldType = SystemGuid.FieldType.MULTI_SELECT;
            return filterConfig;
        }

    }
}