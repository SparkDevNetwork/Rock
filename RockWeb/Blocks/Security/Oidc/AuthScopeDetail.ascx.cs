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

using System;
using System.ComponentModel;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security.Oidc
{
    /// <summary>
    /// Displays the details of the given OpenID Connect Scope.
    /// </summary>
    [DisplayName( "OpenID Connect Scope Detail" )]
    [Category( "Security > OIDC" )]
    [Description( "Displays the details of the given OpenID Connect Scope." )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.OIDC_SCOPE_DETAIL )]
    public partial class AuthScopeDetail : Rock.Web.UI.RockBlock
    {
        private class PageParameterKeys
        {
            /// <summary>
            /// The scope identifier
            /// </summary>
            public const string ScopeId = "ScopeId";
        }

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                var scopeId = PageParameter( PageParameterKeys.ScopeId ).AsIntegerOrNull();
                if ( scopeId == null )
                {
                    DisplayErrorMessage( "No Auth Scope Id was specified." );
                    base.OnLoad( e );
                    return;
                }

                ShowDetail( scopeId.Value );
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var scopeId = PageParameter( PageParameterKeys.ScopeId ).AsIntegerOrNull();
            if ( scopeId == null )
            {
                DisplayErrorMessage( "No Auth Scope Id was specified." );
                return;
            }

            SaveAuthScope( scopeId.Value );
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Displays the error message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayErrorMessage( string message )
        {
            nbWarningMessage.Text = message;
            nbWarningMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
            nbWarningMessage.Visible = true;
            pnlEditDetails.Visible = false;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="authScopeId">The rest user identifier.</param>
        public void ShowDetail( int authScopeId )
        {
            var rockContext = new RockContext();

            AuthScope authScope = null;
            var isNew = authScopeId.Equals( 0 );
            if ( !isNew )
            {
                authScope = new AuthScopeService( rockContext ).Get( authScopeId );
                lTitle.Text = ActionTitle.Edit( "Scope" ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( "Scope" ).FormatAsHtmlTitle();
            }

            if ( authScope == null )
            {
                if ( !isNew )
                {
                    DisplayErrorMessage( "The Auth Scope with the specified Id was found." );
                    return;
                }

                authScope = new AuthScope { Id = 0, IsActive = true };
            }

            hfRestUserId.Value = authScope.Id.ToString();

            tbName.Text = authScope.Name;
            tbPublicName.Text = authScope.PublicName;
            cbActive.Checked = authScope.IsActive;

            nbEditModeMessage.Text = string.Empty;
            if ( authScope.IsSystem )
            {
                tbName.Enabled = false;
                cbActive.Enabled = false;
                nbEditModeMessage.Text = EditModeMessage.System( Rock.Model.AuthScope.FriendlyTypeName );
            }

            var editAllowed = authScope.IsAuthorized( Authorization.EDIT, CurrentPerson );
            lbSave.Visible = editAllowed;
        }

        /// <summary>
        /// Saves the authentication scope.
        /// </summary>
        /// <param name="authScopeId">The authentication scope identifier.</param>
        private void SaveAuthScope( int authScopeId )
        {
            var isNew = authScopeId.Equals( 0 );

            var authScope = new AuthScope();

            var editAllowed = authScope.IsAuthorized( Authorization.EDIT, CurrentPerson );
            if ( !editAllowed )
            {
                DisplayErrorMessage( "The current user is not authorized to make changes." );
                return;
            }

            var rockContext = new RockContext();
            var authScopeService = new AuthScopeService( rockContext );
            if ( isNew )
            {
                authScopeService.Add( authScope );
            }
            else
            {
                authScope = authScopeService.Get( authScopeId );
            }

            if ( authScope == null )
            {
                DisplayErrorMessage( "The Auth Scope with the specified Id was found." );
                return;
            }

            if ( !authScope.IsSystem )
            {
                authScope.Name = tbName.Text;
                authScope.IsActive = cbActive.Checked;
            }

            authScope.PublicName = tbPublicName.Text;

            rockContext.SaveChanges();
        }
        #endregion
    }
}