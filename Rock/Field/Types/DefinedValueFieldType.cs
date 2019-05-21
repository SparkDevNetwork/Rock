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
    /// Field Type used to display a dropdown list of Defined Values for a specific Defined Type.
    /// Stored as either a single DefinedValue.Guid or a comma-delimited list of DefinedValue.Guids (if AllowMultiple).
    /// </summary>
    [Serializable]
    public class DefinedValueFieldType : FieldType, IEntityFieldType, IEntityQualifierFieldType, ICachedEntitiesFieldType
    {
        #region Configuration

        private const string DEFINED_TYPE_KEY = "definedtype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string DISPLAY_DESCRIPTION = "displaydescription";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";

        /// <summary>
        /// Returns a list of the configuration keys.
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( DEFINED_TYPE_KEY );
            configKeys.Add( ALLOW_MULTIPLE_KEY );
            configKeys.Add( DISPLAY_DESCRIPTION );
            configKeys.Add( ENHANCED_SELECTION_KEY );
            configKeys.Add( INCLUDE_INACTIVE_KEY );
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
            var ddlDefinedType = new RockDropDownList();
            controls.Add( ddlDefinedType );
            ddlDefinedType.AutoPostBack = true;
            ddlDefinedType.SelectedIndexChanged += OnQualifierUpdated;
            ddlDefinedType.Label = "Defined Type";
            ddlDefinedType.Help = "The Defined Type to select values from.";

            Rock.Model.DefinedTypeService definedTypeService = new Model.DefinedTypeService( new RockContext() );
            foreach ( var definedType in definedTypeService.Queryable().OrderBy( d => d.Name ) )
            {
                ddlDefinedType.Items.Add( new ListItem( definedType.Name, definedType.Id.ToString() ) );
            }

            // Add checkbox for deciding if the defined values list is rendered as a drop
            // down list or a checkbox list.
            var cbAllowMultipleValues = new RockCheckBox();
            controls.Add( cbAllowMultipleValues );
            cbAllowMultipleValues.AutoPostBack = true;
            cbAllowMultipleValues.CheckedChanged += OnQualifierUpdated;
            cbAllowMultipleValues.Label = "Allow Multiple Values";
            cbAllowMultipleValues.Text = "Yes";
            cbAllowMultipleValues.Help = "When set, allows multiple defined type values to be selected.";

            // option for Display Descriptions
            var cbDescription = new RockCheckBox();
            controls.Add( cbDescription );
            cbDescription.AutoPostBack = true;
            cbDescription.CheckedChanged += OnQualifierUpdated;
            cbDescription.Label = "Display Descriptions";
            cbDescription.Text = "Yes";
            cbDescription.Help = "When set, the defined value descriptions will be displayed instead of the values.";

            // option for Displaying an enhanced 'chosen' value picker
            var cbEnhanced = new RockCheckBox();
            controls.Add( cbEnhanced );
            cbEnhanced.AutoPostBack = true;
            cbEnhanced.CheckedChanged += OnQualifierUpdated;
            cbEnhanced.Label = "Enhance For Long Lists";
            cbEnhanced.Text = "Yes";
            cbEnhanced.Help = "When set, will render a searchable selection of options.";

            // Add checkbox for deciding if the list should include inactive items
            var cbIncludeInactive = new RockCheckBox();
            controls.Add( cbIncludeInactive );
            cbIncludeInactive.AutoPostBack = true;
            cbIncludeInactive.CheckedChanged += OnQualifierUpdated;
            cbIncludeInactive.Label = "Include Inactive";
            cbIncludeInactive.Text = "Yes";
            cbIncludeInactive.Help = "When set, inactive defined values will be included in the list.";

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
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", "When set, will render a searchable selection of options.", string.Empty ) );
            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", "When set, inactive defined values will be included in the list.", string.Empty ) );

            if ( controls != null )
            {
                DropDownList ddlDefinedType = controls.Count > 0 ? controls[0] as DropDownList : null;
                CheckBox cbAllowMultipleValues = controls.Count > 1 ? controls[1] as CheckBox : null;
                CheckBox cbDescription = controls.Count > 2 ? controls[2] as CheckBox : null;
                CheckBox cbEnhanced = controls.Count > 3 ? controls[3] as CheckBox : null;
                CheckBox cbIncludeInactive = controls.Count > 4 ? controls[4] as CheckBox : null;

                if ( ddlDefinedType != null )
                {
                    configurationValues[DEFINED_TYPE_KEY].Value = ddlDefinedType.SelectedValue;
                }

                if ( cbAllowMultipleValues != null )
                {
                    configurationValues[ALLOW_MULTIPLE_KEY].Value = cbAllowMultipleValues.Checked.ToString();
                }

                if ( cbDescription != null )
                {
                    configurationValues[DISPLAY_DESCRIPTION].Value = cbDescription.Checked.ToString();
                }

                if ( cbEnhanced != null )
                {
                    configurationValues[ENHANCED_SELECTION_KEY].Value = cbEnhanced.Checked.ToString();
                }

                if ( cbIncludeInactive != null )
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = cbIncludeInactive.Checked.ToString();
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
                DropDownList ddlDefinedType = controls.Count > 0 ? controls[0] as DropDownList : null;
                CheckBox cbAllowMultipleValues = controls.Count > 1 ? controls[1] as CheckBox : null;
                CheckBox cbDescription = controls.Count > 2 ? controls[2] as CheckBox : null;
                CheckBox cbEnhanced = controls.Count > 3 ? controls[3] as CheckBox : null;
                CheckBox cbIncludeInactive = controls.Count > 4 ? controls[4] as CheckBox : null;

                if ( ddlDefinedType != null )
                {
                    ddlDefinedType.SelectedValue = configurationValues.GetValueOrNull( DEFINED_TYPE_KEY );
                }

                if ( cbAllowMultipleValues != null )
                {
                    cbAllowMultipleValues.Checked = configurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBooleanOrNull() ?? false;
                }

                if ( cbDescription != null )
                {
                    cbDescription.Checked = configurationValues.GetValueOrNull( DISPLAY_DESCRIPTION ).AsBooleanOrNull() ?? false;
                }

                if ( cbEnhanced != null )
                {
                    cbEnhanced.Checked = configurationValues.GetValueOrNull( ENHANCED_SELECTION_KEY ).AsBooleanOrNull() ?? false;
                }

                if ( cbIncludeInactive != null )
                {
                    cbIncludeInactive.Checked = configurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBooleanOrNull() ?? false;
                }
            }
        }

        #endregion

        #region EntityQualifierConfiguration

        /// <summary>
        /// Gets the configuration values for this field using the EntityTypeQualiferColumn and EntityTypeQualifierValues
        /// </summary>
        /// <param name="entityTypeQualifierColumn">The entity type qualifier column.</param>
        /// <param name="entityTypeQualifierValue">The entity type qualifier value.</param>
        /// <returns></returns>
        public Dictionary<string, Rock.Field.ConfigurationValue> GetConfigurationValuesFromEntityQualifier( string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( DEFINED_TYPE_KEY, new ConfigurationValue( "Defined Type", "The Defined Type to select values from", string.Empty ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple defined type values to be selected.", string.Empty ) );
            configurationValues.Add( DISPLAY_DESCRIPTION, new ConfigurationValue( "Display Descriptions", "When set, the defined value descriptions will be displayed instead of the values.", string.Empty ) );
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", "When set, will render a searchable selection of options.", string.Empty ) );

            if ( entityTypeQualifierColumn.Equals( "DefinedTypeId", StringComparison.OrdinalIgnoreCase ) )
            {
                configurationValues[DEFINED_TYPE_KEY].Value = entityTypeQualifierValue;
            }

            return configurationValues;
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
                    var definedValue = DefinedValueCache.Get( guid );
                    if ( definedValue != null )
                    {
                        names.Add( useDescription ? definedValue.Description : definedValue.Value );
                    }
                }

                formattedValue = names.AsDelimited( ", " );
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                bool useDescription = false;
                if ( configurationValues != null &&
                     configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) &&
                     configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean() )
                {
                    useDescription = true;
                }

                // if there are multiple defined values, just pick the first one as the sort value
                Guid guid = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList().FirstOrDefault();
                var definedValue = DefinedValueCache.Get( guid );
                if ( definedValue != null )
                {
                    // sort by Order then Description/Value (using a padded string)
                    var sortValue = definedValue.Order.ToString().PadLeft( 10 ) + "," + ( useDescription ? definedValue.Description : definedValue.Value );
                    return sortValue;
                }
            }

            return base.SortValue( parentControl, value, configurationValues );
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

            bool useDescription = configurationValues != null && configurationValues.ContainsKey( DISPLAY_DESCRIPTION ) && configurationValues[DISPLAY_DESCRIPTION].Value.AsBoolean();
            int? definedTypeId = configurationValues != null && configurationValues.ContainsKey( DEFINED_TYPE_KEY ) ? configurationValues[DEFINED_TYPE_KEY].Value.AsIntegerOrNull() : null;

            if ( definedTypeId.HasValue )
            {
                var definedType = DefinedTypeCache.Get( definedTypeId.Value );

            }
            if ( configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean() )
            {
                if ( configurationValues != null && configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) && configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() )
                {
                    editControl = new DefinedValuesPickerEnhanced { ID = id, DisplayDescriptions = useDescription, DefinedTypeId = definedTypeId };
                }
                else
                {
                    editControl = new DefinedValuesPicker { ID = id, DisplayDescriptions = useDescription, DefinedTypeId = definedTypeId };
                }
            }
            else
            {
                editControl = new DefinedValuePicker { ID = id, DisplayDescriptions = useDescription, DefinedTypeId = definedTypeId };
                if ( configurationValues != null && configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) && configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() )
                {
                    ( ( DefinedValuePicker ) editControl ).EnhanceForLongLists = true;
                }
            }

            if ( editControl is IDefinedValuePicker )
            {
                ( editControl as IDefinedValuePicker ).IncludeInactive = configurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBooleanOrNull() ?? false;
            }

            if ( definedTypeId.HasValue )
            {
                return editControl;
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

            var definedValuePicker = control as IDefinedValuePicker;

            if ( definedValuePicker != null )
            {
                definedValueIdList = definedValuePicker.SelectedDefinedValuesId.ToList();
            }

            var guids = new List<Guid>();

            foreach ( int definedValueId in definedValueIdList )
            {
                var definedValue = DefinedValueCache.Get( definedValueId );
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
                var definedValuePicker = control as IDefinedValuePicker;
                if ( definedValuePicker != null )
                {
                    var ids = new List<int>();
                    foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                    {
                        var definedValue = DefinedValueCache.Get( guid );
                        if ( definedValue != null )
                        {
                            ids.Add( definedValue.Id );
                        }
                    }

                    definedValuePicker.SelectedDefinedValuesId = ids.ToArray();
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
        /// <param name="filterMode">The filter mode.</param>
        /// <returns></returns>
        public override Control FilterValueControl( Dictionary<string, ConfigurationValue> configurationValues, string id, bool required, FilterMode filterMode )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();

            var overrideConfigValues = new Dictionary<string, ConfigurationValue>();
            foreach ( var keyVal in configurationValues )
            {
                overrideConfigValues.Add( keyVal.Key, keyVal.Value );
            }

            overrideConfigValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new ConfigurationValue( ( true ).ToString() ) );

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
        /// <param name="filterMode">The filter mode.</param>
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
        /// Gets the filter value value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetFilterValueValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            string value = base.GetFilterValueValue( control, configurationValues );
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            if ( allowMultiple && string.IsNullOrWhiteSpace( value ) )
            {
                return null;
            }

            return value;
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
                var definedValue = DefinedValueCache.Get( guid );
                if ( definedValue != null )
                {
                    values.Add( useDescription ? definedValue.Description : definedValue.Value );
                }
            }

            return AddQuotes( values.ToList().AsDelimited( "' OR '" ) );
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
                        var dv = DefinedValueCache.Get( value.AsGuid() );
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
        /// Gets a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, ParameterExpression parameterExpression )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            List<string> selectedValues;
            if ( allowMultiple || filterValues.Count != 1 )
            {
                ComparisonType comparisonType = filterValues[0].ConvertToEnum<ComparisonType>( ComparisonType.Contains );

                // if it isn't either "Contains" or "Not Contains", just use the base AttributeFilterExpression
                if ( !( new ComparisonType[] { ComparisonType.Contains, ComparisonType.DoesNotContain } ).Contains( comparisonType ) )
                {
                    return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
                }

                //// OR up the where clauses for each of the selected values 
                // and make sure to wrap commas around things so we don't collide with partial matches
                // so it'll do something like this:
                //
                // WHERE ',' + Value + ',' like '%,bacon,%'
                // OR ',' + Value + ',' like '%,lettuce,%'
                // OR ',' + Value + ',' like '%,tomato,%'

                if ( filterValues.Count > 1 )
                {
                    selectedValues = filterValues[1].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                }
                else
                {
                    selectedValues = new List<string>();
                }

                Expression comparison = null;

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

                if ( comparison == null )
                {
                    // No Value specified, so return NoAttributeFilterExpression ( which means don't filter )
                    return new NoAttributeFilterExpression();
                }
                else
                {
                    return comparison;
                }
            }

            selectedValues = filterValues[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            int valueCount = selectedValues.Count();
            MemberExpression propertyExpression = Expression.Property( parameterExpression, "Value" );
            if ( valueCount == 0 )
            {
                // No Value specified, so return NoAttributeFilterExpression ( which means don't filter )
                return new NoAttributeFilterExpression();
            }
            else if ( valueCount == 1 )
            {
                // only one value, so do an Equal instead of Contains which might compile a little bit faster
                ComparisonType comparisonType = ComparisonType.EqualTo;
                return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, AttributeConstantExpression( selectedValues[0] ) );
            }
            else
            {
                ConstantExpression constantExpression = Expression.Constant( selectedValues, typeof( List<string> ) );
                return Expression.Call( constantExpression, typeof( List<string> ).GetMethod( "Contains", new Type[] { typeof( string ) } ), propertyExpression );
            }
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
            var item = DefinedValueCache.Get( guid );
            return item != null ? item.Id : ( int? ) null;
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
                item = DefinedValueCache.Get( id.Value );
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

        #region ICachedEntitiesFieldType Members
        /// <summary>
        /// Gets the cached defined values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var definedValues = new List<IEntityCache>();

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var definedValue = DefinedValueCache.Get( guid );
                    if ( definedValue != null )
                    {
                        definedValues.Add( definedValue );
                    }
                }
            }

            return definedValues;
        }
        #endregion
    }
}