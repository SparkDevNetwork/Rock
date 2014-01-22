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

            if ( CurrentUser == null || !CurrentUser.IsAuthenticated )
                DisplayError( "You must login before changing your password" );

            if ( !Page.IsPostBack )
            {
                if ( CurrentUser != null )
                    tbUserName.Text = CurrentUser.UserName;
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
            var userLoginService = new UserLoginService();
            var userLogin = userLoginService.GetByUserName( tbUserName.Text );
            if ( userLogin != null )
            {
                if ( UserLoginService.IsPasswordValid( tbPassword.Text ) )
                {
                    if ( userLoginService.ChangePassword( userLogin, tbOldPassword.Text, tbPassword.Text ) )
                    {
                        userLoginService.Save( userLogin, CurrentPersonId );

                        lSuccess.Text = GetAttributeValue( "SuccessCaption" );
                        pnlEntry.Visible = false;
                        pnlSuccess.Visible = true;
                    }
                    else
                        DisplayError( "InvalidPasswordCaption" );
                }
                else
                {
                    InvalidPassword();
                }
            }
            else
                DisplayError( "InvalidUserNameCaption" );
        }

        #endregion

        #region Methods

        private void InvalidPassword()
        {
            lInvalid.Text = UserLoginService.FriendlyPasswordRules();
            pnlInvalid.Visible = true;
        }

        private void DisplayError( string message )
        {
            lInvalid.Text = GetAttributeValue( message );
            pnlInvalid.Visible = true;
        }

        #endregion

    }
}