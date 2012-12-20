//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Constants;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class PageReference : FieldType
    {
        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues )
        {
            DropDownList dropDownList = new DropDownList();

            PageService pageService = new PageService();
            List<Rock.Model.Page> allPages = pageService.Queryable().ToList();
            dropDownList.Items.Add( new ListItem(None.Text, None.IdValue) );
            foreach ( var page in allPages.OrderBy(a => a.PageSortHash) )
            {
                dropDownList.Items.Add( new ListItem( page.DropDownListText, page.Guid.ToString() ) );
            }

            return dropDownList;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            DropDownList dropDownList = control as DropDownList;
            string result = null;
            
            if ( dropDownList != null )
            {
                result = dropDownList.SelectedValue;
            }

            return result;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            DropDownList dropDownList = control as DropDownList;
            if ( dropDownList != null )
            {
                dropDownList.SetValue( value );
            }
        }
    }
}