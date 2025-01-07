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
using System.Web;
using Owin;
using Owin.Security.OpenIdConnect.Server;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Security.Oidc
{
    /// <summary>
    /// Logout the user from their auth client.
    /// </summary>
    [DisplayName( "Logout" )]
    [Category( "Security > OIDC" )]
    [Description( "Logout the user from their auth client." )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.OIDC_LOGOUT )]
    public partial class Logout : RockBlock
    {
        #region Base Control Methods
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            LogoutUser();
            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ShowError( string message )
        {
            nbNotificationBox.Text = message;
            nbNotificationBox.Visible = true;
        }

        #endregion Methods

        #region Private Methods
        private void LogoutUser()
        {
            var context = Context.GetOwinContext();
            var response = context.GetOpenIdConnectResponse();
            if ( response == null )
            {
                context.Authentication.SignOut( OpenIdConnectServerDefaults.AuthenticationType );
                Authorization.SignOut();
            }
            else if ( !string.IsNullOrWhiteSpace( response.Error ) )
            {
                throw new Exception( response.ErrorDescription );
            }
        }
        #endregion Events
    }
}