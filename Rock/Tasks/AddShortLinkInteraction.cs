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
using Rock.Cms.Utm;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using UAParser;

namespace Rock.Tasks
{
    /// <summary>
    /// Updates Interaction data to track when a <see cref="PageShortLink">short link</see> is used
    /// </summary>
    public sealed class AddShortLinkInteraction : BusStartedTask<AddShortLinkInteraction.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var rockContext = new RockContext();
            var pageShortLink = new PageShortLinkService( rockContext ).Get( message.PageShortLinkId );
            if ( pageShortLink == null )
            {
                return;
            }

            var userAgent = ( message.UserAgent ?? string.Empty ).Trim();
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
                int channelMediumTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_URLSHORTENER.AsGuid() ).Id;
                InteractionChannelService interactionChannelService = new InteractionChannelService( rockContext );
                var interactionChannel = interactionChannelService.Queryable()
                    .Where( a => a.ChannelTypeMediumValueId == channelMediumTypeValueId )
                    .FirstOrDefault();
                if ( interactionChannel == null )
                {
                    interactionChannel = new InteractionChannel();
                    interactionChannel.Name = "Short Links";
                    interactionChannel.ChannelTypeMediumValueId = channelMediumTypeValueId;
                    interactionChannel.ComponentEntityTypeId = EntityTypeCache.Get<Rock.Model.PageShortLink>().Id;
                    interactionChannel.Guid = SystemGuid.InteractionChannel.SHORT_LINKS.AsGuid();
                    interactionChannelService.Add( interactionChannel );
                    rockContext.SaveChanges();
                }

                // check that the page exists as a component
                var interactionComponent = new InteractionComponentService( rockContext ).GetComponentByChannelIdAndEntityId( interactionChannel.Id, pageShortLink.Id, message.Token );
                if ( message.Url.IsNotNullOrWhiteSpace() )
                {
                    if ( interactionComponent.ComponentSummary != message.Url )
                    {
                        interactionComponent.ComponentSummary = message.Url;
                    }

                    var urlDataJson = new { Url = message.Url }.ToJson();
                    if ( interactionComponent.ComponentData != urlDataJson )
                    {
                        interactionComponent.ComponentData = urlDataJson;
                    }
                }

                rockContext.SaveChanges();

                // Add the interaction
                if ( interactionComponent != null )
                {
                    int? personAliasId = null;
                    if ( message.UserName.IsNotNullOrWhiteSpace() )
                    {
                        var currentUser = new UserLoginService( rockContext ).GetByUserName( message.UserName );
                        personAliasId = currentUser?.Person?.PrimaryAlias?.Id;
                    }

                    if ( !personAliasId.HasValue && message.VisitorPersonAliasIdKey.IsNotNullOrWhiteSpace() )
                    {
                        personAliasId = new PersonAliasService( rockContext ).GetSelect( message.VisitorPersonAliasIdKey, s => s.Id );
                    }

                    Parser uaParser = Parser.GetDefault();
                    ClientInfo client = uaParser.Parse( userAgent );
                    var clientOs = client.OS.ToString();
                    var clientBrowser = client.UA.ToString();

                    var interaction = new InteractionService( rockContext ).AddInteraction( interactionComponent.Id, null, "View", message.Url, personAliasId, message.DateViewed, clientBrowser, clientOs, clientType, userAgent, message.IPAddress, message.SessionId?.AsGuidOrNull() );

                    UtmHelper.GetUtmDefinedValueOrTextFromInputValue( message.UtmSource,
                        SystemGuid.DefinedType.UTM_SOURCE,
                        out int? sourceValueId,
                        out string sourceText );

                    UtmHelper.GetUtmDefinedValueOrTextFromInputValue( message.UtmMedium,
                        SystemGuid.DefinedType.UTM_MEDIUM,
                        out int? mediumValueId,
                        out string mediumText );

                    UtmHelper.GetUtmDefinedValueOrTextFromInputValue( message.UtmCampaign,
                        SystemGuid.DefinedType.UTM_CAMPAIGN,
                        out int? campaignValueId,
                        out string campaignText );

                    interaction.Source = sourceText;
                    interaction.SourceValueId = sourceValueId;
                    interaction.Medium = mediumText;
                    interaction.MediumValueId = mediumValueId;
                    interaction.Campaign = campaignText;
                    interaction.CampaignValueId = campaignValueId;
                    interaction.ChannelCustomIndexed1 = message.PurposeKey.Truncate( 500, false );

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the Page Id.
            /// </summary>
            /// <value>
            /// Page Id.
            /// </value>
            public int PageShortLinkId { get; set; }

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
            /// Gets or sets the visitor person alias identifier key.
            /// This will get used if UserName is not specified.
            /// </summary>
            /// <value>The visitor person alias identifier key.</value>
            public string VisitorPersonAliasIdKey { get; set; }

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
            /// Gets or sets the UTM source of the link
            /// </summary>
            public string UtmSource { get; set; }

            /// <summary>
            /// Gets or sets the UTM medium of the link
            /// </summary>
            public string UtmMedium { get; set; }

            /// <summary>
            /// Gets or sets the UTM campaign of the link
            /// </summary>
            public string UtmCampaign { get; set; }

            /// <summary>
            /// Gets or sets the purpose of the redirect. This is generic data
            /// that has no specific meaning. It will be stored in the
            /// <see cref="Interaction.ChannelCustomIndexed1"/> property.
            /// </summary>
            public string PurposeKey { get; set; }
        }
    }
}