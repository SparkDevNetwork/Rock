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
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a group and role filtered by a selected group type
    /// Stored as "GroupType.Guid|Group.Guid|GroupTypeRole.Guid"
    /// </summary>
    public class GroupAndRoleFieldType : FieldType
    {
        #region Configuration

        /// <summary>
        /// Configuration Key for GroupAndRole Picker Label
        /// </summary>
        public static readonly string CONFIG_GROUP_AND_ROLE_PICKER_LABEL = "groupAndRolePickerLabel";

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
            configurationValues.Add( CONFIG_GROUP_AND_ROLE_PICKER_LABEL, new ConfigurationValue( "Group/Role Picker Label", "The label for the group/role picker.", string.Empty ) );

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
                if ( textBoxGroupAndRolePickerLabel != null )
                {
                    textBoxGroupAndRolePickerLabel.Text = configurationValues[CONFIG_GROUP_AND_ROLE_PICKER_LABEL].Value;
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

            Guid? groupTypeGuid = null;
            Guid? groupGuid = null;
            Guid? groupTypeRoleGuid = null;

            string[] parts = ( value ?? string.Empty ).Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
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

            var rockContext = new RockContext();
            if ( groupGuid.HasValue )
            {
                var group = new GroupService( rockContext ).Get( groupGuid.Value );
                if ( group != null )
                {
                    formattedValue = "Group: " + group.Name;
                }
            }
            else if ( groupTypeGuid.HasValue )
            {
                var groupType = new GroupTypeService( rockContext ).Get( groupTypeGuid.Value );
                if ( groupType != null )
                {
                    formattedValue = "Group type: " + groupType.Name;
                }
            }

            if ( groupTypeRoleGuid.HasValue )
            {
                var groupTypeRole = new GroupTypeRoleService( rockContext ).Get( groupTypeRoleGuid.Value );
                if ( groupTypeRole != null )
                {
                    formattedValue += string.IsNullOrEmpty( formattedValue ) ? string.Empty : ", " + "Role: " + groupTypeRole.Name;
                }
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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            GroupAndRolePicker editControl = new GroupAndRolePicker { ID = id };
            if ( configurationValues != null )
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
                    var groupType = new GroupTypeService( rockContext ).Get( groupAndRolePicker.GroupTypeId.Value );
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

                return string.Format( "{0}|{1}|{2}", groupTypeGuid, groupGuid, groupTypeRoleGuid );
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

        #endregion
    }
}