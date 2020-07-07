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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks when a page is viewed.
    /// </summary>
    [RockObsolete( "1.8" )]
    [Obsolete("Use InteractionTransaction Instead", true )]
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
        /// Gets or sets the browser title. This can be different than the page title as Lava and/or blocks can change this.
        /// </summary>
        /// <value>
        /// The browser title.
        /// </value>
        public string BrowserTitle { get; set; }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            
            if ( !PageId.HasValue )
            {
                return;
            }

            var userAgent = ( this.UserAgent ?? string.Empty ).Trim();
            if ( userAgent.Length > 450 )
            {
                userAgent = userAgent.Substring( 0, 450 ); // trim super long useragents to fit in pageViewUserAgent.UserAgent
            }

            // get user agent info
            var clientType = InteractionDeviceType.GetClientType( userAgent );
            // don't log visits from crawlers
            if ( clientType == "Crawler" )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                // lookup the interaction channel, and create it if it doesn't exist
                int channelMediumTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE.AsGuid() ).Id;
                var interactionChannelService = new InteractionChannelService( rockContext );
                var interactionChannelId = interactionChannelService.Queryable()
                    .Where( a =>
                        a.ChannelTypeMediumValueId == channelMediumTypeValueId &&
                        a.ChannelEntityId == this.SiteId )
                    .Select( a => ( int? ) a.Id )
                    .FirstOrDefault();
                if ( interactionChannelId == null )
                {
                    var interactionChannel = new InteractionChannel();
                    interactionChannel.Name = SiteCache.Get( SiteId ?? 1 ).Name;
                    interactionChannel.ChannelTypeMediumValueId = channelMediumTypeValueId;
                    interactionChannel.ChannelEntityId = this.SiteId;
                    interactionChannel.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.Page>().Id;
                    interactionChannelService.Add( interactionChannel );
                    rockContext.SaveChanges();
                    interactionChannelId = interactionChannel.Id;
                }

                // check that the page exists as a component
                var interactionComponent = new InteractionComponentService( rockContext ).GetComponentByChannelIdAndEntityId( interactionChannelId.Value, PageId, PageTitle );
                if ( interactionComponent.Id == 0 )
                {
                    rockContext.SaveChanges();
                }

                // Add the interaction
                if ( interactionComponent != null )
                {
                    var title = string.Empty;
                    if ( BrowserTitle.IsNotNullOrWhiteSpace() )
                    {
                        title = BrowserTitle;
                    }
                    else
                    {
                        title = PageTitle;
                    }

                    // remove site name from browser title
                    if ( title.Contains( "|" ) )
                    {
                        title = title.Substring( 0, title.LastIndexOf( '|' ) ).Trim();
                    }

                    var interactionService = new InteractionService( rockContext );
                    var interaction = interactionService.CreateInteraction( interactionComponent.Id, this.UserAgent, this.Url, this.IPAddress, this.SessionId.AsGuidOrNull() );

                    interaction.EntityId = null;
                    interaction.Operation = "View";
                    interaction.InteractionSummary = title;
                    interaction.InteractionData = Url;
                    interaction.PersonAliasId = PersonAliasId;
                    interaction.InteractionDateTime = DateViewed;
                    interactionService.Add( interaction );
                    
                    rockContext.SaveChanges();
                }
            }
        }
    }
}