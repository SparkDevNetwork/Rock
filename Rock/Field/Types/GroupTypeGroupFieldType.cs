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
    /// Field Type to select a single (or null) group filtered by a selected group type
    /// Stored as "GroupType.Guid|Group.Guid"
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.GROUP_TYPE_GROUP )]
    public class GroupTypeGroupFieldType : FieldType, IEntityReferenceFieldType
    {
        #region Configuration

        /// <summary>
        /// Configuration Key for Group Picker Label
        /// </summary>
        public static readonly string CONFIG_GROUP_PICKER_LABEL = "groupPickerLabel";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !TryGetValueParts( privateValue, out var groupTypeGuid, out var groupGuid ) )
            {
                return string.Empty;
            }

            using ( var rockContext = new RockContext() )
            {
                if ( groupGuid.HasValue )
                {
                    var groupName = new GroupService( rockContext ).GetSelect( groupGuid.Value, g => g.Name );

                    if ( groupName != null )
                    {
                        return $"Group: {groupName}";
                    }
                }
                else if ( groupTypeGuid.HasValue )
                {
                    var groupTypeName = new GroupTypeService( rockContext ).GetSelect( groupTypeGuid.Value, gt => gt.Name );

                    if ( groupTypeName != null )
                    {
                        return $"Group type: {groupTypeName}";
                    }
                }
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            return GetTextValue( privateValue, privateConfigurationValues );
        }

        /// <inheritdoc/>
        public override string GetPublicEditValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !TryGetValueParts( privateValue, out var groupTypeGuid, out var groupGuid ) )
            {
                return base.GetPublicEditValue( privateValue, privateConfigurationValues );
            }
            var groupType = GroupTypeCache.Get( groupTypeGuid.Value )
                    .ToListItemBag();

            ViewModels.Utility.ListItemBag group = null;
            if ( groupGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    group = new GroupService( rockContext ).Get( groupGuid.Value )
                        .ToListItemBag();
                }
            }
            return new GroupAndGroupType { GroupType = groupType, Group = group }
                .ToCamelCaseJson( false, true );
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var privateValue = base.GetPrivateEditValue( publicValue, privateConfigurationValues );
            var groupAndGroupType = privateValue.FromJsonOrNull<GroupAndGroupType>();
            if ( groupAndGroupType.GroupType == null )
            {
                return string.Empty;
            }
            if ( groupAndGroupType.Group == null )
            {
                return groupAndGroupType.GroupType.Value;
            }
            return $"{groupAndGroupType.GroupType.Value}|{groupAndGroupType.Group.Value}";
        }

        /// <summary>
        /// Tries the get the parts of the database value.
        /// </summary>
        /// <param name="privateValue">The private value.</param>
        /// <param name="groupTypeGuid">On return contains the group type unique identifier.</param>
        /// <param name="groupGuid">On return contains the group unique identifier.</param>
        /// <returns><c>true</c> if either <paramref name="groupTypeGuid"/> or <paramref name="groupGuid"/> are valid, <c>false</c> otherwise.</returns>
        private bool TryGetValueParts( string privateValue, out Guid? groupTypeGuid, out Guid? groupGuid )
        {
            string[] parts = ( privateValue ?? string.Empty ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

            if ( parts.Length > 0 )
            {
                groupTypeGuid = parts[0].AsGuidOrNull();
                groupGuid = parts.Length > 1 ? parts[1].AsGuidOrNull() : null;
            }
            else
            {
                groupTypeGuid = null;
                groupGuid = null;
            }

            return groupTypeGuid.HasValue || groupGuid.HasValue;
        }

        #endregion

        #region Edit Control

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( !TryGetValueParts( privateValue, out var groupTypeGuid, out var groupGuid ) )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                // We only use one of these at a time when formatting the value
                // so we don't need to reference both.
                if ( groupGuid.HasValue )
                {
                    var groupId = new GroupService( rockContext ).GetId( groupGuid.Value );

                    if ( groupId.HasValue )
                    {
                        return new List<ReferencedEntity>
                        {
                            new ReferencedEntity( EntityTypeCache.GetId<Group>().Value, groupId.Value )
                        };
                    }
                }
                else if ( groupTypeGuid.HasValue )
                {
                    var groupTypeId = GroupTypeCache.GetId( groupTypeGuid.Value );

                    if ( groupTypeId.HasValue )
                    {
                        return new List<ReferencedEntity>
                        {
                            new ReferencedEntity( EntityTypeCache.GetId<GroupType>().Value, groupTypeId.Value )
                        };
                    }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the Name property of a Group and
            // the Name property of a GroupType. It should have its
            // persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<GroupType>().Value, nameof( GroupType.Name ) ),
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
            configKeys.Add( CONFIG_GROUP_PICKER_LABEL );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            List<Control> controls = new List<Control>();

            var textBoxGroupPickerLabel = new RockTextBox();
            controls.Add( textBoxGroupPickerLabel );
            textBoxGroupPickerLabel.Label = "Group Picker Label";
            textBoxGroupPickerLabel.Help = "The label for the group picker";
            textBoxGroupPickerLabel.Text = "Group";

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
            configurationValues.Add( CONFIG_GROUP_PICKER_LABEL, new ConfigurationValue( "Group Picker Label", "The label for the group picker.", "Group" ) );

            if ( controls != null && controls.Count == 1 )
            {
                var textBoxGroupPickerLabel = controls[0] as RockTextBox;
                if ( textBoxGroupPickerLabel != null )
                {
                    configurationValues[CONFIG_GROUP_PICKER_LABEL].Value = textBoxGroupPickerLabel.Text;
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
                var textBoxGroupPickerLabel = controls[0] as RockTextBox;
                if ( textBoxGroupPickerLabel != null && configurationValues?.ContainsKey( CONFIG_GROUP_PICKER_LABEL ) == true )
                {
                    textBoxGroupPickerLabel.Text = configurationValues[CONFIG_GROUP_PICKER_LABEL].Value;
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
            GroupTypeGroupPicker editControl = new GroupTypeGroupPicker { ID = id };
            if ( configurationValues != null && configurationValues.ContainsKey( CONFIG_GROUP_PICKER_LABEL ) )
            {
                editControl.GroupControlLabel = configurationValues[CONFIG_GROUP_PICKER_LABEL].Value;
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
            GroupTypeGroupPicker groupTypeGroupPicker = control as GroupTypeGroupPicker;
            if ( groupTypeGroupPicker != null )
            {
                var rockContext = new RockContext();

                Guid? groupTypeGuid = null;
                Guid? groupGuid = null;
                if ( groupTypeGroupPicker.GroupTypeId.HasValue )
                {
                    var groupType = new GroupTypeService( rockContext ).GetNoTracking( groupTypeGroupPicker.GroupTypeId.Value );
                    if ( groupType != null )
                    {
                        groupTypeGuid = groupType.Guid;
                    }
                }

                if ( groupTypeGroupPicker.GroupId.HasValue )
                {
                    var group = new GroupService( rockContext ).GetNoTracking( groupTypeGroupPicker.GroupId.Value );
                    if ( group != null )
                    {
                        groupGuid = group.Guid;
                    }
                }

                if ( groupTypeGuid.HasValue || groupGuid.HasValue )
                {
                    return string.Format( "{0}|{1}", groupTypeGuid, groupGuid );
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
            GroupTypeGroupPicker groupTypeGroupPicker = control as GroupTypeGroupPicker;
            if ( groupTypeGroupPicker != null )
            {
                // initialize in case the value isn't set
                groupTypeGroupPicker.GroupTypeId = null;
                groupTypeGroupPicker.GroupId = null;

                string[] parts = ( value ?? string.Empty ).Split( '|' );
                var rockContext = new RockContext();
                if ( parts.Length >= 1 )
                {
                    var groupType = new GroupTypeService( rockContext ).Get( parts[0].AsGuid() );
                    if ( groupType != null )
                    {
                        groupTypeGroupPicker.GroupTypeId = groupType.Id;
                    }

                    if ( parts.Length >= 2 )
                    {
                        var group = new GroupService( rockContext ).Get( parts[1].AsGuid() );
                        if ( group != null )
                        {
                            groupTypeGroupPicker.GroupId = group.Id;
                        }
                    }
                }
            }
        }

#endif
        #endregion

        /// <summary>
        /// A POCO to store the Group and the Group Type as a ListItemBag
        /// </summary>
        private class GroupAndGroupType
        {
            public ListItemBag GroupType { get; set; }
            public ListItemBag Group { get; set; }
        }
    }
}