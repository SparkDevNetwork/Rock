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
using System.Linq;
using System.Text.RegularExpressions;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using UAParser;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a page is viewed.
    /// </summary>
    public class PageViewTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int? PageId { get; set; }

        /// <summary>
        /// Gets or sets the Site Id.
        /// </summary>
        /// <value>
        /// Site Id.
        /// </value>
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the Person Id.
        /// </summary>
        /// <value>
        /// Person Id.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the DateTime the page was viewed.
        /// </summary>
        /// <value>
        /// Date Viewed.
        /// </value>
        public DateTime DateViewed { get; set; }

        /// <summary>
        /// Gets or sets the IP address that requested the page.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the browser vendor and version.
        /// </summary>
        /// <value>
        /// IP Address.
        /// </value>
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the Rock SessionId Guid ( RockSessionId )
        /// </summary>
        /// <value>
        /// Session Id.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the query string.
        /// </summary>
        /// <value>
        /// Query String.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        /// <value>
        /// Page Title.
        /// </value>
        public string PageTitle { get; set; }

        /// <summary>
        /// The ua parser
        /// </summary>
        private static Parser uaParser = Parser.GetDefault();

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            if ( PageId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var userAgent = ( this.UserAgent ?? string.Empty ).Trim();
                    if ( userAgent.Length > 450 )
                    {
                        userAgent = userAgent.Substring( 0, 450 ); // trim super long useragents to fit in pageViewUserAgent.UserAgent
                    }

                    // get user agent info
                    var clientType = InteractionDeviceType.GetClientType( userAgent );

                    // don't log visits from crawlers
                    if ( clientType != "Crawler" )
                    {
                        // lookup the interaction channel, and create it if it doesn't exist
                        int channelMediumTypeValueId = DefinedValueCache.Read( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
                        var interactionChannelService = new InteractionChannelService( rockContext );
                        var interactionChannel = interactionChannelService.Queryable()
                            .Where( a =>
                                a.ChannelTypeMediumValueId == channelMediumTypeValueId &&
                                a.ChannelEntityId == this.SiteId )
                            .FirstOrDefault();
                        if ( interactionChannel == null )
                        {
                            interactionChannel = new InteractionChannel();
                            interactionChannel.Name = SiteCache.Read( SiteId ?? 1 ).Name;
                            interactionChannel.ChannelTypeMediumValueId = channelMediumTypeValueId;
                            interactionChannel.ChannelEntityId = this.SiteId;
                            interactionChannel.ComponentEntityTypeId = EntityTypeCache.Read<Rock.Model.Page>().Id;
                            interactionChannelService.Add( interactionChannel );
                            rockContext.SaveChanges();
                        }

                        // check that the page exists as a component
                        var interactionComponent = new InteractionComponentService( rockContext ).GetComponentByEntityId( interactionChannel.Id, PageId.Value, PageTitle );
                        rockContext.SaveChanges();

                        // Add the interaction
                        if ( interactionComponent != null )
                        {
                            ClientInfo client = uaParser.Parse( userAgent );
                            var clientOs = client.OS.ToString();
                            var clientBrowser = client.UserAgent.ToString();

                            new InteractionService( rockContext ).AddInteraction( interactionComponent.Id, null, "View", Url, PersonAliasId, DateViewed,
                                clientBrowser, clientOs, clientType, userAgent, IPAddress, this.SessionId?.AsGuidOrNull() );
                            rockContext.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}