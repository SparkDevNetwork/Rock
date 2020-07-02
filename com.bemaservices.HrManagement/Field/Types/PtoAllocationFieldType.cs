// <copyright>
// Copyright by BEMA Information Services
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
using Rock;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace com.bemaservices.HrManagement.Field.Types
{
    /// <summary>
    /// Field Type used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    [Serializable]
    public class PtoAllocationFieldType : Rock.Field.FieldType
    {

        #region Configuration

        private const string FIELDTYPE_KEY = "fieldtype";
        private const string REPEAT_COLUMNS = "repeatColumns";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( FIELDTYPE_KEY );
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

            var ddlFieldType = new RockDropDownList();
            ddlFieldType.Items.Add( new ListItem( "Drop Down List", "ddl" ) );
            ddlFieldType.Items.Add( new ListItem( "Drop Down List (Enhanced for Long Lists)", "ddl_enhanced" ) );
            ddlFieldType.Items.Add( new ListItem( "Radio Buttons", "rb" ) );
            ddlFieldType.AutoPostBack = true;
            ddlFieldType.SelectedIndexChanged += OnQualifierUpdated;
            ddlFieldType.Label = "Control Type";
            ddlFieldType.Help = "The type of control to use for selecting a single value from the list.";
            controls.Add( ddlFieldType );

            var tbRepeatColumns = new NumberBox();
            tbRepeatColumns.Label = "Columns";
            tbRepeatColumns.Help = "Select how many columns the list should use before going to the next row. If blank or 0 then 4 columns will be displayed. There is no enforced upper limit however the block this control is used in might add contraints due to available space.";
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

            string description = "The type of control to use for selecting a single value from the list.";
            configurationValues.Add( FIELDTYPE_KEY, new ConfigurationValue( "Control Type", description, "ddl" ) );

            description = "Select how many columns the list should use before going to the next row. If blank 4 is used.";
            configurationValues.Add( REPEAT_COLUMNS, new ConfigurationValue( "Repeat Columns", description, string.Empty ) );

            if ( controls != null && controls.Count > 1 )
            {
                var ddlFieldType = controls[0] as RockDropDownList;
                var tbRepeatColumns = controls[1] as NumberBox;

                tbRepeatColumns.Visible = ddlFieldType.SelectedValue == "rb" ? true : false;

                configurationValues[FIELDTYPE_KEY].Value = ddlFieldType.SelectedValue;
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

            if ( controls != null && controls.Count > 1 && configurationValues != null )
            {
                var ddlFieldType = controls[0] as RockDropDownList;
                var tbRepeatColumns = controls[1] as NumberBox;

                ddlFieldType.SelectedValue = configurationValues.ContainsKey( FIELDTYPE_KEY ) ? configurationValues[FIELDTYPE_KEY].Value : ddlFieldType.SelectedValue;
                tbRepeatColumns.Text = configurationValues.ContainsKey( REPEAT_COLUMNS ) ? configurationValues[REPEAT_COLUMNS].Value : string.Empty;
                tbRepeatColumns.Visible = ddlFieldType.SelectedValue == "rb" ? true : false;
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
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var configuredValues = GetConfiguredAllocationValues();
                var selectedValues = value.ToUpper().Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                var a = configuredValues;
                var b = a.Where( v => selectedValues.Contains( v.Key.ToUpper() ) );
                var c = b.Select( v => v.Value );
                var d = c.ToList();
                var e = d.AsDelimited( ", " );

                return e;
                //return  configuredValues
                //    .Where( v => selectedValues.Contains( v.Key ) )
                //    .Select( v => v.Value )
                //    .ToList()
                //    .AsDelimited( ", " );
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

                string fieldType = configurationValues.ContainsKey( FIELDTYPE_KEY ) ? configurationValues[FIELDTYPE_KEY].Value : "ddl";
                if ( fieldType == "rb" )
                {
                    editControl = new RockRadioButtonList { ID = id };
                    ( ( RadioButtonList ) editControl ).RepeatDirection = RepeatDirection.Horizontal;

                    if ( configurationValues.ContainsKey( REPEAT_COLUMNS ) )
                    {
                        ( ( RadioButtonList ) editControl ).RepeatColumns = configurationValues[REPEAT_COLUMNS].Value.AsInteger();
                    }
                }
                else
                {
                    editControl = new RockDropDownList { ID = id };
                    ( ( RockDropDownList ) editControl ).EnhanceForLongLists = fieldType == "ddl_enhanced";
                    ( ( RockDropDownList ) editControl ).DisplayEnhancedAsAbsolute = true;
                    editControl.Items.Add( new ListItem() );
                }

                foreach ( var keyVal in GetConfiguredAllocationValues() )
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
                return ( ( ListControl ) control ).SelectedValue;
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
                    ( ( ListControl ) control ).SetValue( value );
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
            if ( configurationValues != null )
            {
                var cbList = new RockCheckBoxList();
                cbList.ID = string.Format( "{0}_cbList", id );
                cbList.AddCssClass( "js-filter-control" );
                cbList.RepeatDirection = RepeatDirection.Horizontal;

                foreach ( var keyVal in GetConfiguredAllocationValues() )
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
                CheckBoxList cbl = ( CheckBoxList ) control;
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

                CheckBoxList cbl = ( CheckBoxList ) control;
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
            var configuredValues = GetConfiguredAllocationValues();
            var selectedValues = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            return AddQuotes( configuredValues
                .Where( v => selectedValues.Contains( v.Key ) )
                .Select( v => v.Value )
                .ToList()
                .AsDelimited( "' OR '" ) );
        }

        public Dictionary<string, string> GetConfiguredAllocationValues()
        {
            var items = new Dictionary<string, string>();

            var listSource = @"{% include '~/Plugins/com_bemaservices/HrManagement/Assets/Lava/PtoAllocationFieldTypeLava.lava'  %}";

            var options = new Rock.Lava.CommonMergeFieldsOptions();
            options.GetLegacyGlobalMergeFields = false;
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, options );

            listSource = listSource.ResolveMergeFields( mergeFields, "RockEntity" );

            if ( listSource.ToUpper().Contains( "SELECT" ) && listSource.ToUpper().Contains( "FROM" ) )
            {
                var tableValues = new List<string>();
                DataTable dataTable = Rock.Data.DbService.GetDataTable( listSource, CommandType.Text, null );
                if ( dataTable != null && dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "Text" ) )
                {
                    foreach ( DataRow row in dataTable.Rows )
                    {
                        items.AddOrIgnore( row["value"].ToString(), row["text"].ToString() );
                    }
                }
            }

            else
            {
                foreach ( string keyvalue in listSource.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    var keyValueArray = keyvalue.Split( new char[] { '^' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( keyValueArray.Length > 0 )
                    {
                        items.AddOrIgnore( keyValueArray[0].Trim(), keyValueArray.Length > 1 ? keyValueArray[1].Trim() : keyValueArray[0].Trim() );
                    }
                }
            }


            return items;
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
            List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
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
            List<string> selectedValues = null;
            ComparisonType comparisonType = ComparisonType.Contains;

            if ( filterValues.Count == 1 )
            {
                // if there is only one filter value, it is a Contains comparison for the selectedValues
                // This is the normal thing that DataViews would do with a SelectSingleFieldType
                selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            }
            else if ( filterValues.Count >= 2 )
            {
                // if there are 2 (or more) filter values, the first is the comparison type and the 2nd is the selected value(s)
                // Note: Rock Lava Entity commands could do this, DataViews don't currently support more than just 'Contains'
                comparisonType = filterValues[0].ConvertToEnum<ComparisonType>( ComparisonType.Contains );

                if ( comparisonType == ComparisonType.EqualTo )
                {
                    // If EqualTo was specified, treat it as Contains
                    comparisonType = ComparisonType.Contains;
                }

                if ( comparisonType == ComparisonType.NotEqualTo )
                {
                    // If NotEqualTo was specified, treat it as DoesNotContain
                    comparisonType = ComparisonType.DoesNotContain;
                }

                selectedValues = filterValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            }

            // if IsBlank (or IsNotBlank)
            if ( ( ComparisonType.IsBlank | ComparisonType.IsNotBlank ).HasFlag( comparisonType ) )
            {
                // Just checking if IsBlank or IsNotBlank, so let ComparisonExpression do its thing
                MemberExpression propertyExpression = Expression.Property( parameterExpression, this.AttributeValueFieldName );
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, AttributeConstantExpression( string.Empty ) );
            }

            if ( selectedValues?.Any() == true )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
                ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                Expression expression = Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
                if ( comparisonType == ComparisonType.DoesNotContain )
                {
                    expression = Expression.Not( expression );
                }

                return expression;
            }


            return new NoAttributeFilterExpression();
        }

        #endregion

    }
}
