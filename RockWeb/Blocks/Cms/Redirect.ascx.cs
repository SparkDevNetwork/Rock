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
using Rock.Attribute;
using System.ComponentModel;
using Rock.Security;
using Rock;
using System.Net;

namespace RockWeb.Blocks.Cms
{
    [DisplayName("Redirect")]
    [Category("CMS")]
    [Description("Redirects the page to the URL provided.")]
    [TextField( "Url", "The path to redirect to <span class='tip tip-lava'></span>.", order:0 )]
    [CustomDropdownListField("Redirect When", "When the redirect will occur.", "1^Always,2^When On Provided Network,3^When NOT On Provided Network", true, "1", order:1)]
    [TextField("Network", "The network to compare to in the format of '192.168.0.0/24'. See http://www.ipaddressguide.com/cidr for assistance in calculating CIDR addresses.", false, "", order:2)]
    public partial class Redirect : Rock.Web.UI.RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if (!Page.IsPostBack)
            {
                RefreshContent();
            }
        }

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RefreshContent();
        }
        
        private void RefreshContent()
        {
            string url = GetAttributeValue( "Url" );
            int redirectOption = GetAttributeValue( "RedirectWhen" ).AsInteger();
            

            if ( !string.IsNullOrEmpty( url ) )
            {
                // if always redirect 
                if (redirectOption == 1 )
                {
                    RedirectToUrl( url );
                    return;
                }

                // check network to determine redirect
                string network = GetAttributeValue( "Network" );

                if ( network.IsNullOrWhiteSpace() )
                {
                    nbAlert.Text = "No network was provided to test against.";
                }

                var userIP = Request.UserHostAddress;

                if (userIP == "::1" )
                {
                    userIP = "127.0.0.1";
                }

                var isOnNetwork = IsInRange( userIP, network );

                if ((redirectOption == 2 && isOnNetwork) || (redirectOption == 3 && !isOnNetwork) )
                {
                    RedirectToUrl( url );
                    return;
                }

                return;
                
            }
            else
            {
                nbAlert.Text = "Missing Url value for redirect!";
            }
        }

        private void RedirectToUrl(string url)
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, CurrentPerson );
            string resolvedUrl = url.ResolveMergeFields( mergeFields );

            if ( IsUserAuthorized( Rock.Security.Authorization.ADMINISTRATE ) )
            {
                nbAlert.Text = string.Format( "If you did not have Administrate permissions on this block, you would have been redirected to here: <a href='{0}'>{0}</a>.", Page.ResolveUrl( resolvedUrl ) );
            }
            else
            {
                Response.Redirect( resolvedUrl, false );
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }

        // true if ipAddress falls inside the CIDR range, example
        // bool result = IsInRange("10.50.30.7", "10.0.0.0/8");
        private bool IsInRange( string ipAddress, string CIDRmask )
        {
            string[] parts = CIDRmask.Split( '/' );

            int IP_addr = BitConverter.ToInt32( IPAddress.Parse( parts[0] ).GetAddressBytes(), 0 );
            int CIDR_addr = BitConverter.ToInt32( IPAddress.Parse( ipAddress ).GetAddressBytes(), 0 );
            int CIDR_mask = IPAddress.HostToNetworkOrder( -1 << (32 - int.Parse( parts[1] )) );

            return ((IP_addr & CIDR_mask) == (CIDR_addr & CIDR_mask));
        }

    }
}