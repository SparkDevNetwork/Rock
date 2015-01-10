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
using System.Linq.Expressions;
using System.Web.UI;
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
        /// <summary>
        /// Initializes a new instance of the <see cref="Field"/> class.
        /// </summary>
        public FieldType()
        {
        }

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
                return System.Web.HttpUtility.HtmlEncode( value ).Truncate( 100 );
            }

            return value;
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual string FormatValueAsHtml( string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            return FormatValue( null, value, configurationValues, false );
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
        /// Creates the controls needed to filter (query) values using this field type.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual List<Control> FilterControls ( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var controls = new List<Control>();

            RockDropDownList ddlText = ComparisonHelper.ComparisonControl( ComparisonHelper.StringFilterComparisonTypes, false );
            ddlText.ID = string.Format( "{0}_ddlText", id );
            ddlText.AddCssClass( "js-filter-compare" );
            controls.Add( ddlText );

            var tbText = new RockTextBox();
            tbText.ID = string.Format( "{0}_tbText", id );
            tbText.AddCssClass( "js-filter-control" );
            controls.Add( tbText );

            return controls;
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControls">The filter controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public virtual List<string> GetFilterValues( List<Control> filterControls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var values = new List<string>();

            if ( filterControls != null )
            {
                if ( filterControls.Count > 0 && filterControls[0] is RockDropDownList )
                {
                    values.Add( ( (RockDropDownList)filterControls[0] ).SelectedValue );
                }
                if ( filterControls.Count > 1 && filterControls[1] is RockTextBox)
                {
                    values.Add( ( (RockTextBox)filterControls[1] ).Text );
                }
            }

            return values;
        }

        /// <summary>
        /// Sets the filter value.
        /// </summary>
        /// <param name="filterControls">The filter controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValue">The filter value.</param>
        public virtual void SetFilterValues ( List<Control> filterControls, Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues )
        {
            if ( filterControls != null )
            {
                if ( filterControls.Count > 0 && filterControls[0] is RockDropDownList && filterValues.Count > 0 )
                {
                    ( (RockDropDownList)filterControls[0] ).SetValue( filterValues[0] );
                }
                if ( filterControls.Count > 1 && filterControls[1] is RockTextBox )
                {
                    ( (RockTextBox)filterControls[1] ).Text = filterValues[1];
                }
            }
        }

        /// <summary>
        /// Gets the filters expression.
        /// </summary>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <returns></returns>
        public virtual Expression FilterExpression( IService serviceInstance, ParameterExpression parameterExpression, string propertyName, List<string> filterValues )
        {
            if ( filterValues.Count >= 2 )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );

                string comparisonValue = filterValues[0];
                if ( comparisonValue != "0" )
                {
                    ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                    ConstantExpression constantExpression;
                    if ( propertyExpression.Type == typeof( Guid ) )
                    {
                        constantExpression = Expression.Constant( filterValues[1].AsGuid() );
                    }
                    else
                    {
                        constantExpression = Expression.Constant( filterValues[1] );
                    }

                    return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                }
            }

            return null;
        }

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

        /// <summary>
        /// Gets information about how to configure a filter UI for this type of field. Used primarily for dataviews
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns></returns>
        public virtual Rock.Reporting.EntityField GetFilterConfig( Rock.Web.Cache.AttributeCache attribute )
        {
            var entityField = new Rock.Reporting.EntityField();
            entityField.Name = attribute.Name;
            entityField.Title = attribute.Name.SplitCase();
            entityField.AttributeGuid = attribute.Guid;
            entityField.FieldKind = Reporting.FieldKind.Attribute;
            entityField.PropertyType = null;

            entityField.ControlCount = 2;
            entityField.FilterFieldType = SystemGuid.FieldType.TEXT;
            
            return entityField;
        }
    }
}