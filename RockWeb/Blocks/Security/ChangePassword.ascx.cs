// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using Rock;
using Rock.Address;
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

    [TextField( "Invalid Password Caption",
        Key = AttributeKey.InvalidPasswordCaption,
        Description = "",
        IsRequired = false,
        DefaultValue = "The password is not valid.",
        Category = "Captions",
        Order = 0 )]
    [TextField( "Success Caption",
        Key = AttributeKey.SuccessCaption,
        Description = "",
        IsRequired = false,
        DefaultValue = "Your password has been changed",
        Category = "Captions",
        Order = 1 )]
    [TextField( "Change Password Not Supported Caption",
        Key = AttributeKey.ChangePasswordNotSupportedCaption,
        Description = "",
        IsRequired = false,
        DefaultValue = "Changing your password is not supported.",
        Category = "Captions",
        Order = 2 )]
    [BooleanField(
        "Disable Captcha Support",
        Key = AttributeKey.DisableCaptchaSupport,
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        Order = 3 )]
    [Rock.SystemGuid.BlockTypeGuid( "3C12DE99-2D1B-40F2-A9B8-6FE7C2524B37" )]
    public partial class ChangePassword : Rock.Web.UI.RockBlock
    {
        public static class AttributeKey
        {
            public const string InvalidPasswordCaption = "InvalidPasswordCaption";
            public const string SuccessCaption = "SuccessCaption";
            public const string ChangePasswordNotSupportedCaption = "ChangePasswordNotSupportedCaption";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;
            cpCaptcha.Visible = !( GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || !cpCaptcha.IsAvailable );

            if ( CurrentUser == null || ! CurrentUser.IsAuthenticated )
            {
                DisplayMessage( "You must log in before changing your password", NotificationBoxType.Warning );
                pnlChangePassword.Visible = false;
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    if ( PageParameter( "ChangeRequired" ).AsBoolean() )
                    {
                        nbMessage.NotificationBoxType = NotificationBoxType.Info;
                        nbMessage.Text = "Please change your password before continuing.";
                        nbMessage.Visible = true;
                    }

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
            var disableCaptchaSupport = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || !cpCaptcha.IsAvailable;
            if ( !disableCaptchaSupport && !cpCaptcha.IsResponseValid() )
            {
                DisplayMessage( "There was an issue processing your request. Please try again. If the issue persists please contact us.", NotificationBoxType.Warning );
                return;
            }

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

                            if ( !string.IsNullOrWhiteSpace( PageParameter( "ReturnUrl" ) ) )
                            {
                                string redirectUrl = Server.UrlDecode( PageParameter( "ReturnUrl" ) );
                                Response.Redirect( redirectUrl );
                                Context.ApplicationInstance.CompleteRequest();
                            }

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