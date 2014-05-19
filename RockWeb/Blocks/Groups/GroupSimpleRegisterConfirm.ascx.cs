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
using System.ComponentModel;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// Block that will update a group member's status to active
    /// </summary>
    [DisplayName( "Group Simple Register Confirm" )]
    [Category( "Groups" )]
    [Description( "Confirmation block that will update a group member's status to active. (Use with Group Simple Register block)." )]

    [TextField("Success Message", "The text to display when a valid group member key is provided", false, "You have been registered.")]
    [TextField("Error Message", "The text to display when a valid group member key is NOT provided", false, "Sorry, there was a problem confirming your registration.  Please try to register again.")]
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
                    var rockContext = new RockContext();
                    var groupMemberService = new GroupMemberService( rockContext );
                    var groupMember = groupMemberService.GetByUrlEncodedKey( PageParameter( "gm" ) );
                    if ( groupMember == null )
                    {
                        ShowError();
                    }
                    else
                    {
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                        rockContext.SaveChanges();

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
