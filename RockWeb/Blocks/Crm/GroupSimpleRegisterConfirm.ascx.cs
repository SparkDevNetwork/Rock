//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

using System.Collections.Generic;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Block that will update a group member's status to active
    /// </summary>
    #region Block Attributes

    [TextField("Success Message", "The text to display when a valid group member key is provided", false, "You have been registered.")]
    [TextField("Error Message", "The text to display when a valid group member key is NOT provided", false, "Sorry, there was a problem confirming your registration.  Please try to register again.")]

    #endregion

    public partial class GroupSimpleRegisterConfirm : Rock.Web.UI.RockBlock
    {

         #region overridden control methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string groupMemberKey = PageParameter( "gm" );
            var groupMemberService = new GroupMemberService();
            var groupMember = groupMemberService.GetByUrlEncodedKey( PageParameter( "gm" ) );
            if ( groupMember != null )
            {
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                groupMemberService.Save( groupMember, CurrentPersonId );

                nbMessage.NotificationBoxType = NotificationBoxType.Success;
                nbMessage.Title = "Success";
                nbMessage.Text = GetAttributeValue( "SuccessMessage" );
            }
            else
            {
                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "Sorry";
                nbMessage.Text = GetAttributeValue( "ErrorMessage" );
            }
        }

        #endregion

    }
      
}
