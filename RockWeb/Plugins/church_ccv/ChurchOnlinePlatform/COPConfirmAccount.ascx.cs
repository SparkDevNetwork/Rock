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
using System.Web.Security;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using church.ccv.Authentication;

namespace RockWeb.Plugins.church_ccv.COP
{
    /// <summary>
    /// Block for user to confirm a newly created login account.
    /// </summary>
    [DisplayName( "Confirm Account" )]
    [Category( "CCV > Church Online Platform" )]
    [Description( "Block for user to confirm a newly created login account, usually from an email that was sent to them." )]

    [CodeEditorField( "Confirmed Caption", "", Rock.Web.UI.Controls.CodeEditorMode.Html, Rock.Web.UI.Controls.CodeEditorTheme.Rock, 200, false, "{0}, Your account has been confirmed.  Thank you for creating the account", "Captions", 0 )]
    public partial class ConfirmAccount : Rock.Web.UI.RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the confirmation code.
        /// </summary>
        /// <value>
        /// The confirmation code.
        /// </value>
        protected string ConfirmationCode
        {
            get
            {
                string confirmationCode = ViewState["ConfirmationCode"] as string;
                return confirmationCode ?? string.Empty;
            }

            set
            {
                ViewState["ConfirmationCode"] = value;
            }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            pnlCode.Visible = false;
            pnlInvalid.Visible = false;

            if (!Page.IsPostBack)
            {
                ConfirmationCode = Request.QueryString["cc"];
                
                ShowConfirmed();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnCodeConfirm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnCodeConfirm_Click( object sender, EventArgs e )
        {
            ConfirmationCode = tbConfirmationCode.Text;
            ShowConfirmed();
        }
        
        #endregion

        #region Methods
        
        /// <summary>
        /// Shows the confirmed.
        /// </summary>
        private void ShowConfirmed()
        {
            if( !string.IsNullOrWhiteSpace( this.ConfirmationCode ) )
            {
                RockContext rockContext = new RockContext();
                UserLogin user = null;

                try
                {
                    user = new UserLoginService( rockContext ).GetByConfirmationCode( this.ConfirmationCode );
                }
                catch
                {
                }

                if ( user != null )
                {
                    user.IsConfirmed = true;
                    rockContext.SaveChanges( );

                    // they are confirmed--now try logging them in to church online & us

                    string chOPUrl = ChurchOnlinePlatform.CreateSSOUrlChOP( user.Person );
                    if ( !chOPUrl.IsNullOrWhiteSpace( ) )
                    {
                        Rock.Security.Authorization.SetAuthCookie( user.UserName, false, false );

                        // Redirect to return URL and end processing
                        Response.Redirect( chOPUrl, false );
                        Context.ApplicationInstance.CompleteRequest( );
                    }
                    else
                    {
                        pnlCode.Visible = true;
                        pnlInvalid.Visible = true;
                        lInvalid.Text = "Church Online Platform login failed.";
                    }
                }
                else
                {
                    pnlCode.Visible = true;
                    pnlInvalid.Visible = true;
                    lInvalid.Text = "Please check your confirmation code and try again.";
                }
            }
            else
            {
                // no confirmation code is present, so display the entry form
                pnlCode.Visible = true;
            }
        }
        #endregion
    }
}