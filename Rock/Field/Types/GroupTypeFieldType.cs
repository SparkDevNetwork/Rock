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

using Rock.Constants;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) GroupType
    /// </summary>
    public class GroupTypeFieldType : FieldType
    {
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
                var groupType = new Rock.Model.GroupTypeService().Get( guid );
                if ( groupType != null )
                {
                    return groupType.Name;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editControl = new GroupTypePicker { ID = id }; 
            editControl.GroupTypes = new GroupTypeService().Queryable()
                .OrderBy( a => a.Name ).ToList();
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
            List<string> values = new List<string>();

            GroupTypePicker groupTypePicker = control as GroupTypePicker;
            if ( groupTypePicker != null )
            {
                if ( groupTypePicker.SelectedGroupTypeId.HasValue )
                {
                    Guid groupTypeGuid = new GroupTypeService().Queryable()
                        .Where( g => g.Id == groupTypePicker.SelectedGroupTypeId.Value )
                        .Select( g => g.Guid )
                        .FirstOrDefault();
                    if ( groupTypeGuid != null )
                    {
                        return groupTypeGuid.ToString();
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
            GroupTypePicker dropDownList = control as GroupTypePicker;
            if (dropDownList != null && !string.IsNullOrWhiteSpace(value))
            {
                Guid groupTypeGuid = Guid.Empty;
                if (Guid.TryParse(value, out groupTypeGuid))
                {
                    dropDownList.SelectedGroupTypeId = new GroupTypeService().Queryable()
                        .Where( g => g.Guid.Equals(groupTypeGuid))
                        .Select( g => g.Id )
                        .FirstOrDefault();
                }
            }
        }
    }
}