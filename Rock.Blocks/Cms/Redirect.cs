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
using System.Net;

using Rock.Attribute;

namespace Rock.Blocks.Cms
{
    [DisplayName( "Redirect" )]
    [Category( "CMS" )]
    [Description( "Redirects the page to the URL provided." )]

    #region Block Attributes

    [TextField(
        "Url",
        Description = "The path to redirect to <span class='tip tip-lava'></span>.",
        Order = 0,
        Key = AttributeKey.Url )]

    [CustomDropdownListField(
        "Redirect When",
        Description = "When the redirect will occur.",
        ListSource = "1^Always,2^When On Provided Network,3^When NOT On Provided Network",
        IsRequired = true,
        DefaultValue = "1",
        Order = 1,
        Key = AttributeKey.RedirectWhen )]

    [TextField(
        "Network",
        Description = "The network to compare to in the format of '192.168.0.0/24'. See http://www.ipaddressguide.com/cidr for assistance in calculating CIDR addresses.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.Network )]

    [BooleanField(
        "Permanent Redirect",
        Description = "If enabled, the redirect will be performed with a 301 status code which will indicate to search engines that this page has permanently moved to the new location. <span class='badge badge-warning'>Do not enable if using Lava.</span>",
        IsRequired = false,
        Order = 3,
        DefaultBooleanValue = false,
        ControlType = Rock.Field.Types.BooleanFieldType.BooleanControlType.Checkbox,
        Key = AttributeKey.PermanentRedirect )]

    #endregion

    // In order for the redirect to take affect, a full page reload is required.
    [ConfigurationChangedReload( Enums.Cms.BlockReloadMode.Page )]

    [Rock.SystemGuid.EntityTypeGuid( "0740a29c-0201-4b36-97a1-707d00da99d3" )]
    [Rock.SystemGuid.BlockTypeGuid( "B97FB779-5D3E-4663-B3B5-3C2C227AE14A" )]
    public class Redirect : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string Url = "Url";
            public const string RedirectWhen = "RedirectWhen";
            public const string Network = "Network";
            public const string PermanentRedirect = "PermanentRedirect";
        }

        #endregion Attribute Keys

        /// <inheritdoc/>
        public override string ObsidianFileUrl => null; // Static HTML content.

        #region Methods

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            var url = GetAttributeValue( AttributeKey.Url );
            var redirectOption = GetAttributeValue( AttributeKey.RedirectWhen ).AsInteger();

            if ( url.IsNullOrWhiteSpace() )
            {
                return "<div class='alert alert-danger'>Missing URL value for redirect!</div>";
            }

            // if always redirect
            if ( redirectOption == 1 )
            {
                return RedirectToUrl( url );
            }

            // check network to determine redirect
            string network = GetAttributeValue( AttributeKey.Network );

            if ( network.IsNullOrWhiteSpace() )
            {
                return "<div class='alert alert-danger'>No network was provided to test against.</div>";
            }

            var userIP = RequestContext.ClientInformation.IpAddress;

            // ClientInformation.IpAddress will return "localhost" for ::1 IPv6
            // loopback. Switch it to an IPv4 address. In the future when IPv6
            // is more widely adopted, this block may need to be updated to
            // properly handle IPv6 addresses and networks.
            if ( userIP == "localhost" )
            {
                userIP = "127.0.0.1";
            }

            var isOnNetwork = IsInRange( userIP, network );

            if ( ( redirectOption == 2 && isOnNetwork ) || ( redirectOption == 3 && !isOnNetwork ) )
            {
                return RedirectToUrl( url );
            }

            return string.Empty;
        }

        /// <summary>
        /// Perform a redirect to the target URL with the appropriate HTTP headers
        /// set. If the person is an Administrator of this block then we display
        /// an alert instead so they don't lock themselves out.
        /// </summary>
        /// <param name="url">The URL to be redirected to. This may contain Lava.</param>
        /// <returns>A string to return as the HTML content.</returns>
        private string RedirectToUrl( string url )
        {
            var mergeFields = RequestContext.GetCommonMergeFields();
            var resolvedUrl = url.ResolveMergeFields( mergeFields );

            resolvedUrl = RequestContext.ResolveRockUrl( resolvedUrl );

            if ( BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
            {
                return $"<div class='alert alert-warning'>If you did not have Administrate permissions on this block, you would have been redirected to here: <a href='{resolvedUrl}'>{resolvedUrl}</a>.</div>";
            }
            else
            {
                var permanent = GetAttributeValue( AttributeKey.PermanentRedirect ).AsBoolean();

                if ( permanent )
                {
                    RequestContext.Response.SetHttpHeader( "Cache-Control", "max-age=300, s-maxage=300" );
                }

                RequestContext.Response.RedirectToUrl( resolvedUrl, permanent );

                return string.Empty;
            }
        }

        // true if ipAddress falls inside the CIDR range, example
        // bool result = IsInRange("10.50.30.7", "10.0.0.0/8");
        private bool IsInRange( string ipAddress, string mask )
        {
            var parts = mask.Split( '/' );

            var ipAddr = BitConverter.ToInt32( IPAddress.Parse( parts[0] ).GetAddressBytes(), 0 );
            var cidrAddr = BitConverter.ToInt32( IPAddress.Parse( ipAddress ).GetAddressBytes(), 0 );
            var cidrMask = IPAddress.HostToNetworkOrder( -1 << ( 32 - int.Parse( parts[1] ) ) );

            return ( ( ipAddr & cidrMask ) == ( cidrAddr & cidrMask ) );
        }

        #endregion
    }
}
