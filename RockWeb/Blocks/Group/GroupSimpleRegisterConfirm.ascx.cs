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

            try
            {
                string groupMemberKey = PageParameter( "gm" );
                if ( string.IsNullOrWhiteSpace( groupMemberKey ) )
                {
                    ShowError( "Missing Parameter Value" );
                }
                else
                {
                    var groupMemberService = new GroupMemberService();
                    var groupMember = groupMemberService.GetByUrlEncodedKey( PageParameter( "gm" ) );
                    if ( groupMember == null )
                    {
                        ShowError();
                    }
                    else
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                        groupMemberService.Save( groupMember, CurrentPersonId );

                        nbMessage.NotificationBoxType = NotificationBoxType.Success;
                        nbMessage.Title = "Success";
                        nbMessage.Text = GetAttributeValue( "SuccessMessage" );
                    }
                }
            }
            catch (SystemException ex)
            {
                ShowError( ex.Message );
            }
        }

        #endregion

        private void ShowError( string errorDetail = "")
        {
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Title = "Sorry";
            if ( string.IsNullOrWhiteSpace( errorDetail ) )
            {
                nbMessage.Text = GetAttributeValue( "ErrorMessage" );
            }
            else
            {
                nbMessage.Text = string.Format( "{0} [{1}]", GetAttributeValue( "ErrorMessage" ), errorDetail );
            }
        }

    }
      
}
