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
using Rock.Model;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Power Bi Account Register" )]
    [Category( "Reporting" )]
    [Description( "This block registers a Power BI account for Rock to use." )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.POWERBI_ACCOUNT_REGISTRATION )]
    public partial class PowerBiAccountRegister : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                // check if PowerBI is making a call back
                if ( Request["Authenticated"] != null )
                {
                    pnlEntry.Visible = false;
                    pnlResponse.Visible = true;

                    if ( Request["Authenticated"] == "True" )
                    {

                        nbResponse.NotificationBoxType = NotificationBoxType.Success;
                        nbResponse.Text = "The Power BI account has been successfully created. You can manage all Power BI accounts under 'Admin Tools > General Settings > Defined Types > Power BI Accounts'.";
                    }
                    else
                    {
                        nbResponse.NotificationBoxType = NotificationBoxType.Danger;
                        nbResponse.Text = "Authentication Failed.";
                    }
                }
                else
                {
                    pnlEntry.Visible = true;
                    pnlResponse.Visible = false;

                    var globalAttributes = GlobalAttributesCache.Get();
                    var externalUrl = globalAttributes.GetValue( "InternalApplicationRoot" );

                    if ( !externalUrl.EndsWith( @"/" ) )
                    {
                        externalUrl += @"/";
                    }

                    var redirectUrl = externalUrl + "Webhooks/PowerBiAuth.ashx";

                    lRedirectUrl.Text = redirectUrl;
                    lHomepage.Text = externalUrl;
                    txtRedirectUrl.Text = redirectUrl;
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnRegister control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRegister_Click( object sender, EventArgs e )
        {
            // Authenticate
            PowerBiUtilities.CreateAccount( txtAccountName.Text, txtAccountDescription.Text, txtClientId.Text, txtClientSecret.Text, txtRedirectUrl.Text, Request.UrlProxySafe().AbsoluteUri );
        }

        #endregion
    }
}