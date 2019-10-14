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
using System.Web;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Transactions
{
    /// <summary>
    /// Tracks interactions
    /// </summary>
    public class InteractionTransaction : ITransaction
    {
        private DefinedValueCache _channelMediumTypeValue;
        private int _channelEntityId;
        private string _channelName;
        private int _componentEntityTypeId;
        private int _componentEntityId;
        private string _componentName;
        private Guid? _browserSessionId;
        private string _userAgent;
        private string _url;
        private string _ipAddress;
        private int? _currentPersonAliasId;
        private DateTime _interactionDateTime;

        /// <summary>
        /// set to false if insufficient data was specified to log the transaction
        /// </summary>
        private bool _logInteraction = true;

        /// <summary>
        /// Optional: Gets or sets the interaction summary. Leave null to use the Page Browser Title or Page Title
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        public string InteractionSummary { get; set; }

        /// <summary>
        /// Gets or sets a value the Interaction should get logged when the page is viewed by the crawler (default False)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log crawlers]; otherwise, <c>false</c>.
        /// </value>
        public bool LogCrawlers { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transactions.InteractionTransaction"/> class.
        /// </summary>
        /// <param name="channelMediumTypeValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        public InteractionTransaction( DefinedValueCache channelMediumTypeValue, IEntity channelEntity, IEntity componentEntity )
        {
            if ( channelEntity == null || componentEntity == null )
            {
                _logInteraction = false;
            }

            _channelMediumTypeValue = channelMediumTypeValue;
            _channelEntityId = channelEntity.Id;
            _channelName = channelEntity.ToString();
            _componentEntityTypeId = channelEntity.TypeId;
            _componentEntityId = componentEntity.Id;
            _componentName = componentEntity.ToString();

            Initialize();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction"/> class.
        /// </summary>
        /// <param name="channelMediumTypeValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        public InteractionTransaction( DefinedValueCache channelMediumTypeValue, IEntityCache channelEntity, IEntityCache componentEntity )
        {
            if ( channelEntity == null || componentEntity == null )
            {
                _logInteraction = false;
                return;
            }

            _channelMediumTypeValue = channelMediumTypeValue;
            _channelEntityId = channelEntity.Id;
            _channelName = channelEntity.ToString();
            _componentEntityTypeId = channelEntity.CachedEntityTypeId;
            _componentEntityId = componentEntity.Id;
            _componentName = componentEntity.ToString();

            Initialize();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize()
        {
            RockPage rockPage;

            try
            {
                rockPage = HttpContext.Current.Handler as RockPage;
            }
            catch
            {
                rockPage = null;
            }

            HttpRequest request = null;
            try
            {
                if ( rockPage != null )
                {
                    request = rockPage.Request;
                }
                else if ( HttpContext.Current != null )
                {
                    request = HttpContext.Current.Request;
                }
            }
            catch
            {
                // intentionally ignore exception (.Request will throw an exception instead of simply returning null if it isn't available)
            }

            if ( rockPage == null || request == null )
            {
                _logInteraction = false;
                return;
            }

            _browserSessionId = rockPage.Session["RockSessionID"]?.ToString().AsGuidOrNull();
            _userAgent = request.UserAgent;
            _url = request.Url.ToString();
            try
            {
                _ipAddress = RockPage.GetClientIpAddress();
            }
            catch
            {
                _ipAddress = "";
            }

            _currentPersonAliasId = rockPage.CurrentPersonAliasId;

            var title = string.Empty;
            if ( rockPage.BrowserTitle.IsNotNullOrWhiteSpace() )
            {
                title = rockPage.BrowserTitle;
            }
            else
            {
                title = rockPage.PageTitle;
            }

            // remove site name from browser title
            if ( title?.Contains( "|" ) == true)
            {
                title = title.Substring( 0, title.LastIndexOf( '|' ) ).Trim();
            }

            InteractionSummary = title;

            _interactionDateTime = RockDateTime.Now;
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            if ( !this._logInteraction )
            {
                return;
            }

            if ( !this.LogCrawlers )
            {
                // get user agent info
                var clientType = InteractionDeviceType.GetClientType( _userAgent );
                // don't log visits from crawlers
                if ( clientType == "Crawler" )
                {
                    return;
                }
            }

            var rockContext = new RockContext();

            // lookup the interaction channel, and create it if it doesn't exist
            var interactionChannelService = new InteractionChannelService( rockContext );
            var interactionChannelId = interactionChannelService.Queryable()
                .Where( a =>
                    a.ChannelTypeMediumValueId == _channelMediumTypeValue.Id &&
                    a.ChannelEntityId == _channelEntityId )
                .Select( a => ( int? ) a.Id )
                .FirstOrDefault();
            if ( interactionChannelId == null )
            {
                var interactionChannel = new InteractionChannel();
                interactionChannel.Name = _channelName;
                interactionChannel.ChannelTypeMediumValueId = _channelMediumTypeValue.Id;
                interactionChannel.ChannelEntityId = _channelEntityId;
                interactionChannel.ComponentEntityTypeId = _componentEntityTypeId;
                interactionChannelService.Add( interactionChannel );
                rockContext.SaveChanges();
                interactionChannelId = interactionChannel.Id;
            }

            // check that the contentChannelItem exists as a component
            var interactionComponent = new InteractionComponentService( rockContext ).GetComponentByEntityId( interactionChannelId.Value, _componentEntityId, _componentName );
            rockContext.SaveChanges();

            // Add the interaction
            if ( interactionComponent != null )
            {
                var interactionService = new InteractionService( rockContext );
                var interaction = interactionService.CreateInteraction( interactionComponent.Id, _userAgent, _url, _ipAddress, _browserSessionId );

                interaction.EntityId = null;
                interaction.Operation = "View";
                interaction.InteractionSummary = InteractionSummary;
                interaction.InteractionData = _url;
                interaction.PersonAliasId = _currentPersonAliasId;
                interaction.InteractionDateTime = RockDateTime.Now;
                interactionService.Add( interaction );
                rockContext.SaveChanges();
            }
        }
    }
}