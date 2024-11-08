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
#if WEBFORMS
using System.Web.UI;
using System.Web.UI.WebControls;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select multiple (or none) Campuses.
    /// Stored as Campus's Guid.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [IconSvg( @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 16 16""><path d=""M6.07,3.57a.39.39,0,0,1,.39-.4H7.65a.4.4,0,0,1,.39.4V4.75a.4.4,0,0,1-.39.4H6.46a.39.39,0,0,1-.39-.4Zm4.73-.4a.4.4,0,0,1,.4.4V4.75a.4.4,0,0,1-.4.4H9.62a.39.39,0,0,1-.39-.4V3.57a.39.39,0,0,1,.39-.4ZM6.07,6.73a.39.39,0,0,1,.39-.4H7.65a.4.4,0,0,1,.39.4V7.91a.4.4,0,0,1-.39.4H6.46a.39.39,0,0,1-.39-.4Zm4.73-.4a.4.4,0,0,1,.4.4V7.91a.4.4,0,0,1-.4.4H9.62a.39.39,0,0,1-.39-.4V6.73a.39.39,0,0,1,.39-.4ZM3.89,2.58A1.58,1.58,0,0,1,5.47,1h6.32a1.58,1.58,0,0,1,1.58,1.58v9.48a1.58,1.58,0,0,1-1.58,1.58H5.47a1.58,1.58,0,0,1-1.58-1.58Zm1.19,0v9.48a.4.4,0,0,0,.39.39h2V10.87a1.19,1.19,0,0,1,2.37,0v1.58h2a.4.4,0,0,0,.4-.39V2.58a.4.4,0,0,0-.4-.4H5.47A.4.4,0,0,0,5.08,2.58Z""/><path d=""M2.63,12.5A2.5,2.5,0,0,0,5.13,15H9.79a.4.4,0,0,0,.4-.4v-.11a.4.4,0,0,0-.4-.4H5.05a1.5,1.5,0,0,1-1.5-1.5V3.73a.4.4,0,0,0-.4-.4H3a.4.4,0,0,0-.4.4Z""/></svg>" )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CAMPUSES )]
    public class CampusesFieldType : SelectFromListFieldType, ICachedEntitiesFieldType, IEntityReferenceFieldType, ISplitMultiValueFieldType
    {
        #region Configuration

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string FILTER_CAMPUS_TYPES_KEY = "filterCampusTypes";
        private const string FILTER_CAMPUS_STATUS_KEY = "filterCampusStatus";
        private const string SELECTABLE_CAMPUSES_KEY = "SelectableCampusIds";
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
                    .Select( c => new CampusFieldType.CampusItemViewModel
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
                // unique identifiers that can be stored in the database.
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

        #endregion

        #region Methods

        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            var allCampuses = CampusCache.All();

            if ( configurationValues == null )
            {
                return allCampuses.ToDictionary( c => c.Guid.ToString(), c => c.Name );
            }

            bool includeInactive = configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
            List<int> campusTypesFilter = configurationValues.ContainsKey( FILTER_CAMPUS_TYPES_KEY ) ? configurationValues[FILTER_CAMPUS_TYPES_KEY].Value.SplitDelimitedValues( false ).AsIntegerList() : null;
            List<int> campusStatusFilter = configurationValues.ContainsKey( FILTER_CAMPUS_STATUS_KEY ) ? configurationValues[FILTER_CAMPUS_STATUS_KEY].Value.SplitDelimitedValues( false ).AsIntegerList() : null;
            List<int> selectableCampuses = configurationValues.ContainsKey( SELECTABLE_CAMPUSES_KEY ) && configurationValues[SELECTABLE_CAMPUSES_KEY].Value.IsNotNullOrWhiteSpace()
                ? configurationValues[SELECTABLE_CAMPUSES_KEY].Value.SplitDelimitedValues( false ).AsIntegerList()
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
            if ( privateValue.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var valueGuidList = privateValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).AsGuidList();

            var ids = valueGuidList
                .Select( guid => CampusCache.GetId( guid ) )
                .Where( id => id.HasValue )
                .ToList();

            var campusEntityTypeId = EntityTypeCache.GetId<Campus>().Value;

            return ids
                .Select( id => new ReferencedEntity( campusEntityTypeId, id.Value ) )
                .ToList();
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

        #region ISplitMultiValueFieldType

        /// <inheritdoc/>
        public ICollection<string> SplitMultipleValues( string privateValue )
        {
            return privateValue.Split( ',' );
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
            Dictionary<string, ConfigurationValue> configurationValues = base.ConfigurationValues( controls );

            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", "When set, inactive campuses will be included in the list.", string.Empty ) );
            configurationValues.Add( FILTER_CAMPUS_TYPES_KEY, new ConfigurationValue( "Filter Campus Types", string.Empty, string.Empty ) );
            configurationValues.Add( FILTER_CAMPUS_STATUS_KEY, new ConfigurationValue( "Filter Campus Status", string.Empty, string.Empty ) );
            configurationValues.Add( SELECTABLE_CAMPUSES_KEY, new ConfigurationValue( "Selectable Campuses", "Specify the campuses eligible for this control. If none are specified then all will be displayed.", string.Empty ) );

            if ( controls != null )
            {
                CheckBox cbIncludeInactive = controls.Count > 2 ? controls[2] as CheckBox : null;
                RockCheckBoxList cblCampusTypes = controls.Count > 3 ? controls[3] as RockCheckBoxList : null;
                RockCheckBoxList cblCampusStatuses = controls.Count > 4 ? controls[4] as RockCheckBoxList : null;
                RockCheckBoxList cblSelectableValues = controls.Count > 5 ? controls[5] as RockCheckBoxList : null;

                configurationValues[INCLUDE_INACTIVE_KEY].Value = cbIncludeInactive != null ? cbIncludeInactive.Checked.ToString() : null;
                configurationValues[FILTER_CAMPUS_TYPES_KEY].Value = cblCampusTypes != null ? string.Join( ",", cblCampusTypes.SelectedValues ) : null;
                configurationValues[FILTER_CAMPUS_STATUS_KEY].Value = cblCampusStatuses != null ? string.Join( ",", cblCampusStatuses.SelectedValues ) : null;

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
            base.SetConfigurationValues( controls, configurationValues );

            if ( controls != null && configurationValues != null )
            {
                CheckBox cbIncludeInactive = controls.Count > 2 ? controls[2] as CheckBox : null;
                RockCheckBoxList cblCampusTypes = controls.Count > 3 ? controls[3] as RockCheckBoxList : null;
                RockCheckBoxList cblCampusStatuses = controls.Count > 4 ? controls[4] as RockCheckBoxList : null;
                RockCheckBoxList cblSelectableValues = controls.Count > 5 ? controls[5] as RockCheckBoxList : null;

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
                    var selectableValues = configurationValues.GetValueOrNull( SELECTABLE_CAMPUSES_KEY )?.SplitDelimitedValues( false );

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

#endif
        #endregion
    }
}
