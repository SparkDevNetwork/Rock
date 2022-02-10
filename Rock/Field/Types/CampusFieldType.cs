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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Campus
    /// Stored as Campus's Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class CampusFieldType : FieldType, IEntityFieldType, ICachedEntitiesFieldType
    {
        #region Configuration

        private const string CLIENT_VALUES = "values";
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string FILTER_CAMPUS_TYPES_KEY = "filterCampusTypes";
        private const string FILTER_CAMPUS_STATUS_KEY = "filterCampusStatus";
        private const string FORCE_VISIBLE_KEY = "forceVisible";
        private const string SELECTABLE_CAMPUSES_KEY = "SelectableCampusIds";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( INCLUDE_INACTIVE_KEY );
            configKeys.Add( FILTER_CAMPUS_TYPES_KEY );
            configKeys.Add( FILTER_CAMPUS_STATUS_KEY );
            configKeys.Add( SELECTABLE_CAMPUSES_KEY );
            return configKeys;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues )
        {
            var clientValues = GetListSource( privateConfigurationValues )
                    .Select( kvp => new ListItemViewModel
                    {
                        Value = kvp.Key,
                        Text = kvp.Value
                    } )
                    .ToList()
                    .ToCamelCaseJson( false, true );

            return new Dictionary<string, string>
            {
                [CLIENT_VALUES] = clientValues
            };
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            // Add checkbox for deciding if the list should include inactive items
            var cbIncludeInactive = new RockCheckBox();
            cbIncludeInactive.AutoPostBack = true;
            cbIncludeInactive.CheckedChanged += OnQualifierUpdated;
            cbIncludeInactive.Label = "Include Inactive";
            cbIncludeInactive.Text = "Yes";
            cbIncludeInactive.Help = "When set, inactive campuses will be included in the list.";

            // Checkbox list to select Filter Campus Types
            var campusTypeDefinedValues = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid() ).DefinedValues.Select( v => new { Text = v.Value, Value = v.Id } );
            var cblCampusTypes = new RockCheckBoxList();
            cblCampusTypes.AutoPostBack = true;
            cblCampusTypes.SelectedIndexChanged += OnQualifierUpdated;
            cblCampusTypes.RepeatDirection = RepeatDirection.Horizontal;
            cblCampusTypes.Label = "Filter Campus Types";
            cblCampusTypes.Help = "When set this will filter the campuses displayed in the list to the selected Types. Setting a filter will cause the campus picker to display even if 0 campuses are in the list.";
            cblCampusTypes.DataTextField = "Text";
            cblCampusTypes.DataValueField = "Value";
            cblCampusTypes.DataSource = campusTypeDefinedValues;
            cblCampusTypes.DataBind();

            // Checkbox list to select Filter Campus Status
            var campusStatusDefinedValues = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.CAMPUS_STATUS.AsGuid() ).DefinedValues.Select( v => new { Text = v.Value, Value = v.Id } );
            var cblCampusStatuses = new RockCheckBoxList();
            cblCampusStatuses.AutoPostBack = true;
            cblCampusStatuses.SelectedIndexChanged += OnQualifierUpdated;
            cblCampusStatuses.RepeatDirection = RepeatDirection.Horizontal;
            cblCampusStatuses.Label = "Filter Campus Status";
            cblCampusStatuses.Help = "When set this will filter the campuses displayed in the list to the selected Statuses. Setting a filter will cause the campus picker to display even if 0 campuses are in the list.";
            cblCampusStatuses.DataTextField = "Text";
            cblCampusStatuses.DataValueField = "Value";
            cblCampusStatuses.DataSource = campusStatusDefinedValues;
            cblCampusStatuses.DataBind();

            var activeCampuses = CampusCache.All( false ).Select( v => new { Text = v.Name, Value = v.Id } );
            var cblSelectableCampuses = new RockCheckBoxList
            {
                AutoPostBack = true,
                RepeatDirection = RepeatDirection.Horizontal,
                Label = "Selectable Campuses",
                DataTextField = "Text",
                DataValueField = "Value",
                DataSource = activeCampuses
            };

            cblCampusStatuses.SelectedIndexChanged += OnQualifierUpdated;
            cblSelectableCampuses.DataBind();

            var controls = base.ConfigurationControls();
            controls.Add( cbIncludeInactive );
            controls.Add( cblCampusTypes );
            controls.Add( cblCampusStatuses );
            controls.Add( cblSelectableCampuses );

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
            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", "When set, inactive campuses will be included in the list.", string.Empty ) );
            configurationValues.Add( FILTER_CAMPUS_TYPES_KEY, new ConfigurationValue( "Filter Campus Types", string.Empty, string.Empty ) );
            configurationValues.Add( FILTER_CAMPUS_STATUS_KEY, new ConfigurationValue( "Filter Campus Status", string.Empty, string.Empty ) );
            configurationValues.Add( SELECTABLE_CAMPUSES_KEY, new ConfigurationValue( " Selectable Campuses", "Specify the campuses eligible for this control. If none are specified then all will be displayed.", string.Empty ) );

            if ( controls != null )
            {
                CheckBox cbIncludeInactive = controls.Count > 0 ? controls[0] as CheckBox : null;
                RockCheckBoxList cblCampusTypes = controls.Count > 1 ? controls[1] as RockCheckBoxList : null;
                RockCheckBoxList cblCampusStatuses = controls.Count > 2 ? controls[2] as RockCheckBoxList : null;
                RockCheckBoxList cblSelectableValues = controls.Count > 3 ? controls[3] as RockCheckBoxList : null;

                if ( cbIncludeInactive != null )
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = cbIncludeInactive.Checked.ToString();
                }

                if ( cblCampusTypes != null )
                {
                    configurationValues[FILTER_CAMPUS_TYPES_KEY].Value = string.Join( ",", cblCampusTypes.SelectedValues );
                }

                if ( cblCampusStatuses != null )
                {
                    configurationValues[FILTER_CAMPUS_STATUS_KEY].Value = string.Join( ",", cblCampusStatuses.SelectedValues );
                }

                if ( cblSelectableValues != null )
                {
                    var selectableValues = new List<string>( cblSelectableValues.SelectedValues );

                    var activeCampuses = CampusCache.All( cbIncludeInactive.Checked ).Select( v => new { Text = v.Name, Value = v.Id } );
                    cblSelectableValues.DataSource = activeCampuses;
                    cblSelectableValues.DataBind();

                    if ( selectableValues != null && selectableValues.Any() )
                    {
                        foreach ( ListItem listItem in cblSelectableValues.Items )
                        {
                            listItem.Selected = selectableValues.Contains( listItem.Value );
                        }
                    }

                    configurationValues[SELECTABLE_CAMPUSES_KEY].Value = string.Join( ",", cblSelectableValues.SelectedValues );
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
                CheckBox cbIncludeInactive = controls.Count > 0 ? controls[0] as CheckBox : null;
                RockCheckBoxList cblCampusTypes = controls.Count > 1 ? controls[1] as RockCheckBoxList : null;
                RockCheckBoxList cblCampusStatuses = controls.Count > 2 ? controls[2] as RockCheckBoxList : null;
                RockCheckBoxList cblSelectableValues = controls.Count > 3 ? controls[3] as RockCheckBoxList : null;

                if ( cbIncludeInactive != null )
                {
                    cbIncludeInactive.Checked = configurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBooleanOrNull() ?? false;
                }
                
                if ( cblCampusTypes != null )
                {
                    var selectedCampusTypes = configurationValues.GetValueOrNull( FILTER_CAMPUS_TYPES_KEY )?.SplitDelimitedValues( false );
                    if ( selectedCampusTypes != null && selectedCampusTypes.Any() )
                    {
                        foreach ( ListItem listItem in cblCampusTypes.Items )
                        {
                            listItem.Selected = selectedCampusTypes.Contains( listItem.Value );
                        }
                    }
                }
                
                if ( cblCampusStatuses != null )
                {
                    var selectedCampusStatuses = configurationValues.GetValueOrNull( FILTER_CAMPUS_STATUS_KEY )?.SplitDelimitedValues( false );
                    if ( selectedCampusStatuses != null && selectedCampusStatuses.Any() )
                    {
                        foreach ( ListItem listItem in cblCampusStatuses.Items )
                        {
                            listItem.Selected = selectedCampusStatuses.Contains( listItem.Value );
                        }
                    }
                }

                if ( cblSelectableValues != null )
                {
                    var selectableValues = new List<string>( cblSelectableValues.SelectedValues );

                    var activeCampuses = CampusCache.All( cbIncludeInactive.Checked ).Select( v => new { Text = v.Name, Value = v.Id } );
                    cblSelectableValues.DataSource = activeCampuses;
                    cblSelectableValues.DataBind();

                    if ( selectableValues != null && selectableValues.Any() )
                    {
                        foreach ( ListItem listItem in cblSelectableValues.Items )
                        {
                            listItem.Selected = selectableValues.Contains( listItem.Value );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        private Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            return GetListSource( configurationValues.ToDictionary( k => k.Key, k => k.Value ) );
        }

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        private Dictionary<string, string> GetListSource( Dictionary<string, string> configurationValues )
        {
            var allCampuses = CampusCache.All();

            if ( configurationValues == null )
            {
                return allCampuses.ToDictionary( c => c.Guid.ToString(), c => c.Name );
            }

            bool includeInactive = configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].AsBoolean();
            List<int> campusTypesFilter = configurationValues.ContainsKey( FILTER_CAMPUS_TYPES_KEY ) ? configurationValues[FILTER_CAMPUS_TYPES_KEY].SplitDelimitedValues( false ).AsIntegerList() : null;
            List<int> campusStatusFilter = configurationValues.ContainsKey( FILTER_CAMPUS_STATUS_KEY ) ? configurationValues[FILTER_CAMPUS_STATUS_KEY].SplitDelimitedValues( false ).AsIntegerList() : null;
            List<int> selectableCampuses = configurationValues.ContainsKey( SELECTABLE_CAMPUSES_KEY ) && configurationValues[SELECTABLE_CAMPUSES_KEY].IsNotNullOrWhiteSpace()
                ? configurationValues[SELECTABLE_CAMPUSES_KEY].SplitDelimitedValues( false ).AsIntegerList()
                : null;

            var campusList = allCampuses
                .Where( c => ( !c.IsActive.HasValue || c.IsActive.Value || includeInactive )
                    && campusTypesFilter.ContainsOrEmpty( c.CampusTypeValueId ?? -1 )
                    && campusStatusFilter.ContainsOrEmpty( c.CampusStatusValueId ?? -1 )
                    && selectableCampuses.ContainsOrEmpty( c.Id ) )
                .ToList();

            return campusList.ToDictionary( c => c.Guid.ToString(), c => c.Name );
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string value, Dictionary<string, string> configurationValues )
        {
            if ( value == null )
            {
                return string.Empty;
            }

            var campus = CampusCache.Get( value.AsGuid() );

            return campus?.Name ?? string.Empty;
        }

        /// <summary>
        /// Returns the formatted selected campus. If there is only one campus then nothing is returned.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return GetTextValue( value, configurationValues.ToDictionary( k => k.Key, k => k.Value.Value ) );
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var campusPicker = new CampusPicker { ID = id };
            if ( configurationValues == null )
            {
                return campusPicker;
            }

            campusPicker.IncludeInactive = configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();

            if ( configurationValues.ContainsKey( FILTER_CAMPUS_TYPES_KEY ) )
            {
                campusPicker.CampusTypesFilter = configurationValues[FILTER_CAMPUS_TYPES_KEY].Value.SplitDelimitedValues( false ).ToList().AsIntegerList();
            }

            if ( configurationValues.ContainsKey( FILTER_CAMPUS_STATUS_KEY ) )
            {
                campusPicker.CampusStatusFilter = configurationValues[FILTER_CAMPUS_STATUS_KEY].Value.SplitDelimitedValues( false ).ToList().AsIntegerList();
            }

            if ( configurationValues.ContainsKey( FORCE_VISIBLE_KEY ) )
            {
                campusPicker.ForceVisible = configurationValues[FORCE_VISIBLE_KEY].Value.AsBoolean();
            }

            if ( configurationValues.ContainsKey( SELECTABLE_CAMPUSES_KEY ) && configurationValues[SELECTABLE_CAMPUSES_KEY].Value.IsNotNullOrWhiteSpace() )
            {
                int[] selectableValues = configurationValues[SELECTABLE_CAMPUSES_KEY].Value.Split( ',' ).Select( int.Parse ).ToArray();
                var selectableCampuses = CampusCache.All().Where( c => selectableValues.Contains( c.Id ) ).ToList();
                campusPicker.Campuses = selectableCampuses;
            }

            return campusPicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// returns Campus.Guid as string
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            CampusPicker campusPicker = control as CampusPicker;

            if ( campusPicker != null )
            {
                int? campusId = campusPicker.SelectedCampusId;
                if ( campusId.HasValue )
                {
                    var campus = CampusCache.Get( campusId.Value );
                    if ( campus != null )
                    {
                        return campus.Guid.ToString();
                    }
                }

                return string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// Expects value as a Campus.Guid as string
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            CampusPicker campusPicker = control as CampusPicker;

            if ( campusPicker != null )
            {
                Guid? guid = value.AsGuidOrNull();
                int? campusId = null;

                // get the item (or null) and set it
                if ( guid.HasValue )
                {
                    campusId = CampusCache.Get( guid.Value )?.Id;
                }

                campusPicker.SelectedCampusId = campusId;
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
            var cbList = new RockCheckBoxList();
            cbList.ID = string.Format( "{0}_cbList", id );
            cbList.AddCssClass( "js-filter-control" );
            cbList.RepeatDirection = RepeatDirection.Horizontal;

            bool includeInactive = configurationValues != null && configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();

            var campusList = CampusCache.All( includeInactive );
            if ( campusList.Any() )
            {
                foreach ( var campus in campusList )
                {
                    ListItem listItem = new ListItem( campus.Name, campus.Guid.ToString() );
                    cbList.Items.Add( listItem );
                }

                return cbList;
            }

            return null;
        }

        /// <summary>
        /// Formats the filter value value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public override string FormatFilterValueValue( Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var campusGuids = value.SplitDelimitedValues().AsGuidList();

            var campuses = campusGuids.Select( a => CampusCache.Get( a ) ).Where( c => c != null );
            return AddQuotes( campuses.Select( a => a.Name ).ToList().AsDelimited( "' OR '" ) );
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
        /// Gets the filters expression.
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

            return base.AttributeFilterExpression( configurationValues, filterValues, parameterExpression );
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = CampusCache.Get( guid );
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            CampusCache item = null;
            if ( id.HasValue )
            {
                item = CampusCache.Get( id.Value );
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
                return new CampusService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        /// <summary>
        /// Gets the cached entities as a list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var guids = value.SplitDelimitedValues().AsGuidList();
            var result = new List<IEntityCache>();

            result.AddRange( guids.Select( g => CampusCache.Get( g ) ) );

            return result;
        }

        #endregion
    }
}