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
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type
    /// Stored as either a single DefinedValue.Guid or a comma-delimited list of DefinedValue.Guids (if AllowMultiple)
    /// </summary>
    [Serializable]
    public class DefinedValueFieldType : FieldType, IEntityFieldType
    {
        #region Configuration

        private const string DEFINED_TYPE_KEY = "definedtype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string DISPLAY_DESCRIPTION = "displaydescription";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( DEFINED_TYPE_KEY );
            configKeys.Add( ALLOW_MULTIPLE_KEY );
            configKeys.Add( DISPLAY_DESCRIPTION );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // build a drop down list of defined types (the one that gets selected is
            // used to build a list of defined values) 
            var ddl = new RockDropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;
            ddl.Label = "Defined Type";
            ddl.Help = "The Defined Type to select values from.";

            Rock.Model.DefinedTypeService definedTypeService = new Model.DefinedTypeService( new RockContext() );
            foreach ( var definedType in definedTypeService.Queryable().OrderBy( d => d.Name ) )
            {
                ddl.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );
            }

            // Add checkbox for deciding if the defined values list is rendered as a drop
            // down list or a checkbox list.
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Allow Multiple Values";
            cb.Text = "Yes";
            cb.Help = "When set, allows multiple defined type values to be selected.";

            // option for Display Descriptions
            var cbDescription = new RockCheckBox();
            controls.Add( cbDescription );
            cbDescription.AutoPostBack = true;
            cbDescription.CheckedChanged += OnQualifierUpdated;
            cbDescription.Label = "Display Descriptions";
            cbDescription.Text = "Yes";
            cbDescription.Help = "When set, the defined value descriptions will be displayed instead of the values.";
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
            configurationValues.Add( DEFINED_TYPE_KEY, new ConfigurationValue( "Defined Type", "The Defined Type to select values from", string.Empty ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple defined type values to be selected.", string.Empty ) );
            configurationValues.Add( DISPLAY_DESCRIPTION, new ConfigurationValue( "Display Descriptions", "When set, the defined value descriptions will be displayed instead of the values.", string.Empty ) );

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] != null && controls[0] is DropDownList )
                {
                    configurationValues[DEFINED_TYPE_KEY].Value = ( (DropDownList)controls[0] ).SelectedValue;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is CheckBox )
                {
                    configurationValues[ALLOW_MULTIPLE_KEY].Value = ( (CheckBox)controls[1] ).Checked.ToString();
                }

                if ( controls.Count > 2 && controls[2] != null && controls[2] is CheckBox )
                {
                    configurationValues[DISPLAY_DESCRIPTION].Value = ( (CheckBox)controls[2] ).Checked.ToString();
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
                if ( controls.Count > 0 && controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( DEFINED_TYPE_KEY ) )
                {
                    ( (DropDownList)controls[0] ).SelectedValue = configurationValues[DEFINED_TYPE_KEY].Value;
                }

                if ( controls.Count > 1 && controls[1] != null && controls[1] is CheckBox && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) )
                {
                    ( (CheckBox)controls[1] ).Checked = configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
                }

                if ( controls.Count > 2 && controls[2] != null && controls[2] is CheckBox && configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) )
                {
                    ( (CheckBox)controls[2] ).Checked = configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean();
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
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                bool useDescription = false;
                if ( !condensed &&
                     configurationValues != null &&
                     configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) &&
                     configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean() )
                {
                    useDescription = true;
                }

                var names = new List<string>();
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var definedValue = Rock.Web.Cache.DefinedValueCache.Read( guid );
                    if ( definedValue != null )
                    {
                        names.Add( useDescription ? definedValue.Description : definedValue.Value );
                    }
                }

                formattedValue = names.AsDelimited( ", " );
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
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
            ListControl editControl;

            if ( configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean() )
            {
                editControl = new Rock.Web.UI.Controls.RockCheckBoxList { ID = id, RepeatDirection = RepeatDirection.Horizontal };
                editControl.AddCssClass( "checkboxlist-group" );
            }
            else
            {
                editControl = new Rock.Web.UI.Controls.RockDropDownList { ID = id };
                editControl.Items.Add( new ListItem() );
            }

            if ( configurationValues != null && configurationValues.ContainsKey( DEFINED_TYPE_KEY ) )
            {
                int? definedTypeId = configurationValues[DEFINED_TYPE_KEY].Value.AsIntegerOrNull();
                if ( definedTypeId.HasValue )
                {
                    Rock.Model.DefinedValueService definedValueService = new Model.DefinedValueService( new RockContext() );
                    var definedValues = definedValueService.GetByDefinedTypeId( definedTypeId.Value );
                    if ( definedValues.Any() )
                    {
                        bool useDescription = configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) && configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean();

                        foreach ( var definedValue in definedValues )
                        {
                            editControl.Items.Add( new ListItem( useDescription ? definedValue.Description : definedValue.Value, definedValue.Id.ToString() ) );
                        }
                    }

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
            var definedValueIdList = new List<int>();

            if ( control != null && control is ListControl )
            {
                if ( control is Rock.Web.UI.Controls.RockDropDownList )
                {
                    definedValueIdList.Add( ( (ListControl)control ).SelectedValue.AsInteger() );
                }
                else if ( control is Rock.Web.UI.Controls.RockCheckBoxList )
                {
                    var cblControl = control as Rock.Web.UI.Controls.RockCheckBoxList;

                    definedValueIdList.AddRange( cblControl.Items.Cast<ListItem>()
                        .Where( i => i.Selected )
                        .Select( i => i.Value ).AsIntegerList() );
                }
            }

            var guids = new List<Guid>();

            foreach ( int definedValueId in definedValueIdList )
            {
                var definedValue = Rock.Web.Cache.DefinedValueCache.Read( definedValueId );
                if ( definedValue != null )
                {
                    guids.Add( definedValue.Guid );
                }
            }

            return guids.AsDelimited( "," );
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
                    var ids = new List<string>();
                    foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                    {
                        var definedValue = Rock.Web.Cache.DefinedValueCache.Read( guid );
                        if ( definedValue != null )
                        {
                            ids.Add( definedValue.Id.ToString() );
                        }
                    }

                    var listControl = control as ListControl;
                    foreach ( ListItem li in listControl.Items )
                    {
                        li.Selected = ids.Contains( li.Value );
                    }
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
        /// <returns></returns>
        public override Control FilterCompareControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            if ( allowMultiple )
            {
                return base.FilterCompareControl( configurationValues, id, required, filterMode );
            }
            else
            {
                var lbl = new Label();
                lbl.ID = string.Format( "{0}_lIs", id );
                lbl.AddCssClass( "data-view-filter-label" );
                lbl.Text = "Is";
                
                // hide the compare control when in SimpleFilter mode
                lbl.Visible = filterMode != FilterMode.SimpleFilter;
                return lbl;
            }
        }

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
        /// Filters the value control.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();

            var overrideConfigValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var keyVal in configurationValues )
            {
                overrideConfigValues.Add( keyVal.Key, keyVal.Value );
            }

            overrideConfigValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new ConfigurationValue( ( !allowMultiple ).ToString() ) );

            return base.FilterValueControl( overrideConfigValues, id, required, filterMode );
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
        /// Gets the filter value.
        /// </summary>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override List<string> GetFilterValues( Control filterControl, Dictionary<string, ConfigurationValue> configurationValues, FilterMode filterMode )
        {
            var values = new List<string>();

            if ( filterControl != null )
            {
                bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();

                try
                {
                    if ( allowMultiple )
                    {
                        var filterValues = base.GetFilterValues( filterControl, configurationValues, filterMode );
                        if ( filterValues != null )
                        {
                            filterValues.ForEach( v => values.Add( v ) );
                        }
                    }
                    else
                    {
                        values.Add( GetEditValue( filterControl.Controls[1].Controls[0], configurationValues ) );
                    }
                }
                catch
                {
                    // intentionally ignore
                }
            }

            return values;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            bool useDescription = false;
            if ( configurationValues != null &&
                configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) &&
                configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean() )
            {
                useDescription = true;
            }

            var values = new List<string>();
            foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
            {
                var definedValue = Rock.Web.Cache.DefinedValueCache.Read( guid );
                if ( definedValue != null )
                {
                    values.Add( useDescription ? definedValue.Description : definedValue.Value );
                }
            }

            return values.Select( v => "'" + v + "'" ).ToList().AsDelimited( " or " );
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
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            if ( allowMultiple )
            {
                return base.GetFilterFormatScript( configurationValues, title );
            }

            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            var format = "return Rock.reporting.formatFilterForDefinedValueField('{0}', $selectedContent);";
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
        public override Expression PropertyFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, Expression parameterExpression, string propertyName, Type propertyType )
        {
            List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            if ( selectedValues.Any() )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, propertyName );

                var type = propertyType;
                bool isNullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof( Nullable<> );
                if ( isNullableType )
                {
                    type = Nullable.GetUnderlyingType( type );
                    propertyExpression = Expression.Property( propertyExpression, "Value" );
                }

                Type genericListType = typeof( List<> );
                Type specificListType = genericListType.MakeGenericType( type );
                object specificList = Activator.CreateInstance( specificListType );

                foreach ( string value in selectedValues )
                {
                    string tempValue = value;

                    // if this is not for an attribute value, look up the id for the defined value
                    if ( propertyName != "Value" || propertyType != typeof( string ) )
                    {
                        var dv = DefinedValueCache.Read( value.AsGuid() );
                        tempValue = dv != null ? dv.Id.ToString() : string.Empty;
                    }

                    if ( !string.IsNullOrWhiteSpace( tempValue ) )
                    {
                        object obj = Convert.ChangeType( tempValue, type );
                        specificListType.GetMethod( "Add" ).Invoke( specificList, new object[] { obj } );
                    }
                }

                ConstantExpression constantExpression = Expression.Constant( specificList, specificListType );
                return Expression.Call( constantExpression, specificListType.GetMethod( "Contains", new Type[] { type } ), propertyExpression );
            }

            return null;
        }

        /// <summary>
        /// Geta a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            if ( allowMultiple || filterValues.Count != 1 )
            {
                return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
            }

            List<string> selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            if ( selectedValues.Any() )
            {
                MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
                ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
            }

            return null;
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = DefinedValueCache.Read( guid );
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            DefinedValueCache item = null;
            if ( id.HasValue )
            {
                item = DefinedValueCache.Read( id.Value );
            }

            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new DefinedValueService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion
    }
}