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
    /// Tracks when a short link is used.
    /// </summary>
    public class ShortLinkTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the Page Id.
        /// </summary>
        /// <value>
        /// Page Id.
        /// </value>
        public int? SiteUrlMapId { get; set; }

        /// <summary>
        /// Gets or sets the token.
        /// </summary>
        /// <value>
        /// The token.
        /// </value>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the current username.
        /// </summary>
        /// <value>
        /// User Name.
        /// </value>
        public string UserName { get; set; }

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
        /// Gets or sets the session id.
        /// </summary>
        /// <value>
        /// Session Id.
        /// </value>
        public string SessionId { get; set; }

        /// <summary>
        /// The ua parser
        /// </summary>
        private static Parser uaParser = Parser.GetDefault();

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var userAgent = (this.UserAgent ?? string.Empty).Trim();
                if ( userAgent.Length > 450 )
                {
                    userAgent = userAgent.Substring( 0, 450 ); // trim super long useragents to fit in pageViewUserAgent.UserAgent
                }

                int? personAliasId = null;
                if ( UserName.IsNotNullOrWhitespace() )
                {
                    var currentUser = new UserLoginService( rockContext ).GetByUserName( UserName );
                    personAliasId = currentUser?.Person?.PrimaryAlias?.Id;
                }

                // get user agent info
                var clientType = InteractionDeviceType.GetClientType( userAgent );

                // don't log visits from crawlers
                if ( clientType != "Crawler" )
                {
                    InteractionChannelService interactionChannelService = new InteractionChannelService( rockContext );
                    InteractionComponentService interactionComponentService = new InteractionComponentService( rockContext );
                    InteractionDeviceTypeService interactionDeviceTypeService = new InteractionDeviceTypeService( rockContext );
                    InteractionSessionService interactionSessionService = new InteractionSessionService( rockContext );
                    InteractionService interactionService = new InteractionService( rockContext );

                    ClientInfo client = uaParser.Parse( userAgent );
                    var clientOs = client.OS.ToString();
                    var clientBrowser = client.UserAgent.ToString();

                    // lookup the interactionDeviceType, and create it if it doesn't exist
                    var interactionDeviceType = interactionService.GetInteractionDeviceType( clientBrowser, clientOs, clientType, userAgent );

                    // lookup interactionSession, and create it if it doesn't exist
                    InteractionSession interactionSession = interactionService.GetInteractionSession( this.SessionId.AsGuidOrNull(), this.IPAddress, interactionDeviceType.Id );

                    int componentEntityTypeId = EntityTypeCache.Read<Rock.Model.SiteUrlMap>().Id;
                    
                    // lookup the interaction channel, and create it if it doesn't exist
                    int channelMediumTypeValueId = DefinedValueCache.Read( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_URLSHORTENER.AsGuid() ).Id;

                    // check that the Site Url Map exists as a channel
                    var interactionChannel = interactionChannelService.Queryable()
                        .Where( a => a.ChannelTypeMediumValueId == channelMediumTypeValueId )
                        .FirstOrDefault();

                    if ( interactionChannel == null )
                    {
                        interactionChannel = new InteractionChannel();
                        interactionChannel.Name = "UrlShortener";
                        interactionChannel.ChannelTypeMediumValueId = channelMediumTypeValueId;
                        interactionChannel.ComponentEntityTypeId = componentEntityTypeId;
                        interactionChannelService.Add( interactionChannel );
                        rockContext.SaveChanges();
                    }

                    // check that the page exists as a component
                    var interactionComponent = interactionComponentService.Queryable()
                        .Where( a => 
                            a.EntityId == SiteUrlMapId && 
                            a.ChannelId == interactionChannel.Id )
                        .FirstOrDefault();
                    if ( interactionComponent == null )
                    {
                        interactionComponent = new InteractionComponent();
                        interactionComponent.Name = Token;
                        interactionComponent.EntityId = SiteUrlMapId;
                        interactionComponent.ChannelId = interactionChannel.Id;
                        interactionComponentService.Add( interactionComponent );
                        rockContext.SaveChanges();
                    }
                                      
                    // add the interaction
                    Interaction interaction = new Interaction();
                    interactionService.Add( interaction );

                    // obfuscate rock magic token
                    Regex rgx = new Regex( @"rckipid=([^&]*)" );
                    string cleanUrl = rgx.Replace( this.Url, "rckipid=XXXXXXXXXXXXXXXXXXXXXXXXXXXX" );

                    interaction.InteractionData = cleanUrl;
                    interaction.Operation = "View";
                    interaction.PersonAliasId = personAliasId;
                    interaction.InteractionDateTime = this.DateViewed;
                    interaction.InteractionSessionId = interactionSession != null ? interactionSession.Id : (int?)null;
                    interaction.InteractionComponentId = interactionComponent.Id;
                    rockContext.SaveChanges();
                }
            }
        }
    }
}