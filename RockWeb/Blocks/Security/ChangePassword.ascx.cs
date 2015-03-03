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
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for user to change their password.
    /// </summary>
    [DisplayName( "Change Password" )]
    [Category( "Security" )]
    [Description( "Block for a user to change their password." )]

    [TextField( "Invalid Password Caption","", false, "The password is not valid.", "Captions", 0 )]
    [TextField( "Success Caption","", false, "Your password has been changed", "Captions", 1 )]
    [TextField( "Change Password Not Supported Caption", "", false, "Changing your password is not supported.", "Captions", 2 )]
    public partial class ChangePassword : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            if ( CurrentUser == null || ! CurrentUser.IsAuthenticated )
            {
                DisplayMessage( "You must login before changing your password", NotificationBoxType.Warning );
                pnlChangePassword.Visible = false;
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    var component = Rock.Security.AuthenticationContainer.GetComponent( CurrentUser.EntityType.Name );
                    if ( !component.SupportsChangePassword )
                    {
                        DisplayMessage( string.Format( "Changing your password is not supported when logged in using {0}.", component.EntityType.FriendlyName), NotificationBoxType.Warning );
                        pnlChangePassword.Visible = false;
                    }
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnChange control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnChange_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var userLoginService = new UserLoginService( rockContext );
            var userLogin = userLoginService.GetByUserName( CurrentUser.UserName );

            if ( userLogin != null )
            {
                if ( UserLoginService.IsPasswordValid( tbPassword.Text ) )
                {
                    var component = Rock.Security.AuthenticationContainer.GetComponent( userLogin.EntityType.Name );

                    if ( component.SupportsChangePassword )
                    {

                        string warningMessage;
                        if ( component.ChangePassword( userLogin, tbOldPassword.Text, tbPassword.Text, out warningMessage ) )
                        {
                            rockContext.SaveChanges();

                            DisplayMessage( GetAttributeValue( "SuccessCaption" ) , NotificationBoxType.Success );
                            pnlChangePassword.Visible = false;
                        }
                        else
                        {
                            if ( string.IsNullOrWhiteSpace( warningMessage ) )
                            {
                                DisplayErrorFromAttribute( "InvalidPasswordCaption" );
                            }
                            else
                            {
                                DisplayMessage( warningMessage, NotificationBoxType.Danger );
                            }
                        }
                    }
                    else
                    {
                        // shouldn't happen, but just in case
                        DisplayErrorFromAttribute( "ChangePasswordNotSupportedCaption" );
                    }
                }
                else
                {
                    DisplayMessage( UserLoginService.FriendlyPasswordRules(), NotificationBoxType.Danger );
                }
            }
            else
            {
                // shouldn't happen, but just in case
                DisplayErrorFromAttribute( "InvalidUserNameCaption" );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Displays the error from attribute.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        private void DisplayErrorFromAttribute( string messageKey )
        {
            DisplayMessage( GetAttributeValue( messageKey ), NotificationBoxType.Danger );
        }

        /// <summary>
        /// Displays the error text.
        /// </summary>
        /// <param name="messageText">The message text.</param>
        private void DisplayMessage (string messageText, NotificationBoxType noticeType)
        {
            nbMessage.NotificationBoxType = noticeType;
            nbMessage.Text = messageText;
            nbMessage.Visible = true;
        }

        #endregion

    }
}