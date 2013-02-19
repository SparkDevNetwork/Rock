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

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class EntityType : FieldType
    {
        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            DropDownList dropDownList = new DropDownList();

            var service = new EntityTypeService();
            var entityTypes = new EntityTypeService().GetEntities();

            dropDownList.Items.Add( new ListItem(None.Text, None.IdValue) );
            foreach ( var entityType in entityTypes.OrderBy( e => e.FriendlyName ).ThenBy( e => e.Name ))
            {
                dropDownList.Items.Add( new ListItem( entityType.FriendlyName, entityType.Id.ToString() ) );
            }

            return dropDownList;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            DropDownList dropDownList = control as DropDownList;
            if ( dropDownList != null )
            {
                return dropDownList.SelectedValue;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            DropDownList dropDownList = control as DropDownList;
            if ( dropDownList != null )
            {
                dropDownList.SetValue( value );
            }
        }
    }
}