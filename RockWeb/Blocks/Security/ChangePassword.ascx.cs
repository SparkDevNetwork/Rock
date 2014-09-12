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

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for user to change their password.
    /// </summary>
    [DisplayName( "Change Password" )]
    [Category( "Security" )]
    [Description( "Block for a user to change their password." )]

    [TextField( "Invalid UserName Caption", "", false, "The User Name/Password combination is not valid.", "Captions", 0 )]
    [TextField( "Invalid Password Caption","", false, "The User Name/Password combination is not valid.", "Captions", 1 )]
    [TextField( "Success Caption","", false, "Your password has been changed", "Captions", 2 )]
    [TextField( "Change Password Not Supported Caption", "", false, "Changing your password is not supported.", "Captions", 3 )]
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

            if ( CurrentUser == null || ! CurrentUser.IsAuthenticated )
            {
                DisplayErrorText( "You must login before changing your password" );
                pnlChangePassword.Visible = false;
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    var component = Rock.Security.AuthenticationContainer.GetComponent( CurrentUser.EntityType.Name );
                    if ( component.SupportsChangePassword )
                    {
                        tbUserName.Text = CurrentUser.UserName;
                    }
                    else
                    {
                        DisplayErrorFromAttribute( "ChangePasswordNotSupportedCaption" );
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
            var userLogin = userLoginService.GetByUserName( tbUserName.Text );

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

                            lSuccess.Text = GetAttributeValue( "SuccessCaption" );
                            pnlEntry.Visible = false;
                            pnlSuccess.Visible = true;
                        }
                        else
                        {
                            if ( string.IsNullOrWhiteSpace( warningMessage ) )
                            {
                                DisplayErrorFromAttribute( "InvalidPasswordCaption" );
                            }
                            else
                            {
                                DisplayErrorText( warningMessage );
                            }
                        }
                    }
                    else
                    {
                        // shouldn't happen, but just in case
                        DisplayErrorFromAttribute( "ChangePasswordNotSupportedCaption" );
                        pnlChangePassword.Visible = false;
                    }
                }
                else
                {
                    InvalidPassword();
                }
            }
            else
            {
                DisplayErrorFromAttribute( "InvalidUserNameCaption" );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invalids the password.
        /// </summary>
        private void InvalidPassword()
        {
            nbPasswordMessage.Text = UserLoginService.FriendlyPasswordRules();
            nbPasswordMessage.Visible = true;
        }

        /// <summary>
        /// Displays the error from attribute.
        /// </summary>
        /// <param name="messageKey">The message key.</param>
        private void DisplayErrorFromAttribute( string messageKey )
        {
            DisplayErrorText( GetAttributeValue( messageKey ) );
        }

        /// <summary>
        /// Displays the error text.
        /// </summary>
        /// <param name="messageText">The message text.</param>
        private void DisplayErrorText (string messageText)
        {
            nbPasswordMessage.Text = messageText;
            nbPasswordMessage.Visible = true;
        }

        #endregion

    }
}