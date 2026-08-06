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
using Microsoft.Extensions.DependencyInjection;

using Rock;
using Rock.Configuration;
using Rock.Configuration.ConnectedServices;
using Rock.Model;

using System;
using System.ComponentModel;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;

namespace RockWeb.Blocks.Store
{
    /// <summary>
    /// Lists packages that have been purchased in the Rock Store.
    /// </summary>
    [DisplayName( "Link Organization" )]
    [Category( "Store" )]
    [Description( "Links a Rock organization to the store." )]
    [Rock.SystemGuid.BlockTypeGuid( "41DFED6E-2ECD-4198-80C3-816B27241EB4" )]
    public partial class LinkOrganization : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var requestId = PageParameter( "request_id" );
            var status = PageParameter( "status" );

            if ( !IsPostBack && requestId.IsNotNullOrWhiteSpace() && status.IsNotNullOrWhiteSpace() )
            {
                Page.RegisterAsyncTask( new PageAsyncTask( async ( cancellationToken ) =>
                {
                    await ProcessCallback( requestId, status, cancellationToken );
                } ) );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        protected async void btnStart_Click( object sender, EventArgs e )
        {
            try
            {
                var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();

                if ( !provider.IsOrganizationLinked() && provider.IsLegacyOrganizationLinked() )
                {
                    try
                    {
                        var upgradeCts = new CancellationTokenSource( 5_000 );
                        var result = await provider.UpgradeLegacyIdentifierAsync( upgradeCts.Token );
                        var upgradeReturnUrl = PageParameter( "ReturnUrl" );

                        if ( upgradeReturnUrl.IsNotNullOrWhiteSpace() )
                        {
                            RequestContext.Response.RedirectToUrl( upgradeReturnUrl );
                        }
                        else
                        {
                            RequestContext.Response.RedirectToUrl( "/RockShop" );
                        }

                        return;
                    }
                    catch
                    {
                        // Intentionally ignored, fall through to full link flow.
                    }
                }

                var originalReturnUrl = PageParameter( "ReturnUrl" );
                var uri = new UriBuilder( RequestContext.RequestUri );
                var parameters = uri.Query.ParseQueryString();

                parameters.Remove( "ReturnUrl" );
                parameters.Remove( "request_id" );
                parameters.Remove( "status" );

                uri.Query = parameters.ToString();

                var returnUrl = uri.ToString();
                var cts = new CancellationTokenSource( 5_000 );
                var redirectUrl = await provider.StartLinkOrganizationAsync( returnUrl, originalReturnUrl, cts.Token );

                RequestContext.Response.RedirectToUrl( redirectUrl );
            }
            catch ( Exception ex )
            {
                if ( ex is HttpRequestException httpEx && httpEx.InnerException != null )
                {
                    ex = httpEx.InnerException;
                }

                nbStartError.Text = $"<p>An error occurred trying to contact the remote server. Please try again in a little while.</p><p>{ex.Message.EncodeHtml()}</p>";
            }
        }

        protected void btnContinue_Click( object sender, EventArgs e )
        {
            var returnUrl = PageParameter( "ReturnUrl" );
            if ( returnUrl.IsNotNullOrWhiteSpace() )
            {
                Response.Redirect( returnUrl );
            }
            else
            {
                Response.Redirect( "/RockShop" );
            }
        }

        #endregion

        #region Methods

        private async Task ProcessCallback( string requestId, string status, CancellationToken cancellationToken )
        {
            if ( status != "success" )
            {
                nbStartError.Text = $"<p>An error occurred trying to link your organization. Please try again in a little while.</p>";
                return;
            }

            try
            {
                var provider = RockApp.Current.GetRequiredService<ConnectedServicesProvider>();
                var result = await provider.CompleteLinkOrganizationAsync( requestId, cancellationToken );

                if ( result.Context.IsNotNullOrWhiteSpace() )
                {
                    RequestContext.Response.RedirectToUrl( result.Context );
                }
                else
                {
                    RequestContext.Response.RedirectToUrl( "/RockShop" );
                }
            }
            catch ( Exception ex )
            {
                if ( ex is HttpRequestException httpEx && httpEx.InnerException != null )
                {
                    ex = httpEx.InnerException;
                }

                nbStartError.Text = $"<p>An error occurred trying to contact the remote server. Please try again in a little while.</p><p>{ex.Message.EncodeHtml()}</p>";
            }
        }

        #endregion
    }
}