//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( 0, "Treeview Title", "", "", false, "Group Tree View" )]
    [GroupTypesField( 1, "Group Types", false, "", "", "", "Select group types to show in this block.  Leave all unchecked to show all group types." )]
    [GroupField( 2, "Group", false, "", "", "", "Select the root group to show in this block." )]
    [BooleanField( 3, "Limit to Security Role Groups", false )]
    [DetailPage]
    public partial class GroupTreeView : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            hfLimitToSecurityRoleGroups.Value = GetAttributeValue( "LimittoSecurityRoleGroups" );
            hfRootGroupId.Value = GetAttributeValue( "Group" );

            string[] eventArgs = ( Request.Form["__EVENTARGUMENT"] ?? string.Empty ).Split( new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries );
            if ( eventArgs.Length == 2 )
            {
                if ( eventArgs[0] == "groupId" )
                {
                    groupItem_Click( eventArgs[1] );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the groupItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void groupItem_Click( string groupId )
        {
            NavigateToDetailPage( "groupId", int.Parse( groupId ) );
        }
    }
}