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
    /// Field Type used to display a dropdown list of Connection Activity Types.
    /// The selected value is stored as a Guid.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.CONNECTION_ACTIVITY_TYPE )]
    public class ConnectionActivityTypeFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string CONNECTION_TYPE_FILTER = "connectionTypeFilter";

        private const string HELP_TEXT_INCLUDE_INACTIVE = "When set, inactive activity types will be included in the list.";
        private const string HELP_TEXT_CONNECTION_TYPE = "Select a Connection Type to limit selection to a specific connection type. Leave blank to allow selection of activity types from any connection type";

        #endregion

        #region Formatting

        /// <inheritdoc />
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var activityTypeName = new ConnectionActivityTypeService( rockContext ).GetSelect( guid.Value, cat => cat.Name );

                    return activityTypeName ?? string.Empty;
                }
            }

            return string.Empty;
        }

        #endregion

        #region Edit Control

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
            var guid = value.AsGuidOrNull();

            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new ConnectionActivityTypeService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

        #region Persistence

        /// <inheritdoc/>
        public override PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues, IDictionary<string, object> cache )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return new PersistedValues
                {
                    TextValue = string.Empty,
                    CondensedTextValue = string.Empty,
                    HtmlValue = string.Empty,
                    CondensedHtmlValue = string.Empty
                };
            }

            var textValue = GetTextValue( privateValue, privateConfigurationValues );
            var condensedTextValue = textValue.Truncate( CondensedTruncateLength );

            return new PersistedValues
            {
                TextValue = textValue,
                CondensedTextValue = condensedTextValue,
                HtmlValue = textValue,
                CondensedHtmlValue = condensedTextValue
            };
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var opportunityId = new ConnectionActivityTypeService( rockContext ).GetId( guid.Value );

                if ( !opportunityId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>()
                {
                    new ReferencedEntity( EntityTypeCache.GetId<ConnectionActivityType>().Value, opportunityId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<ConnectionActivityType>().Value, nameof( ConnectionActivityType.Name ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys.
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();

            configKeys.Add( INCLUDE_INACTIVE_KEY );
            configKeys.Add( CONNECTION_TYPE_FILTER );

            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field.
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            // Add checkbox for deciding if the list should include inactive items.
            var cbIncludeInactive = new RockCheckBox();
            controls.Add( cbIncludeInactive );
            cbIncludeInactive.AutoPostBack = true;
            cbIncludeInactive.CheckedChanged += OnQualifierUpdated;
            cbIncludeInactive.Label = "Include Inactive";
            cbIncludeInactive.Text = "Yes";
            cbIncludeInactive.Help = HELP_TEXT_INCLUDE_INACTIVE;

            // Add ConnectionType Filter drop-down list.
            var ddlConnectionTypeFilter = new RockDropDownList();
            controls.Add( ddlConnectionTypeFilter );
            ddlConnectionTypeFilter.Label = "Connection Type";
            ddlConnectionTypeFilter.Help = HELP_TEXT_CONNECTION_TYPE;
            ddlConnectionTypeFilter.SelectedIndexChanged += OnQualifierUpdated;
            ddlConnectionTypeFilter.AutoPostBack = true;

            var connectionTypeService = new ConnectionTypeService( new RockContext() );
            ddlConnectionTypeFilter.Items.Add( new ListItem() );
            ddlConnectionTypeFilter.Items.AddRange( connectionTypeService.Queryable().Select( x => new ListItem { Text = x.Name, Value = x.Id.ToString() } ).ToArray() );

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            var configurationValues = new Dictionary<string, ConfigurationValue>();

            configurationValues.Add( INCLUDE_INACTIVE_KEY, new ConfigurationValue( "Include Inactive", HELP_TEXT_INCLUDE_INACTIVE, string.Empty ) );
            configurationValues.Add( CONNECTION_TYPE_FILTER, new ConfigurationValue( "Connection Type", HELP_TEXT_CONNECTION_TYPE, string.Empty ) );

            if ( controls != null && controls.Count() == 2 )
            {
                if ( controls[0] != null && controls[0] is CheckBox )
                {
                    configurationValues[INCLUDE_INACTIVE_KEY].Value = ( ( CheckBox ) controls[0] ).Checked.ToString();
                }

                if ( controls[1] != null && controls[1] is DropDownList )
                {
                    configurationValues[CONNECTION_TYPE_FILTER].Value = ( ( DropDownList ) controls[1] ).SelectedValue;
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
            if ( controls != null && controls.Count() == 2 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is CheckBox && configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) )
                {
                    ( ( CheckBox ) controls[0] ).Checked = configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
                }

                if ( controls[1] != null && controls[1] is DropDownList && configurationValues.ContainsKey( CONNECTION_TYPE_FILTER ) )
                {
                    ( ( DropDownList ) controls[1] ).SelectedValue = configurationValues[CONNECTION_TYPE_FILTER].Value;
                }
            }
        }

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
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

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
            var includeInactive = false;
            int? connectionTypeFilterId = null;

            if ( configurationValues != null )
            {
                includeInactive = configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
                connectionTypeFilterId = configurationValues.ContainsKey( CONNECTION_TYPE_FILTER ) ? configurationValues[CONNECTION_TYPE_FILTER].Value.AsIntegerOrNull() : null;
            }

            var activityTypes = new ConnectionActivityTypeService( new RockContext() )
                .Queryable().AsNoTracking()
                .Where( o => o.IsActive || includeInactive )
                .OrderBy( o => o.ConnectionType.Name )
                .ThenBy( o => o.Name )
                .Select( o => new { o.Guid, o.Name, o.ConnectionType } )
                .ToList();

            var editControl = new RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            if ( activityTypes.Any() )
            {
                foreach ( var activityType in activityTypes )
                {
                    if ( connectionTypeFilterId != null
                         && ( activityType.ConnectionType == null || activityType.ConnectionType.Id != connectionTypeFilterId ) )
                    {
                        continue;
                    }

                    var listItem = new ListItem( activityType.Name, activityType.Guid.ToString().ToUpper() );

                    // Don't add an option group if there is a filter since that would be only one group.
                    if ( connectionTypeFilterId == null
                         && activityType.ConnectionType != null )
                    {
                        listItem.Attributes.Add( "OptionGroup", activityType.ConnectionType.Name );
                    }

                    editControl.Items.Add( listItem );
                }

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
            var picker = control as DropDownList;
            if ( picker != null )
            {
                // picker has value as ConnectionActivityType.Guid
                return picker.SelectedValue;
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
            var editControl = control as ListControl;
            if ( editControl != null )
            {
                var includeInactive = configurationValues.ContainsKey( INCLUDE_INACTIVE_KEY ) && configurationValues[INCLUDE_INACTIVE_KEY].Value.AsBoolean();
                if ( !includeInactive )
                {
                    var listItem = editControl.Items.FindByValue( value );
                    if ( listItem == null )
                    {
                        var valueGuid = value.AsGuid();
                        var activityType = new ConnectionActivityTypeService( new RockContext() )
                           .Queryable().AsNoTracking()
                           .Where( o => o.Guid == valueGuid )
                           .FirstOrDefault();
                        if ( activityType != null )
                        {
                            editControl.Items.Add( new ListItem( activityType.Name, activityType.Guid.ToString().ToUpper() ) );
                        }
                    }
                }

                editControl.SetValue( value );
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
            var guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new ConnectionActivityTypeService( new RockContext() ).Get( guid );

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
            var item = new ConnectionActivityTypeService( new RockContext() ).Get( id ?? 0 );

            var guidValue = item != null ? item.Guid.ToString() : string.Empty;

            SetEditValue( control, configurationValues, guidValue );
        }

#endif
        #endregion
    }
}