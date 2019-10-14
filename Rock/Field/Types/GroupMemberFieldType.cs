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
using System.Data.Entity;
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
    /// Field Type used to display a dropdown list of Group Members for a specific Group.
    /// Stored as either a single GroupMember.Guid or a comma-delimited list of GroupMember.Guids (if AllowMultiple)
    /// </summary>
    [Serializable]
    public class GroupMemberFieldType : FieldType, IEntityFieldType, IEntityQualifierFieldType
    {
        #region Controls

        private GroupPicker _gpGroupPicker;
        private int? _groupId;

        #endregion

        #region Configuration

        private const string GROUP_KEY = "group";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( GROUP_KEY );
            configKeys.Add( ALLOW_MULTIPLE_KEY );
            configKeys.Add( ENHANCED_SELECTION_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field.
        /// IMPORTANT! The order of the controls must match the order/index usage
        /// in the ConfigurationValues and SetConfigurationValues methods.
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Save the selected groupType before we rebind it.
            int? groupId = null;
            if ( _gpGroupPicker != null )
            {
                groupId = _gpGroupPicker.SelectedValue.AsIntegerOrNull();

                // store this so we know if the Group actually changes when the SelectedIndexChanged is fired.
                _groupId = groupId;
            }

            // build a group picker (the one that gets selected is
            // used to build a list of groupmember values) 
            _gpGroupPicker = new GroupPicker();
            _gpGroupPicker.Label = "Group";
            _gpGroupPicker.Help = "The Group to select the member(s) from.";
            _gpGroupPicker.SelectItem += OnQualifierUpdated;
            if ( groupId.HasValue )
            {
                _gpGroupPicker.SetValue( groupId.Value );
            }
            controls.Add( _gpGroupPicker );

            // Add checkbox for deciding if the group member picker list is rendered as a drop
            // down list or a checkbox list.
            var cb = new RockCheckBox();
            controls.Add( cb );
            cb.AutoPostBack = true;
            cb.CheckedChanged += OnQualifierUpdated;
            cb.Label = "Allow Multiple Values";
            cb.Text = "Yes";
            cb.Help = "When set, allows multiple group members to be selected.";

            // option for Displaying an enhanced 'chosen' value picker
            var cbEnanced = new RockCheckBox();
            controls.Add( cbEnanced );
            cbEnanced.AutoPostBack = true;
            cbEnanced.CheckedChanged += OnQualifierUpdated;
            cbEnanced.Label = "Enhance For Long Lists";
            cbEnanced.Text = "Yes";
            cbEnanced.Help = "When set, will render a searchable selection of options.";

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
            configurationValues.Add( GROUP_KEY, new ConfigurationValue( "Group", "The Group to select members from.", string.Empty ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple group members to be selected.", string.Empty ) );
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", "When set, will render a searchable selection of options.", string.Empty ) );

            if ( controls != null )
            {
                int i = 0;
                if ( controls.Count > i && controls[i] != null && controls[i] is GroupPicker )
                {
                    configurationValues[GROUP_KEY].Value = ( ( GroupPicker ) controls[i] ).SelectedValue;
                }

                i++;
                if ( controls.Count > i && controls[i] != null && controls[i] is CheckBox )
                {
                    configurationValues[ALLOW_MULTIPLE_KEY].Value = ( (CheckBox)controls[i] ).Checked.ToString();
                }
                i++;
                if ( controls.Count > i && controls[i] != null && controls[i] is CheckBox )
                {
                    configurationValues[ENHANCED_SELECTION_KEY].Value = ( (CheckBox)controls[i] ).Checked.ToString();
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
                int i = 0;
                if ( controls.Count > i && controls[i] != null && controls[i] is GroupPicker && configurationValues.ContainsKey( GROUP_KEY ) )
                {
                    var gpGroupPicker = ( GroupPicker ) controls[i];
                    gpGroupPicker.SetValue( configurationValues[GROUP_KEY].Value.AsInteger() );
                }
                i++;
                if ( controls.Count > i && controls[i] != null && controls[i] is CheckBox && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) )
                {
                    ( (CheckBox)controls[i] ).Checked = configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
                }
                i++;
                if ( controls.Count > i && controls[i] != null && controls[i] is CheckBox && configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) )
                {
                    ( (CheckBox)controls[i] ).Checked = configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean();
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
        public Dictionary<string, Rock.Field.ConfigurationValue> GetConfigurationValuesFromEntityQualifier(string entityTypeQualifierColumn, string entityTypeQualifierValue)
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( GROUP_KEY, new ConfigurationValue( "Group", "The Group to select members from.", string.Empty ) );
            configurationValues.Add( ALLOW_MULTIPLE_KEY, new ConfigurationValue( "Allow Multiple Values", "When set, allows multiple group members to be selected.", string.Empty ) );
            configurationValues.Add( ENHANCED_SELECTION_KEY, new ConfigurationValue( "Enhance For Long Lists", "When set, will render a searchable selection of options.", string.Empty ) );

            if ( entityTypeQualifierColumn.Equals( "GroupId", StringComparison.OrdinalIgnoreCase ) )
            {
                configurationValues[GROUP_KEY].Value = entityTypeQualifierValue;
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
                var names = new List<string>();
                using ( var rockContext = new RockContext() )
                {
                    var groupMemberService = new GroupMemberService( rockContext );
                    foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                    {
                        var groupMember = groupMemberService.GetNoTracking( guid );
                        if ( groupMember != null )
                        {
                            names.Add( groupMember.Person.FullName );
                        }
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
                // if there are multiple group members, just pick the first one as the sort value
                Guid guid = value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList().FirstOrDefault();
                using ( var rockContext = new RockContext() )
                {
                    var groupMember = new GroupMemberService( rockContext ).Get( guid );
                    if ( groupMember != null )
                    {
                        // sort by Order then Description/Value (using a padded string)
                        return groupMember.Person.FullName;
                    }
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

            int? groupId = configurationValues != null && configurationValues.ContainsKey( GROUP_KEY ) ? configurationValues[GROUP_KEY].Value.AsIntegerOrNull() : null;

            if ( configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean() )
            {
                // Select multiple members
                editControl = new GroupMembersPicker { ID = id, GroupId = groupId };
            }
            else
            {
                // Select single member
                editControl = new GroupMemberPicker { ID = id, GroupId = groupId };
                if ( configurationValues != null && configurationValues.ContainsKey( ENHANCED_SELECTION_KEY ) && configurationValues[ENHANCED_SELECTION_KEY].Value.AsBoolean() )
                {
                    ( ( GroupMemberPicker ) editControl ).EnhanceForLongLists = true;
                }
            }

            return editControl;
        }

        /// <summary>
        /// Gets the selected GroupMember(s) as a comma-delimited list of GroupMember.Guid
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var groupMemberIdList = new List<int>();

            if ( control != null && control is ListControl )
            {
                groupMemberIdList.AddRange( ( ( ListControl ) control ).Items.Cast<ListItem>()
                    .Where( i => i.Selected )
                    .Select( i => i.Value ).AsIntegerList() );

                var guids = new List<Guid>();

                if ( groupMemberIdList.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var groupMemberService = new GroupMemberService( rockContext );
                        guids = groupMemberService.Queryable().AsNoTracking().Where( t => groupMemberIdList.Contains( t.Id ) ).Select( a => a.Guid ).ToList();
                    }
                }

                return guids.AsDelimited( "," );
            }

            return null;

        }

        /// <summary>
        /// Sets the value as a GroupMember.Guid or a List of GroupMember.Guids (as strings)
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as ListControl;
            if ( picker != null )
            {
                List<int> selectedGroupMemberIds = new List<int>();
                List<Guid> selectedGroupMemberGuids = value?.Split( ',' ).AsGuidList();
                if ( selectedGroupMemberGuids != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        selectedGroupMemberIds = new GroupMemberService( rockContext ).GetByGuids( selectedGroupMemberGuids ).Select( a => a.Id ).ToList();
                    }
                }

                foreach ( ListItem li in picker.Items )
                {
                    li.Selected = selectedGroupMemberIds.Contains( li.Value.AsInteger() );
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
            var values = new List<string>();
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                foreach ( Guid guid in value.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList() )
                {
                    var groupMember = groupMemberService.Get( guid );
                    if ( groupMember != null )
                    {
                        values.Add( groupMember.Person.FullName );
                    }
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
        /// a '$selectedContent' should be used to limit script to currently selected filter fields.
        /// The script should be in the reportingInclude.js file.
        /// </remarks>
        public override string GetFilterFormatScript( Dictionary<string, ConfigurationValue> configurationValues, string title )
        {
            bool allowMultiple = configurationValues != null && configurationValues.ContainsKey( ALLOW_MULTIPLE_KEY ) && configurationValues[ALLOW_MULTIPLE_KEY].Value.AsBoolean();
            if ( allowMultiple )
            {
                return base.GetFilterFormatScript( configurationValues, title );
            }

            string titleJs = System.Web.HttpUtility.JavaScriptStringEncode( title );
            var format = "return Rock.reporting.formatFilterForGroupMemberField('{0}', $selectedContent);";
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

                using ( var rockContext = new RockContext() )
                {
                    var groupMemberService = new GroupMemberService( rockContext );

                    foreach ( string value in selectedValues )
                    {
                        string tempValue = value;

                        // if this is not for an attribute value, look up the id for the group member
                        if ( propertyName != "Value" || propertyType != typeof( string ) )
                        {
                            var gm = groupMemberService.Get( value.AsGuid() );
                            tempValue = gm != null ? gm.Id.ToString() : string.Empty;
                        }

                        if ( !string.IsNullOrWhiteSpace( tempValue ) )
                        {
                            object obj = Convert.ChangeType( tempValue, type );
                            specificListType.GetMethod( "Add" ).Invoke( specificList, new object[] { obj } );
                        }
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
                if ( !( new ComparisonType[] { ComparisonType.Contains, ComparisonType.DoesNotContain }).Contains(comparisonType))
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
            GroupMember item = null;
            using ( var rockContext = new RockContext() )
            {
                var groupMemberService = new GroupMemberService( rockContext );

                Guid guid = GetEditValue( control, configurationValues ).AsGuid();
                item = groupMemberService.Get( guid );
            }

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
            GroupMember item = null;
            if ( id.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groupMemberService = new GroupMemberService( rockContext );

                    item = groupMemberService.Get( id.Value );
                }
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
                return new GroupMemberService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion
    }
}