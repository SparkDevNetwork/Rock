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
    /// Field Type to select a single (or null) GroupType
    /// Stored as GroupType.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.GROUP_TYPE )]
    public class GroupTypeFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string GROUP_TYPE_PURPOSE_VALUE_GUID = "groupTypePurposeValueGuid";
        private const string GROUP_TYPES_PURPOSES = "groupTypePurposes";
        private const string VALUES = "values";

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfigurationValues( Dictionary<string, string> privateConfigurationValues, ConfigurationValueUsage usage, string value )
        {
            var publicEditConfigurationValues = new Dictionary<string, string>();

            // if the block is in view mode, merely return the Group Type as ListBagItem to be viewed so that the remote device can display it accordingly.
            if ( usage == ConfigurationValueUsage.View )
            {
                publicEditConfigurationValues[VALUES] = GroupTypeCache.All()
                    .Where( g => g.Guid == value.AsGuid() )
                    .ToListItemBagList()
                    .ToCamelCaseJson( false, true );
                return publicEditConfigurationValues;
            }

            publicEditConfigurationValues = base.GetPublicConfigurationValues( privateConfigurationValues, usage, value );

            using ( var rockContext = new RockContext() )
            {
                // Disable security since we are intentionally returning items even
                // if the person (whom we don't know) doesn't have access.
                var definedValueClientService = new Rock.ClientService.Core.DefinedValue.DefinedValueClientService( rockContext, null )
                {
                    EnableSecurity = false
                };
                var groupTypePurposeValueGuid = privateConfigurationValues.GetValueOrNull( GROUP_TYPE_PURPOSE_VALUE_GUID )?.AsGuid();
                publicEditConfigurationValues[GROUP_TYPES_PURPOSES] = definedValueClientService
                        .GetDefinedValuesAsListItems( SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid(),
                            new ClientService.Core.DefinedValue.Options.DefinedValueOptions { UseDescription = true } )
                        .ToCamelCaseJson( false, true );

                if ( groupTypePurposeValueGuid != Guid.Empty )
                {
                    publicEditConfigurationValues[VALUES] = GroupTypeCache.All()
                        .Where( g => g.GroupTypePurposeValue?.Guid == groupTypePurposeValueGuid )
                        .OrderBy( g => g.Name )
                        .ToListItemBagList()
                        .ToCamelCaseJson( false, true );
                }
                else
                {
                    // show all the groups if there is no Group Type Purpose is set
                    publicEditConfigurationValues[VALUES] = GroupTypeCache.All()
                        .OrderBy( g => g.Name )
                        .ToListItemBagList()
                        .ToCamelCaseJson( false, true );
                }
            }
            return publicEditConfigurationValues;
        }

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            Guid guid = Guid.Empty;
            if ( Guid.TryParse( privateValue, out guid ) )
            {
                var groupType = GroupTypeCache.Get( guid );
                if ( groupType != null )
                {
                    formattedValue = groupType.Name;
                }
            }

            return formattedValue;
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
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new GroupTypeService( rockContext ).Get( guid.Value );
            }

            return null;
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
                var groupTypeId = new GroupTypeService( rockContext ).GetId( guid.Value );

                if ( !groupTypeId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<GroupType>().Value, groupTypeId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Group Type and
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<GroupType>().Value, nameof( GroupType.Name ) )
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Configurations the keys.
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            List<string> configKeys = new List<string>();
            configKeys.Add( GROUP_TYPE_PURPOSE_VALUE_GUID );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var dvpGroupTypePurpose = new DefinedValuePicker();
            dvpGroupTypePurpose.DisplayDescriptions = true;
            controls.Add( dvpGroupTypePurpose );
            dvpGroupTypePurpose.AutoPostBack = true;
            dvpGroupTypePurpose.SelectedIndexChanged += OnQualifierUpdated;

            var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid() );
            dvpGroupTypePurpose.DefinedTypeId = definedType?.Id;
            dvpGroupTypePurpose.Label = "Purpose";
            dvpGroupTypePurpose.Help = "An optional setting to limit the selection of group types to those that have the selected purpose.";

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
            configurationValues.Add( GROUP_TYPE_PURPOSE_VALUE_GUID, new ConfigurationValue( "Purpose", "An optional setting to limit the selection of group types to those that have the selected purpose.", string.Empty ) );

            if ( controls != null && controls.Count == 1 &&
                controls[0] != null && controls[0] is DefinedValuePicker )
            {
                int? definedValueId = ( ( DefinedValuePicker ) controls[0] ).SelectedValueAsInt();
                if ( definedValueId.HasValue )
                {
                    var definedValue = DefinedValueCache.Get( definedValueId.Value );
                    if ( definedValue != null )
                    {
                        configurationValues[GROUP_TYPE_PURPOSE_VALUE_GUID].Value = definedValue.Guid.ToString();
                    }
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
            if ( controls != null && controls.Count == 1 && configurationValues != null &&
                controls[0] != null && controls[0] is DefinedValuePicker && configurationValues.ContainsKey( GROUP_TYPE_PURPOSE_VALUE_GUID ) )
            {
                Guid? definedValueGuid = configurationValues[GROUP_TYPE_PURPOSE_VALUE_GUID].Value.AsGuidOrNull();
                if ( definedValueGuid.HasValue )
                {
                    var definedValue = DefinedValueCache.Get( definedValueGuid.Value );
                    if ( definedValue != null )
                    {
                        ( ( DefinedValuePicker ) controls[0] ).SetValue( definedValue.Id.ToString() );
                    }
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editControl = new GroupTypePicker { ID = id };
            editControl.EnhanceForLongLists = true;
            var qryGroupTypes = new GroupTypeService( new RockContext() ).Queryable();

            if ( configurationValues.ContainsKey( GROUP_TYPE_PURPOSE_VALUE_GUID ) )
            {
                var groupTypePurposeValueGuid = ( configurationValues[GROUP_TYPE_PURPOSE_VALUE_GUID] ).Value.AsGuidOrNull();
                if ( groupTypePurposeValueGuid.HasValue )
                {
                    qryGroupTypes = qryGroupTypes.Where( a => a.GroupTypePurposeValue.Guid == groupTypePurposeValueGuid.Value );
                }
            }

            editControl.IsSortedByName = true;
            editControl.GroupTypes = qryGroupTypes.ToList();
            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field ( as Guid )
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            List<string> values = new List<string>();

            GroupTypePicker groupTypePicker = control as GroupTypePicker;
            if ( groupTypePicker != null )
            {
                if ( groupTypePicker.SelectedGroupTypeId.HasValue )
                {
                    var groupType = GroupTypeCache.Get( groupTypePicker.SelectedGroupTypeId.Value );
                    if ( groupType != null )
                    {
                        return groupType.Guid.ToString();
                    }
                }

                return string.Empty;
            }

            return null;
        }

        /// <summary>
        /// Sets the value. ( as Guid )
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            GroupTypePicker groupTypePicker = control as GroupTypePicker;
            if ( groupTypePicker != null )
            {
                Guid? groupTypeGuid = value.AsGuidOrNull();
                GroupTypeCache groupType = null;
                if ( groupTypeGuid.HasValue )
                {
                    groupType = GroupTypeCache.Get( groupTypeGuid.Value );
                }

                groupTypePicker.SelectedGroupTypeId = groupType?.Id;
            }
        }

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new GroupTypeService( new RockContext() ).Get( guid );
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
            var item = new GroupTypeService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }


#endif
        #endregion

    }
}