// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Security.ExternalAuthentication;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// User login using SMS for authentication.
    /// </summary>
    [DisplayName( "SMS Login" )]
    [Category( "Security" )]
    [Description( "User login using SMS for authentication." )]

    [CodeEditorField( "Prompt Message", "Message to show before logging in.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, true, @"
Please enter your cell phone number and we will text you a code to log in with. <br /><i>Text and data rates may apply</i>.
", "", 0 )]
    [LinkedPage( "Resolve Number Page", "Page to resolve duplicate or non-existant mobile numbers.", true, "", "", 1 )]
    [CodeEditorField( "Resolve Message", "Message to show if a single mobile number could not be located.", CodeEditorMode.Html, CodeEditorTheme.Rock, 100, true, defaultValue: @"
We are sorry, but we could not determine which account this number belongs to. 
        ", order: 2 )]

    [LinkedPage( "Redirect Page", "Page to redirect user to upon successful login.", true, "", "", 3 )]
    public partial class SMSLogin : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( !Page.IsPostBack )
                {
                    lbPrompt.Text = GetAttributeValue( "PromptMessage" );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the click event of the btnGenerate control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnGenerate_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid )
            {
                return;
            }

            pnlPhoneNumber.Visible = false;

            var smsAuthentication = new SMSAuthentication();
            var success = smsAuthentication.SendSMSAuthentication( GetPhoneNumber() );

            if ( success )
            {
                pnlCode.Visible = true;
            }
            else
            {
                lbResolve.Text = GetAttributeValue( "ResolveMessage" );
                pnlResolve.Visible = true;
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "ResolveNumberPage" ) ) )
                {
                    btnResolve.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event for the btnLogin control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnLogin_Click( object sender, EventArgs e )
        {
            nbError.Visible = false;
            if ( Page.IsValid )
            {
                RockContext rockContext = new RockContext();
                var smsAuthentication = new SMSAuthentication();
                string error;
                var person = smsAuthentication.GetNumberOwner( GetPhoneNumber(), rockContext, out error );
                if ( person == null )
                {
                    nbError.Text = error;
                    nbError.Visible = true;
                    return;
                }

                var userLoginService = new UserLoginService( rockContext );
                var userLogin = userLoginService.GetByUserName( "SMS_" + person.Id.ToString() );
                if ( userLogin != null && userLogin.EntityType != null )
                {
                    if ( smsAuthentication.Authenticate( userLogin, tbCode.Text ) )
                    {
                        CheckUser( userLogin, Request.QueryString["returnurl"], true );
                        return;
                    }
                }
            }
            nbError.Text = "Sorry, the code you entered did not match the code we generated.";
            nbError.Visible = true;
        }

        /// <summary>
        /// Handles the Click event for the btnResolve control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnResolve_Click( object sender, EventArgs e )
        {
            NavigateToPage( GetAttributeValue( "ResolveNumberPage" ).AsGuid(), new Dictionary<string, string>() { { "MobilePhoneNumber", tbPhoneNumber.Text } } );
        }

        /// <summary>
        /// Handles the Click event for the btnCancel control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlResolve.Visible = false;
            pnlPhoneNumber.Visible = true;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Parses the formatted phonenumber into just numbers
        /// </summary>
        /// <returns></returns>
        private string GetPhoneNumber()
        {
            return Regex.Replace( tbPhoneNumber.Text, @"^(\+)|\D", "$1" );
        }

        /// <summary>
        /// Checks to see if the user can be authenticated
        /// </summary>
        /// <param name="userLogin"></param>
        /// <param name="returnUrl"></param>
        /// <param name="rememberMe"></param>
        private void CheckUser( UserLogin userLogin, string returnUrl, bool rememberMe )
        {
            if ( userLogin != null )
            {
                if ( ( userLogin.IsConfirmed ?? true ) && !( userLogin.IsLockedOut ?? false ) )
                {
                    LoginUser( userLogin.UserName, returnUrl, rememberMe );
                }
                else
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );

                    if ( userLogin.FailedPasswordAttemptCount > 5 )
                    {
                        pnlCode.Visible = false;
                        pnlPhoneNumber.Visible = true;
                    }
                    else
                    {
                        nbError.Visible = true;
                    }
                }
            }
        }


        /// <summary>
        /// Logs in the authenticated user
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="returnUrl"></param>
        /// <param name="rememberMe"></param>
        private void LoginUser( string userName, string returnUrl, bool rememberMe )
        {
            string redirectUrlSetting = LinkedPageUrl( "RedirectPage" );

            UserLoginService.UpdateLastLogin( userName );

            Authorization.SetAuthCookie( userName, rememberMe, false );

            if ( !string.IsNullOrWhiteSpace( returnUrl ) )
            {
                string redirectUrl = Server.UrlDecode( returnUrl );
                Response.Redirect( redirectUrl );
                Context.ApplicationInstance.CompleteRequest();
            }
            else if ( !string.IsNullOrWhiteSpace( redirectUrlSetting ) )
            {
                Response.Redirect( redirectUrlSetting );
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                RockPage.Layout.Site.RedirectToDefaultPage();
            }
        }
        #endregion
    }
}