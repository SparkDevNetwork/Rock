//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;

using Rock;
using Rock.Constants;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) GroupType
    /// </summary>
    public class GroupRoleFieldType : FieldType
    {
        private const string GROUP_TYPE_KEY = "grouptype";

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
            Guid guid = Guid.Empty;
            if ( Guid.TryParse( value, out guid ) )
            {
                var groupRole = new Rock.Model.GroupRoleService().Get( guid );
                if ( groupRole != null )
                {
                    return groupRole.Name;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( GROUP_TYPE_KEY );
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
            DropDownList ddl = new DropDownList();
            controls.Add( ddl );
            ddl.AutoPostBack = true;
            ddl.SelectedIndexChanged += OnQualifierUpdated;

            ddl.Items.Add( new ListItem() );

            var groupTypeService = new Rock.Model.GroupTypeService();
            var groupTypes = groupTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            groupTypes.ForEach( g =>
                ddl.Items.Add( new ListItem( g.Name, g.Id.ToString().ToUpper() ) )
            );

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
            configurationValues.Add( GROUP_TYPE_KEY, new ConfigurationValue( "Group Type", "Type of group to select roles from, if left blank any group type's role can be selected", "" ) );

            if ( controls != null && controls.Count == 1 )
            {
                if ( controls[0] != null && controls[0] is DropDownList )
                {
                    configurationValues[GROUP_TYPE_KEY].Value = ( (DropDownList)controls[0] ).SelectedValue;
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
            if ( controls != null && controls.Count == 1 && configurationValues != null )
            {
                if ( controls[0] != null && controls[0] is DropDownList && configurationValues.ContainsKey( GROUP_TYPE_KEY ) )
                {
                    ( (DropDownList)controls[0] ).SelectedValue = configurationValues[GROUP_TYPE_KEY].Value;
                }
            }
        }        
        
        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            GroupRolePicker editControl = new GroupRolePicker { ID = id };

            if ( configurationValues != null && configurationValues.ContainsKey( GROUP_TYPE_KEY ) )
            {
                int groupTypeId = 0;
                if ( Int32.TryParse( configurationValues[GROUP_TYPE_KEY].Value, out groupTypeId ) && groupTypeId > 0 )
                {
                    editControl.GroupTypeId = groupTypeId;
                }
            }

            return editControl;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            GroupRolePicker groupRolePicker = control as GroupRolePicker;
            if ( groupRolePicker != null )
            {
                if ( groupRolePicker.GroupRoleId.HasValue )
                {
                    var groupRole = new Rock.Model.GroupRoleService().Get( groupRolePicker.GroupRoleId.Value );
                    if ( groupRole != null )
                    {
                        return groupRole.Guid.ToString();
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            Guid guid = Guid.Empty;
            if ( Guid.TryParse( value, out guid ) )
            {
                GroupRolePicker groupRolePicker = control as GroupRolePicker;
                if ( groupRolePicker != null )
                {
                    var groupRole = new Rock.Model.GroupRoleService().Get( guid );
                    if ( groupRole != null )
                    {
                        groupRolePicker.GroupRoleId = groupRole.Id;
                    }
                }
            }
        }
    }
}