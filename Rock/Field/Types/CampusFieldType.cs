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

#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Campus
    /// Stored as Campus's Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><g><path d=""M5.38,7.67A.32.32,0,0,0,5.7,8H6.8a.32.32,0,0,0,.32-.33V6.58a.32.32,0,0,0-.32-.33H5.7a.32.32,0,0,0-.32.33ZM7.12,10.3V9.2a.32.32,0,0,0-.32-.32H5.7a.32.32,0,0,0-.32.32v1.1a.32.32,0,0,0,.32.32H6.8A.32.32,0,0,0,7.12,10.3ZM5.7,5.38H6.8a.32.32,0,0,0,.32-.33V4a.32.32,0,0,0-.32-.33H5.7A.32.32,0,0,0,5.38,4v1.1A.32.32,0,0,0,5.7,5.38ZM11.5,1h-7A1.75,1.75,0,0,0,2.75,2.75V14.34a.66.66,0,1,0,1.31,0V2.75a.44.44,0,0,1,.44-.44h7a.44.44,0,0,1,.44.44V14.34a.66.66,0,1,0,1.31,0V2.75A1.75,1.75,0,0,0,11.5,1ZM10.3,3.62H9.2A.32.32,0,0,0,8.88,4v1.1a.32.32,0,0,0,.32.33h1.1a.32.32,0,0,0,.32-.33V4A.32.32,0,0,0,10.3,3.62Zm0,2.63H9.2a.32.32,0,0,0-.32.33V7.67A.32.32,0,0,0,9.2,8h1.1a.32.32,0,0,0,.32-.33V6.58A.32.32,0,0,0,10.3,6.25Zm0,2.63H9.2a.32.32,0,0,0-.32.32v1.1a.32.32,0,0,0,.32.32h1.1a.32.32,0,0,0,.32-.32V9.2A.32.32,0,0,0,10.3,8.88ZM8,11.5a1.35,1.35,0,0,0-1.29,1.37v1.47a.65.65,0,0,0,.65.66H8.66a.65.65,0,0,0,.65-.66V12.85A1.35,1.35,0,0,0,8,11.5Z""/></g></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CAMPUS )]
    public class CampusFieldType : FieldType, IEntityFieldType, ICachedEntitiesFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string FILTER_CAMPUS_TYPES_KEY = "filterCampusTypes";
        private const string FILTER_CAMPUS_STATUS_KEY = "filterCampusStatus";
        private const string FORCE_VISIBLE_KEY = "forceVisible";
        private const string SELECTABLE_CAMPUSES_KEY = "SelectableCampusIds";
        private const string VALUES_PUBLIC_KEY = "values";
        private const string VALUES_INACTIVE_KEY = "valuesInactive";
        private const string SELECTABLE_CAMPUSES_PUBLIC_KEY = "selectableCampuses";
        private const string CAMPUSES_PROPERTY_KEY = "campuses";
        private const string CAMPUS_TYPES_PROPERTY_KEY = "campusTypes";
        private const string CAMPUS_STATUSES_PROPERTY_KEY = "campusStatuses";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicEditConfigurationProperties( Dictionary<string, string> privateConfigurationValues )
        {
            using ( var rockContext = new RockContext() )
            {
                var configurationProperties = new Dictionary<string, string>();

                // Disable security since we are intentionally returning items even
                // if the person (whom we don't know) doesn't have access.
                var definedValueClientService = new Rock.ClientService.Core.DefinedValue.DefinedValueClientService( rockContext, null )
                {
                    EnableSecurity = false
                };

                // Get the campus types and campus status values that are available
                // for the person to choose in the pickers.
                var campusTypes = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.CAMPUS_TYPE.AsGuid() );
                var campusStatuses = definedValueClientService.GetDefinedValuesAsListItems( SystemGuid.DefinedType.CAMPUS_STATUS.AsGuid() );

                // Get the campuses that are available to be selected.
                var campuses = CampusCache.All()
                    .OrderBy( c => c.Order )
                    .ThenBy( c => c.Name )
                    .Select( c => new CampusItemViewModel
                    {
                        Guid = c.Guid,
                        Name = c.Name,
                        Type = c.CampusTypeValueId.HasValue ? ( Guid? ) DefinedValueCache.Get( c.CampusTypeValueId.Value ).Guid : null,
                        Status = c.CampusStatusValueId.HasValue ? ( Guid? ) DefinedValueCache.Get( c.CampusStatusValueId.Value ).Guid : null,
                        IsActive = c.IsActive ?? false
                    } )
                    .ToList();

                configurationProperties[CAMPUS_TYPES_PROPERTY_KEY] = campusTypes.ToCamelCaseJson( false, true );
                configurationProperties[CAMPUS_STATUSES_PROPERTY_KEY] = campusStatuses.ToCamelCaseJson( false, true );
                configurationProperties[CAMPUSES_PROPERTY_KEY] = campuses.ToCamelCaseJson( false, true );

                return configurationProperties;
            }
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string privateValue )
        {
            var publicConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, privateValue );

            if ( usage == ConfigurationValueUsage.Edit || usage == ConfigurationValueUsage.Configure )
            {
                // Convert the selectable values from integer identifiers into
                // unique identifiers.
                var selectableValues = privateConfigurationValues.GetValueOrDefault( SELECTABLE_CAMPUSES_KEY, string.Empty );
                publicConfigurationValues[SELECTABLE_CAMPUSES_PUBLIC_KEY] = ConvertDelimitedIdsToGuids( selectableValues, v => CampusCache.Get( v )?.Guid );
                publicConfigurationValues.Remove( SELECTABLE_CAMPUSES_KEY );

                // Convert the campus type options from integer identifiers into
                // unique identifiers that can be stored in the database.
                var campusTypes = privateConfigurationValues.GetValueOrDefault( FILTER_CAMPUS_TYPES_KEY, string.Empty );
                publicConfigurationValues[FILTER_CAMPUS_TYPES_KEY] = ConvertDelimitedIdsToGuids( campusTypes, v => DefinedValueCache.Get( v )?.Guid );

                // Convert the campus status options from integer identifiers into
                // unique identifiers that can be stored in the database.
                var campusStatus = privateConfigurationValues.GetValueOrDefault( FILTER_CAMPUS_STATUS_KEY, string.Empty );
                publicConfigurationValues[FILTER_CAMPUS_STATUS_KEY] = ConvertDelimitedIdsToGuids( campusStatus, v => DefinedValueCache.Get( v )?.Guid );
            }

            var publicValues = GetListSource( privateConfigurationValues )
                .Select( kvp => new ListItemBag
                {
                    Value = kvp.Key,
                    Text = kvp.Value
                } );

            var inactiveListItems = GetInactiveListSource( privateConfigurationValues )
                .Select( kvp => new ListItemBag
                {
                    Value = kvp.Key,
                    Text = kvp.Value
                } );

            if ( usage == ConfigurationValueUsage.View )
            {
                publicValues = publicValues.Where( v => v.Value.AsGuid() == privateValue.AsGuid() );
            }

            publicConfigurationValues[VALUES_PUBLIC_KEY] = publicValues.ToCamelCaseJson( false, true );
            publicConfigurationValues[VALUES_INACTIVE_KEY] = inactiveListItems.ToCamelCaseJson( false, true );
            if ( usage == ConfigurationValueUsage.View )
            {
                publicConfigurationValues.Remove( INCLUDE_INACTIVE_KEY );
                publicConfigurationValues.Remove( FILTER_CAMPUS_TYPES_KEY );
                publicConfigurationValues.Remove( FILTER_CAMPUS_STATUS_KEY );
                publicConfigurationValues.Remove( SELECTABLE_CAMPUSES_KEY );
            }

            return publicConfigurationValues;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfigurationValues( Dictionary<string, string> publicConfigurationValues )
        {
            var privateConfigurationValues = base.GetPrivateConfigurationValues( publicConfigurationValues );

            // Convert the selectable values from unique identifiers into
            // integer identifiers that can be stored in the database.
            var selectableValues = publicConfigurationValues.GetValueOrDefault( SELECTABLE_CAMPUSES_PUBLIC_KEY, string.Empty );
            privateConfigurationValues[SELECTABLE_CAMPUSES_KEY] = ConvertDelimitedGuidsToIds( selectableValues, v => CampusCache.Get( v )?.Id );
            privateConfigurationValues.Remove( SELECTABLE_CAMPUSES_PUBLIC_KEY );

            // Convert the campus type options from unique identifiers into
            // integer identifiers that can be stored in the database.
            var campusTypes = publicConfigurationValues.GetValueOrDefault( FILTER_CAMPUS_TYPES_KEY, string.Empty );
            privateConfigurationValues[FILTER_CAMPUS_TYPES_KEY] = ConvertDelimitedGuidsToIds( campusTypes, v => DefinedValueCache.Get( v )?.Id );

            // Convert the campus status options from unique identifiers into
            // integer identifiers that can be stored in the database.
            var campusStatus = publicConfigurationValues.GetValueOrDefault( FILTER_CAMPUS_STATUS_KEY, string.Empty );
            privateConfigurationValues[FILTER_CAMPUS_STATUS_KEY] = ConvertDelimitedGuidsToIds( campusStatus, v => DefinedValueCache.Get( v )?.Id );

            return privateConfigurationValues;
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

        /// <summary>
        /// Gets the inactive list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        private Dictionary<string, string> GetInactiveListSource( Dictionary<string, string> configurationValues )
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
                .Where( c => !includeInactive && c.IsActive.HasValue && !c.IsActive.Value
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

        #endregion

        #region Edit Control

        #endregion

        #region Filter Control

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
        /// Gets the equal to compare value (types that don't support an equalto comparison (i.e. singleselect) should return null
        /// </summary>
        /// <returns></returns>
        public override string GetEqualToCompareValue()
        {
            return null;
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

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            var campusId = CampusCache.GetId( guid.Value );

            if ( !campusId.HasValue )
            {
                return null;
            }


            return new List<ReferencedEntity>
            {
                new ReferencedEntity( EntityTypeCache.GetId<Campus>().Value, campusId.Value )
            };
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Campus and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Campus>().Value, nameof( Campus.Name ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

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
            configurationValues.Add( FORCE_VISIBLE_KEY, new ConfigurationValue( " Force Visible", "Specify the campuses eligible for this control. If none are specified then all will be displayed.", string.Empty ) );

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
        /// Returns the formatted selected campus. If there is only one campus then nothing is returned.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            // Never use the condensed value for webforms blocks.
            return GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

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

        /// <inheritdoc/>
        public override ComparisonValue GetPublicFilterValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var values = privateValue.FromJsonOrNull<List<string>>();

            if ( values?.Count == 1 )
            {
                var selectedValues = values[0].Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                if ( selectedValues.Count == 0 )
                {
                    return new ComparisonValue
                    {
                        Value = string.Empty
                    };
                }
                else
                {
                    return new ComparisonValue
                    {
                        ComparisonType = ComparisonType.Contains,
                        Value = selectedValues.Select( v => GetPublicEditValue( v, privateConfigurationValues ) ).JoinStrings( "," )
                    };
                }
            }
            else
            {
                return base.GetPublicFilterValue( privateValue, privateConfigurationValues );
            }
        }

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
            return item != null ? item.Id : ( int? ) null;
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

#endif
        #endregion

        internal class CampusItemViewModel
        {
            public Guid Guid { get; set; }

            public string Name { get; set; }

            public Guid? Type { get; set; }

            public Guid? Status { get; set; }

            public bool IsActive { get; set; }
        }
    }
}