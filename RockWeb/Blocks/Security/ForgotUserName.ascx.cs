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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for user to request a forgotten username.
    /// </summary>
    [DisplayName( "Forgot Username" )]
    [Category( "Security" )]
    [Description( "Allows a user to get their forgotten username information emailed to them." )]

    [CodeEditorField( "Heading Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "<div class='alert alert-info'>Enter your email address below and we'll send your account information to you right away.</div>", "Captions", 0 )]
    [CodeEditorField( "Invalid Email Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "Sorry, we could not find an account for the email address you entered.", "Captions", 1 )]
    [CodeEditorField( "Success Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "Your user name has been sent with instructions on how to change your password if needed.", "Captions", 2 )]

    
    [LinkedPage( "Confirmation Page", "Page for user to confirm their account (if blank will use 'ConfirmAccount' page route)", true, "", "", 3 )]
    [SystemEmailField( "Forgot Username Email Template", "Email Template to send", false, Rock.SystemGuid.SystemEmail.SECURITY_FORGOT_USERNAME, "", 4, "EmailTemplate" )]
    public partial class ForgotUserName : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlEntry.Visible = true;
            pnlWarning.Visible = false;
            pnlSuccess.Visible = false;

            if ( !Page.IsPostBack )
            {
                lCaption.Text = GetAttributeValue( "HeadingCaption" );
                lWarning.Text = GetAttributeValue( "InvalidEmailCaption" );
                lSuccess.Text = GetAttributeValue( "SuccessCaption" );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {

            var url = LinkedPageUrl( "ConfirmationPage" );
            if ( string.IsNullOrWhiteSpace( url ) )
            {
                url = ResolveRockUrl( "~/ConfirmAccount" );
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "ConfirmAccountUrl", RootPath + url.TrimStart( new char[] { '/' } ) );
            var results = new List<IDictionary<string, object>>();

            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var userLoginService = new UserLoginService( rockContext );

            bool hasAccountWithPasswordResetAbility = false;
            List<string> accountTypes = new List<string>();

            foreach ( Person person in personService.GetByEmail( tbEmail.Text )
                .Where( p => p.Users.Any()))
            {
                var users = new List<UserLogin>();
                foreach ( UserLogin user in userLoginService.GetByPersonId( person.Id ) )
                {
                    if ( user.EntityType != null )
                    {
                        var component = AuthenticationContainer.GetComponent( user.EntityType.Name );
                        if ( component != null && !component.RequiresRemoteAuthentication )
                        {
                            users.Add( user );
                            hasAccountWithPasswordResetAbility = true;
                        }

                        accountTypes.Add( user.EntityType.FriendlyName );
                    }
                }

                var resultsDictionary = new Dictionary<string, object>();
                resultsDictionary.Add( "Person", person);
                resultsDictionary.Add( "Users", users );
                results.Add( resultsDictionary );
            }

            if ( results.Count > 0 && hasAccountWithPasswordResetAbility )
            {
                mergeFields.Add( "Results", results.ToArray() );

                var emailMessage = new RockEmailMessage( GetAttributeValue( "EmailTemplate" ).AsGuid() );
                emailMessage.AddRecipient( new RecipientData( tbEmail.Text, mergeFields ) );
                emailMessage.AppRoot = ResolveRockUrlIncludeRoot( "~/" );
                emailMessage.ThemeRoot = ResolveRockUrlIncludeRoot( "~~/" );
                emailMessage.CreateCommunicationRecord = false;
                emailMessage.Send();

                pnlEntry.Visible = false;
                pnlSuccess.Visible = true;
            }
            else if (results.Count > 0 )
            {
                // the person has user accounts but none of them are allowed to have their passwords reset (Facebook/Google/etc)
                
                lWarning.Text = string.Format( @"<p>We were able to find the following accounts for this email, but 
                                                none of them are able to be reset from this website.</p> <p>Accounts:<br /> {0}</p>
                                                <p>To create a new account with a username and password please see our <a href='{1}'>New Account</a>
                                                page.</p>"
                                    , string.Join( ",", accountTypes )
                                    , ResolveRockUrl( "~/NewAccount" ) );
                pnlWarning.Visible = true;
            }
            else
            {
                pnlWarning.Visible = true;
            }
        }

        #endregion
    }
}