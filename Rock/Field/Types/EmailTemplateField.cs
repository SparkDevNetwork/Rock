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

using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select 0 or more GroupTypes 
    /// </summary>
    public class EmailTemplateField : SelectSingle
    {
        /// <summary>
        /// Creates the control(s) neccessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl(Dictionary<string,ConfigurationValue> configurationValues)
        {
            ListControl editControl = new DropDownList();

            var service = new EmailTemplateService();
            foreach ( var emailTemplate in service.Queryable().OrderBy( e => e.Title ) )
            {
                editControl.Items.Add( new ListItem( emailTemplate.Title, emailTemplate.Guid.ToString() ) );
            }

            return editControl;
        }
    }
}