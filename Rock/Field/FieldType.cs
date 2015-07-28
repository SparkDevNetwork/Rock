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
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field
{
    /// <summary>
    /// Abstract class that all custom field types should inherit from
    /// </summary>
    [Serializable]
    public abstract class FieldType : IFieldType
    {

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        public FieldType()
        {
        }

        #endregion

        #region Configuration 

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public virtual List<string> ConfigurationKeys()
        {
            return new List<string>();
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public virtual List<Control> ConfigurationControls()
        {
            return new List<Control>();
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public virtual Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            return new Dictionary<string, ConfigurationValue>();
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public virtual void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Gets the align value that should be used when displaying value
        /// </summary>
        public virtual HorizontalAlign AlignValue
        {
            get { return HorizontalAlign.Left; }
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public virtual string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            if ( condensed )
            {
                return value.Truncate( 100 );
            }

            return value;
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public virtual string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            return FormatValue( parentControl, value, configurationValues, condensed );
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
        public virtual Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new Rock.Web.UI.Controls.RockTextBox { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( control != null && control is TextBox )
            {
                return ( (TextBox)control ).Text;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public virtual void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            if ( control != null && control is TextBox )
            {
                ( (TextBox)control ).Text = value;
            }
        }

        /// <summary>
        /// Tests the value to ensure that it is a valid value.  If not, message will indicate why
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="message">The message.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is valid; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsValid( string value, bool required, out string message )
        {
            if ( required && string.IsNullOrWhiteSpace( value ) )
            {
                message = "value is required.";
                return false;
            }

            message = string.Empty;
            return true;
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual Control FilterControl ( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            HtmlGenericControl row = new HtmlGenericControl( "div" );
            row.ID = id;
            row.AddCssClass( "row" );
            row.AddCssClass( "field-criteria" );

            var compareControl = FilterCompareControl( configurationValues, id, required, filterMode );
            var valueControl = FilterValueControl( configurationValues, id, required, filterMode );

            bool isLabel = compareControl is Label;

            if ( compareControl != null )
            {
                HtmlGenericControl col1 = new HtmlGenericControl( "div" );
                col1.ID = string.Format( "{0}_col1", id );
                row.Controls.Add( col1 );
                col1.AddCssClass( isLabel ? "col-md-2" : "col-md-4" );
                col1.Controls.Add( compareControl );
            }

            HtmlGenericControl col2 = new HtmlGenericControl( "div" );
            col2.ID = string.Format( "{0}_col2", id );
            row.Controls.Add( col2 );
            col2.AddCssClass( isLabel ? "col-md-10" : "col-md-8" );
            col2.Controls.Add( valueControl );

            return row;
        }

        /// <summary>
        /// Creates the control needed to filter (query) values using this field type using a FilterMode of AdvancedFilter
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public virtual Control FilterControl ( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required)
        {
            return FilterControl( configurationValues, id, required, FilterMode.AdvancedFilter );
        }

        /// <summary>
        /// Determines whether this filter has a filter control
        /// </summary>
        /// <returns></returns>
        public virtual bool HasFilterControl()
        {
            try
            {
                var filterControl = FilterControl( new Dictionary<string, ConfigurationValue>(), "", true, FilterMode.AdvancedFilter );
                return filterControl != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the filter compare control with the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        { 
            RockDropDownList ddlCompare = ComparisonHelper.ComparisonControl( FilterComparisonType, required );
            ddlCompare.ID = string.Format( "{0}_ddlCompare", id );
            ddlCompare.AddCssClass( "js-filter-compare" );
            return ddlCompare;
        }

        /// <summary>
        /// Gets the filter compare control with a FilterMode of AdvancedFilter
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        [Obsolete]
        public virtual Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required )
        {
            return FilterCompareControl( configurationValues, id, required, FilterMode.AdvancedFilter );
        }

        /// <summary>
        /// Gets the type of the filter comparison.
        /// </summary>
        /// <value>
        /// The type of the filter comparison.
        /// </value>
        public virtual ComparisonType FilterComparisonType
        {
            get { return ComparisonHelper.BinaryFilterComparisonTypes; }
        }

        /// <summary>
        /// Gets the filter value control with the specified FilterMode
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public virtual Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            var control = EditControl( configurationValues, id );
            control.ID = string.Format( "{0}_ctlCompareValue", id );
            if ( control is WebControl )
            {
                ( (WebControl)control ).AddCssClass( "js-filter-control" );
            }
            return control;
        }

        /// <summary>
        /// Gets the filter value control with the a FilterMode of AdvancedFilter
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        [Obsolete]
        public virtual Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required)
        {
            return FilterValueControl( configurationValues, id, required, FilterMode.AdvancedFilter );
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var values = new List<string>();

            if ( filterControl != null )
            {
                try
                {
                    string compare = GetFilterCompareValue( filterControl.Controls[0].Controls[0] );
                    if (compare != null )
                    {
                        values.Add( compare );
                    }

                    string value = GetFilterValueValue( filterControl.Controls[1].Controls[0], configurationValues );
                    if ( value != null )
                    {
                        values.Add( value );
                    }
                }
                catch { }
            }

            return values;
        }

        /// <summary>
        /// Gets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns></returns>
        public virtual string GetFilterCompareValue( Control control )
        {
            var ddlCompare = control as RockDropDownList;
            if ( ddlCompare != null )
            {
                return ddlCompare.SelectedValue;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return GetEditValue( control, configurationValues );
        }

        /// <summary>
        /// Sets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        public virtual void SetFilterValues ( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterControl != null )
            {
                try
                {
                    SetFilterCompareValue( filterControl.Controls[0].Controls[0], filterValues.Count > 0 ? filterValues[0] : string.Empty );
                    string value = filterValues.Count > 1 ? filterValues[1] : filterValues.Count > 0 ? filterValues[0] : string.Empty;
                    SetFilterValueValue( filterControl.Controls[1].Controls[0], configurationValues, value );
                }
                catch { }
            }
        }

        /// <summary>
        /// Sets the filter compare value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="value">The value.</param>
        public virtual void SetFilterCompareValue( Control control, string value )
        {
            var ddlCompare = control as RockDropDownList;
            if ( ddlCompare != null )
            {
                ddlCompare.SetValue( value );
            }
        }

        /// <summary>
        /// Sets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public virtual void SetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            SetEditValue( control, configurationValues, value );
        }

        /// <summary>
        /// Formats the filter values.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        public virtual string FormatFilterValues( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterValues != null && filterValues.Any() )
            {
                // If just one value, then it is likely just a value
                if ( filterValues.Count == 1 )
                {
                    string filterValue = FormatFilterValueValue( configurationValues, filterValues[0] );
                    if ( !string.IsNullOrWhiteSpace( filterValue ) )
                    {
                        return "is " + filterValue;
                    }
                }

                // If two more values, then it is a comparison and a value
                else if ( filterValues.Count >= 2 )
                {
                    string comparisonValue = filterValues[0];
                    if ( comparisonValue != "0" )
                    {
                        ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.StartsWith );
                        if ( comparisonType == ComparisonType.IsBlank || comparisonType == ComparisonType.IsNotBlank )
                        {
                            return comparisonType.ConvertToString();
                        }
                        else
                        {
                            return string.Format( "{0} {1}", comparisonType.ConvertToString(), FormatFilterValueValue( configurationValues, filterValues[1] ) );
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            string formattedValue = FormatValue( null, value, configurationValues, false );
            if ( !string.IsNullOrWhiteSpace( formattedValue ) )
            {
                return string.Format( "'{0}'", formattedValue );
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the filter format script.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        /// <remarks>
        /// This script must set a javascript variable named 'result' to a friendly string indicating value of filter controls
        /// a '$selectedContent' should be used to limit script to currently selected filter fields
        /// </remarks>
        public virtual string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            return string.Format( "return Rock.reporting.formatFilterDefault('{0}', $selectedContent);", titleJs);
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
        public virtual Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            if ( filterValues.Count >= 2 )
            {
                string comparisonValue = filterValues[0];
                if ( comparisonValue != "0" )
                {
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );

                    var type = propertyType;
                    bool isNullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> );
                    if ( isNullableType )
                    {
                        type = Nullable.GetUnderlyingType( type );
                    }

                    object value = ConvertValueToPropertyType( filterValues[1], type, isNullableType );
                    if ( value != null )
                    {
                        ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                        ConstantExpression constantExpression = Expression.Constant( value, type );
                        return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Converts the type of the value to property
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <returns></returns>
        public virtual object ConvertValueToPropertyType( string value, Type propertyType )
        {
            return ConvertValueToPropertyType( value, propertyType, false );
        }

        /// <summary>
        /// Converts the type of the value to property.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="isNullableType">if set to <c>true</c> [is nullable type].</param>
        /// <returns></returns>
        public virtual object ConvertValueToPropertyType( string value, Type propertyType, bool isNullableType )
        {
            if ( propertyType == typeof(string) )
            {
                return value;
            }

            if ( string.IsNullOrWhiteSpace(value) && isNullableType)
            {
                return null;
            }

            return Convert.ChangeType( value, propertyType );
        }

        /// <summary>
        /// Geta a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public virtual Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            if ( filterValues.Count >= 2 )
            {
                string comparisonValue = filterValues[0];
                if ( comparisonValue != "0" )
                {
                    ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                    MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
                    ConstantExpression constantExpression = Expression.Constant( filterValues[1], typeof( string ) );

                    return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                }
            }

            return null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when [qualifier updated].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        public void OnQualifierUpdated(object sender, EventArgs e)
        {
            if ( QualifierUpdated != null )
            {
                QualifierUpdated( sender, e );
            }
        }

        /// <summary>
        /// Occurs when [qualifier updated].
        /// </summary>
        public event EventHandler QualifierUpdated;

        #endregion

    }
}