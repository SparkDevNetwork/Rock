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
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a group and role filtered by a selected group type
    /// Stored as "GroupType.Guid|Group.Guid|GroupTypeRole.Guid"
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.GROUP_AND_ROLE )]
    public class GroupAndRoleFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Configuration

        /// <summary>
        /// Configuration Key for GroupAndRole Picker Label
        /// </summary>
        public static readonly string CONFIG_GROUP_AND_ROLE_PICKER_LABEL = "groupAndRolePickerLabel";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            if ( !TryGetGuidValues( privateValue, out var groupTypeGuid, out var groupGuid, out var groupTypeRoleGuid ) )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                if ( groupGuid.HasValue )
                {
                    var group = new GroupService( rockContext ).GetNoTracking( groupGuid.Value );
                    if ( group != null )
                    {
                        formattedValue = "Group: " + group.Name;
                    }
                }
                else if ( groupTypeGuid.HasValue )
                {
                    var groupType = new GroupTypeService( rockContext ).GetNoTracking( groupTypeGuid.Value );
                    if ( groupType != null )
                    {
                        formattedValue = "Group type: " + groupType.Name;
                    }
                }

                if ( groupTypeRoleGuid.HasValue )
                {
                    var groupTypeRole = new GroupTypeRoleService( rockContext ).GetNoTracking( groupTypeRoleGuid.Value );
                    if ( groupTypeRole != null )
                    {
                        formattedValue += string.IsNullOrEmpty( formattedValue ) ? string.Empty : ", " + "Role: " + groupTypeRole.Name;
                    }
                }
            }

            return formattedValue;
        }

        #endregion

        #region Edit Control

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var groupRoleValue = publicValue.FromJsonOrNull<GroupAndRoleValue>();

            if ( groupRoleValue != null )
            {
                return $"{groupRoleValue.GroupType?.Value}|{groupRoleValue.Group?.Value}|{groupRoleValue.GroupRole?.Value}";
            }

            return base.GetPrivateEditValue( publicValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !TryGetGuidValues( privateValue, out var groupTypeGuid, out var groupGuid, out var groupTypeRoleGuid ) )
            {
                return string.Empty;
            }

            var groupTypeRoles = new List<ListItemBag>();
            if ( groupTypeGuid.HasValue && groupTypeRoleGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    groupTypeRoles = new GroupTypeRoleService( rockContext ).Queryable()
                        .Where( r =>
                            r.GroupType.Guid == groupTypeGuid )
                        .OrderBy( r => r.Name )
                        .Select( r => new ListItemBag { Text = r.Name, Value = r.Guid.ToString() } )
                        .ToList();
                }
            }

            return new GroupAndRoleValue
            {
                Group = groupGuid.HasValue ? new ListItemBag() { Value = groupGuid.ToString() } : null,
                GroupType = groupTypeGuid.HasValue ? new ListItemBag() { Value = groupTypeGuid.ToString() } : null,
                GroupRole = groupTypeRoleGuid.HasValue ? new ListItemBag() { Value = groupTypeRoleGuid.ToString() } : null,
                GroupTypeRoles = groupTypeRoles
            }.ToCamelCaseJson( false, true );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries the get unique identifier values that correspond to the private value.
        /// </summary>
        /// <param name="privateValue">The private value.</param>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <param name="groupGuid">The group unique identifier.</param>
        /// <param name="groupTypeRoleGuid">The group type role unique identifier.</param>
        /// <returns><c>true</c> if one or more of the values was found, <c>false</c> otherwise.</returns>
        private static bool TryGetGuidValues( string privateValue, out Guid? groupTypeGuid, out Guid? groupGuid, out Guid? groupTypeRoleGuid )
        {
            groupTypeGuid = null;
            groupGuid = null;
            groupTypeRoleGuid = null;

            string[] parts = ( privateValue ?? string.Empty ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
            if ( parts.Length > 0 )
            {
                groupTypeGuid = parts[0].AsGuidOrNull();
                if ( parts.Length > 1 )
                {
                    groupGuid = parts[1].AsGuidOrNull();
                }

                if ( parts.Length > 2 )
                {
                    groupTypeRoleGuid = parts[2].AsGuidOrNull();
                }
            }

            return groupTypeGuid.HasValue || groupGuid.HasValue || groupTypeRoleGuid.HasValue;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !TryGetGuidValues( privateValue, out var groupTypeGuid, out var groupGuid, out var groupTypeRoleGuid ) )
            {
                return null;
            }

            var entityReferences = new List<ReferencedEntity>();

            using ( var rockContext = new RockContext() )
            {
                if ( groupGuid.HasValue )
                {
                    var groupId = new GroupService( rockContext ).GetId( groupGuid.Value );

                    if ( groupId.HasValue )
                    {
                        entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<Group>().Value, groupId.Value ) );
                    }
                }
                else if ( groupTypeGuid.HasValue )
                {
                    var groupType = GroupTypeCache.Get( groupTypeGuid.Value );

                    if ( groupType != null )
                    {
                        entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<GroupType>().Value, groupType.Id ) );
                    }
                }

                if ( groupTypeRoleGuid.HasValue )
                {
                    var groupTypeRoleId = new GroupTypeRoleService( rockContext ).GetId( groupTypeRoleGuid.Value );

                    if ( groupTypeRoleId.HasValue )
                    {
                        entityReferences.Add( new ReferencedEntity( EntityTypeCache.GetId<GroupTypeRole>().Value, groupTypeRoleId.Value ) );
                    }
                }
            }

            return entityReferences;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name properties of GroupType,
            // GroupTypeRole and Group and should have its persisted values
            // updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<GroupType>().Value, nameof( GroupType.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<GroupTypeRole>().Value, nameof( GroupTypeRole.Name ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Group>().Value, nameof( Group.Name ) )
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
            List<string> configKeys = new List<string>();
            configKeys.Add( CONFIG_GROUP_AND_ROLE_PICKER_LABEL );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var textBoxGroupAndRolePickerLabel = new RockTextBox();
            controls.Add( textBoxGroupAndRolePickerLabel );
            textBoxGroupAndRolePickerLabel.Label = "Group/Role Picker Label";
            textBoxGroupAndRolePickerLabel.Help = "The label for the group/role picker";
            textBoxGroupAndRolePickerLabel.Text = "Group";

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
            configurationValues.Add( CONFIG_GROUP_AND_ROLE_PICKER_LABEL, new ConfigurationValue( "Group/Role Picker Label", "The label for the group/role picker.", "Group" ) );

            if ( controls != null && controls.Count == 1 )
            {
                var textBoxGroupAndRolePickerLabel = controls[0] as RockTextBox;
                if ( textBoxGroupAndRolePickerLabel != null )
                {
                    configurationValues[CONFIG_GROUP_AND_ROLE_PICKER_LABEL].Value = textBoxGroupAndRolePickerLabel.Text;
                }
            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <param name="configurationValues">The configuration values.</param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                var textBoxGroupAndRolePickerLabel = controls[0] as RockTextBox;
                if ( textBoxGroupAndRolePickerLabel != null && configurationValues?.ContainsKey( CONFIG_GROUP_AND_ROLE_PICKER_LABEL ) == true )
                {
                    textBoxGroupAndRolePickerLabel.Text = configurationValues[CONFIG_GROUP_AND_ROLE_PICKER_LABEL].Value;
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
            GroupAndRolePicker editControl = new GroupAndRolePicker { ID = id };
            if ( configurationValues != null && configurationValues.ContainsKey( CONFIG_GROUP_AND_ROLE_PICKER_LABEL ) )
            {
                editControl.GroupControlLabel = configurationValues[CONFIG_GROUP_AND_ROLE_PICKER_LABEL].Value;
            }

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
            GroupAndRolePicker groupAndRolePicker = control as GroupAndRolePicker;
            if ( groupAndRolePicker != null )
            {
                var rockContext = new RockContext();

                Guid? groupTypeGuid = null;
                Guid? groupGuid = null;
                Guid? groupTypeRoleGuid = null;
                if ( groupAndRolePicker.GroupTypeId.HasValue )
                {
                    var groupType = new GroupTypeService( rockContext ).GetNoTracking( groupAndRolePicker.GroupTypeId.Value );
                    if ( groupType != null )
                    {
                        groupTypeGuid = groupType.Guid;
                    }
                }

                if ( groupAndRolePicker.GroupId.HasValue )
                {
                    var group = new GroupService( rockContext ).Get( groupAndRolePicker.GroupId.Value );
                    if ( group != null )
                    {
                        groupGuid = group.Guid;
                    }
                }

                if ( groupAndRolePicker.GroupRoleId.HasValue )
                {
                    var groupTypeRole = new GroupTypeRoleService( rockContext ).Get( groupAndRolePicker.GroupRoleId.Value );
                    if ( groupTypeRole != null )
                    {
                        groupTypeRoleGuid = groupTypeRole.Guid;
                    }
                }

                if ( groupTypeGuid.HasValue || groupGuid.HasValue || groupTypeRoleGuid.HasValue )
                {
                    return string.Format( "{0}|{1}|{2}", groupTypeGuid, groupGuid, groupTypeRoleGuid );
                }
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
            GroupAndRolePicker groupAndRolePicker = control as GroupAndRolePicker;
            if ( groupAndRolePicker != null )
            {
                // initialize in case the value isn't set
                groupAndRolePicker.GroupTypeId = null;
                groupAndRolePicker.GroupId = null;
                groupAndRolePicker.GroupRoleId = null;

                string[] parts = ( value ?? string.Empty ).Split( '|' );
                var rockContext = new RockContext();
                if ( parts.Length >= 1 )
                {
                    var groupType = new GroupTypeService( rockContext ).Get( parts[0].AsGuid() );
                    if ( groupType != null )
                    {
                        groupAndRolePicker.GroupTypeId = groupType.Id;
                    }

                    if ( parts.Length >= 2 )
                    {
                        var group = new GroupService( rockContext ).Get( parts[1].AsGuid() );
                        if ( group != null )
                        {
                            groupAndRolePicker.GroupId = group.Id;
                        }
                    }

                    if ( parts.Length >= 3 )
                    {
                        var groupTypeRole = new GroupTypeRoleService( rockContext ).Get( parts[2].AsGuid() );
                        if ( groupTypeRole != null )
                        {
                            groupAndRolePicker.GroupRoleId = groupTypeRole.Id;
                        }
                    }
                }
            }
        }


#endif
        #endregion

        private class GroupAndRoleValue
        {
            public ListItemBag GroupType { get; set; }
            public ListItemBag Group { get; set; }
            public ListItemBag GroupRole { get; set; }
            public List<ListItemBag> GroupTypeRoles { get; set; }
        }
    }
}