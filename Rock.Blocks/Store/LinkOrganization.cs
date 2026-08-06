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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Rock.Attribute;
using Rock.Configuration;
using Rock.Configuration.ConnectedServices;

namespace Rock.Blocks.Store
{
    /// <summary>
    /// Links a Rock organization to the store.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Link Organization" )]
    [Category( "Store" )]
    [Description( "Links a Rock organization to the store." )]
    [IconCssClass( "ti ti-link" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "363974F0-1A85-40C6-AAAE-36D58A7B7C03" )]
    // WAS [Rock.SystemGuid.BlockTypeGuid( "39F9DD1F-675B-492A-8DD2-E89F7BDA3FB3" )]
    [Rock.SystemGuid.BlockTypeGuid( "41DFED6E-2ECD-4198-80C3-816B27241EB4" )]
    public class LinkOrganization : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ReturnUrl = "ReturnUrl";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// The URL to navigate to after configuration when no ReturnUrl page
        /// parameter was provided.
        /// </summary>
        private const string DefaultContinueUrl = "/RockShop";

        #endregion Constants

        #region Methods

        /// <inheritdoc/>
        public override async Task<object> GetObsidianBlockInitializationAsync()
        {
            var requestId = PageParameter( "request_id" );
            var status = PageParameter( "status" );

            if ( requestId.IsNotNullOrWhiteSpace() && status.IsNotNullOrWhiteSpace() )
            {
                var cts = new CancellationTokenSource( 10_000 );

                return await ProcessCallback( requestId, status, cts.Token );
            }

            return new InitializationBag();
        }

        private async Task<InitializationBag> ProcessCallback( string requestId, string status, CancellationToken cancellationToken )
        {
            if ( status != "success" )
            {
                return new InitializationBag
                {
                    ErrorMessage = "An error occurred trying to link your organization. Please try again in a little while."
                };
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
                    RequestContext.Response.RedirectToUrl( DefaultContinueUrl );
                }

                return new InitializationBag();
            }
            catch ( Exception ex )
            {
                if ( ex is HttpRequestException httpEx && httpEx.InnerException != null )
                {
                    ex = httpEx.InnerException;
                }

                return new InitializationBag
                {
                    ErrorMessage = $"An error occurred trying to contact the remote server. Please try again in a little while. {ex.Message}"
                };
            }
        }

        #endregion Methods

        #region Block Actions

        [BlockAction]
        public async Task<BlockActionResult> StartLink( string callbackUrl )
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
                        var upgradeReturnUrl = PageParameter( PageParameterKey.ReturnUrl );

                        if ( upgradeReturnUrl.IsNotNullOrWhiteSpace() )
                        {
                            return ActionOk( upgradeReturnUrl );
                        }
                        else
                        {
                            return ActionOk( DefaultContinueUrl );
                        }
                    }
                    catch
                    {
                        // Intentionally ignored, fall through to full link flow.
                    }
                }

                var originalReturnUrl = PageParameter( PageParameterKey.ReturnUrl );
                var uri = new UriBuilder( callbackUrl );
                var parameters = uri.Query.ParseQueryString();

                parameters.Remove( PageParameterKey.ReturnUrl );
                parameters.Remove( "request_id" );
                parameters.Remove( "status" );

                uri.Query = parameters.ToString();

                var returnUrl = uri.ToString();
                var cts = new CancellationTokenSource( 5_000 );
                var redirectUrl = await provider.StartLinkOrganizationAsync( returnUrl, originalReturnUrl, cts.Token );

                return ActionOk( redirectUrl );
            }
            catch ( Exception ex )
            {
                if ( ex is HttpRequestException httpEx && httpEx.InnerException != null )
                {
                    ex = httpEx.InnerException;
                }

                return ActionInternalServerError( $"An error occurred trying to contact the remote server. Please try again in a little while. {ex.Message}" );
            }
        }

        #endregion Block Actions

        #region Support Classes

        private class InitializationBag
        {
            public string ErrorMessage { get; set; }
        }

        #endregion
    }
}
