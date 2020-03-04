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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Transactions
{
    /// <summary>
    /// Transaction that will insert interactions. For example, Page Views
    /// </summary>
    public class InteractionTransaction : ITransaction
    {
        private class InteractionInfo
        {
            /// <summary>
            /// Gets or sets the channel type medium value identifier.
            /// </summary>
            /// <value>
            /// The channel type medium value identifier.
            /// </value>
            public int ChannelTypeMediumValueId { get; set; }

            /// <summary>
            /// Gets or sets the channel entity identifier.
            /// </summary>
            /// <value>
            /// The channel entity identifier.
            /// </value>
            public int ChannelEntityId { get; set; }

            /// <summary>
            /// Gets or sets the name of the channel.
            /// </summary>
            /// <value>
            /// The name of the channel.
            /// </value>
            public string ChannelName { get; set; }

            /// <summary>
            /// Gets or sets the component entity type identifier.
            /// </summary>
            /// <value>
            /// The component entity type identifier.
            /// </value>
            public int ComponentEntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the component entity identifier.
            /// </summary>
            /// <value>
            /// The component entity identifier.
            /// </value>
            public int ComponentEntityId { get; set; }

            /// <summary>
            /// Gets or sets the name of the component.
            /// </summary>
            /// <value>
            /// The name of the component.
            /// </value>
            public string ComponentName { get; set; }

            /// <summary>
            /// Gets or sets the browser session identifier.
            /// </summary>
            /// <value>
            /// The browser session identifier.
            /// </value>
            public Guid? BrowserSessionId { get; set; }

            /// <summary>
            /// Gets or sets the user agent.
            /// </summary>
            /// <value>
            /// The user agent.
            /// </value>
            public string UserAgent { get; set; }

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            /// <value>
            /// The URL.
            /// </value>
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the ip address.
            /// </summary>
            /// <value>
            /// The ip address.
            /// </value>
            public string IPAddress { get; set; }

            /// <summary>
            /// Gets or sets the interaction date time.
            /// </summary>
            /// <value>
            /// The interaction date time.
            /// </value>
            public DateTime InteractionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the interaction channel identifier.
            /// </summary>
            /// <value>
            /// The interaction channel identifier.
            /// </value>
            public int? InteractionChannelId { get; set; }

            /// <summary>
            /// Gets or sets the interaction component identifier.
            /// </summary>
            /// <value>
            /// The interaction component identifier.
            /// </value>
            public int? InteractionComponentId { get; set; }

            /// <summary>
            /// Gets or sets the interaction summary.
            /// </summary>
            /// <value>
            /// The interaction summary.
            /// </value>
            public string InteractionSummary { get; set; }

            /// <summary>
            /// Gets or sets the person alias identifier.
            /// </summary>
            /// <value>
            /// The person alias identifier.
            /// </value>
            public int? PersonAliasId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [log crawlers].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [log crawlers]; otherwise, <c>false</c>.
            /// </value>
            public bool LogCrawlers { get; set; }

            /// <summary>
            /// Gets or sets the time to serve the interaction.
            /// </summary>
            /// <value>
            /// The time to serve the interaction.
            /// </value>
            public double? InteractionTimeToServe { get; set; }
        }

        /// <summary>
        /// Keep a list of all the Interactions that have been queued up then insert them all at once 
        /// </summary>
        private static readonly ConcurrentQueue<InteractionInfo> InteractionInfoQueue = new ConcurrentQueue<InteractionInfo>();

        /// <summary>
        /// Optional: Gets or sets the interaction summary. Leave null to use the Page Browser Title or Page Title
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        [Obsolete( "Use the constructor that takes InteractionTransactionOptions" )]
        [RockObsolete( "1.11" )]
        public string InteractionSummary { get; set; }

        /// <summary>
        /// Gets or sets a value the Interaction should get logged when the page is viewed by the crawler (default False)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log crawlers]; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "Use the constructor that takes InteractionTransactionOptions" )]
        [RockObsolete( "1.11" )]
        public bool LogCrawlers { get; set; } = false;

        /// <summary>
        /// Gets or sets the current person alias identifier.
        /// </summary>
        /// <value>
        /// The current person alias identifier.
        /// </value>
        [Obsolete( "Use the constructor that takes InteractionTransactionOptions" )]
        [RockObsolete( "1.11" )]
        public int? CurrentPersonAliasId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Transactions.InteractionTransaction"/> class.
        /// </summary>
        /// <param name="channelMediumTypeValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        public InteractionTransaction( DefinedValueCache channelMediumTypeValue, IEntity channelEntity, IEntity componentEntity ) :
            this( channelMediumTypeValue, channelEntity, componentEntity, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction" /> class.
        /// </summary>
        /// <param name="channelMediumTypeValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        /// <param name="options">The options.</param>
        public InteractionTransaction( DefinedValueCache channelMediumTypeValue, IEntity channelEntity, IEntity componentEntity, InteractionTransactionOptions options ) :
            this( channelMediumTypeValue, channelEntity, componentEntity, options, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction" /> class.
        /// </summary>
        /// <param name="channelMediumTypeValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        /// <param name="options">The options.</param>
        /// <param name="interactionTimeToServe">The interaction time to serve.</param>
        public InteractionTransaction( DefinedValueCache channelMediumTypeValue, IEntity channelEntity, IEntity componentEntity, InteractionTransactionOptions options, double? interactionTimeToServe )
        {
            if ( channelEntity == null || componentEntity == null )
            {
                return;
            }

            var interactionInfo = new InteractionInfo()
            {
                ChannelTypeMediumValueId = channelMediumTypeValue.Id,
                ChannelEntityId = channelEntity.Id,
                ChannelName = channelEntity.ToString(),
                ComponentEntityTypeId = channelEntity.TypeId,
                ComponentEntityId = componentEntity.Id,
                ComponentName = componentEntity.ToString(),
                InteractionTimeToServe = interactionTimeToServe
            };

            Initialize( interactionInfo, options );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction"/> class.
        /// </summary>
        /// <param name="channelMediumTypeValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        public InteractionTransaction( DefinedValueCache channelMediumTypeValue, IEntityCache channelEntity, IEntityCache componentEntity ) :
            this( channelMediumTypeValue, channelEntity, componentEntity, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction" /> class.
        /// </summary>
        /// <param name="channelMediumTypeValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        /// <param name="options">The options.</param>
        public InteractionTransaction( DefinedValueCache channelMediumTypeValue, IEntityCache channelEntity, IEntityCache componentEntity, InteractionTransactionOptions options ) :
            this( channelMediumTypeValue, channelEntity, componentEntity, null, null )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction" /> class.
        /// </summary>
        /// <param name="channelMediumTypeValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        /// <param name="options">The options.</param>
        /// <param name="interactionTimeToServe">The interaction time to serve.</param>
        public InteractionTransaction( DefinedValueCache channelMediumTypeValue, IEntityCache channelEntity, IEntityCache componentEntity, InteractionTransactionOptions options, double? interactionTimeToServe )
        {
            if ( channelEntity == null || componentEntity == null )
            {
                // don't write an interaction if we don't know channelEntity or componentEntity
                return;
            }

            var interactionInfo = new InteractionInfo()
            {
                ChannelTypeMediumValueId = channelMediumTypeValue.Id,
                ChannelEntityId = channelEntity.Id,
                ChannelName = channelEntity.ToString(),
                ComponentEntityTypeId = channelEntity.CachedEntityTypeId,
                ComponentEntityId = componentEntity.Id,
                ComponentName = componentEntity.ToString(),
                InteractionTimeToServe = interactionTimeToServe
            };

            Initialize( interactionInfo, options );
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        private void Initialize( InteractionInfo interactionInfo, InteractionTransactionOptions options )
        {
            RockPage rockPage = null;
            HttpRequest request = null;

            options = options ?? new InteractionTransactionOptions();

            if ( options.GetValuesFromHttpRequest )
            {

                try
                {
                    rockPage = HttpContext.Current.Handler as RockPage;
                }
                catch
                {
                    rockPage = null;
                }


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
            }

            interactionInfo.Url = options?.Url ?? request?.Url.ToString();
            interactionInfo.UserAgent = options?.UserAgent ?? request?.UserAgent;
            try
            {
                interactionInfo.IPAddress = options?.IPAddress ?? RockPage.GetClientIpAddress();
            }
            catch
            {
                interactionInfo.IPAddress = string.Empty;
            }

            if ( interactionInfo.Url == null )
            {
                // can't log a interaction if we don't know the URL they are interacting with
                return;
            }

            interactionInfo.BrowserSessionId = options?.BrowserSessionId ?? rockPage?.Session["RockSessionId"]?.ToString().AsGuidOrNull();

            // if a specific PersonAliasId was specified, use that, otherwise get it from the page
            interactionInfo.PersonAliasId = options?.PersonAliasId ?? rockPage?.CurrentPersonAliasId;

            var title = string.Empty;
            if ( rockPage != null )
            {
                if ( rockPage.BrowserTitle.IsNotNullOrWhiteSpace() )
                {
                    title = rockPage.BrowserTitle;
                }
                else
                {
                    title = rockPage.PageTitle;
                }
            }

            // remove site name from browser title
            if ( title?.Contains( "|" ) == true )
            {
                title = title.Substring( 0, title.LastIndexOf( '|' ) ).Trim();
            }

            // if a specific InteractionSummary was specified, use that, otherwise get it from the page
            interactionInfo.InteractionSummary = options?.InteractionSummary ?? title;

            // if a specific LogCrawlers was specified, use that, don't log crawlers
            interactionInfo.LogCrawlers = options?.LogCrawlers ?? false;

            interactionInfo.InteractionDateTime = options?.InteractionDateTime ?? RockDateTime.Now;

            InteractionInfoQueue.Enqueue( interactionInfo );
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            var interactionInfosToProcess = new List<InteractionInfo>();
            while ( InteractionInfoQueue.TryDequeue( out InteractionInfo interactionInfo ) )
            {
                interactionInfosToProcess.Add( interactionInfo );
            }

            if ( !interactionInfosToProcess.Any() )
            {
                // if all the interactions have been process, just exit
                return;
            }

            var userAgentsLookup = interactionInfosToProcess.Select( a => a.UserAgent ).Distinct().ToList().ToDictionary( a => a, v => InteractionDeviceType.GetClientType( v ) );

            interactionInfosToProcess = interactionInfosToProcess.Where( a => a.LogCrawlers || userAgentsLookup.GetValueOrNull( a.UserAgent ) != "Crawler" ).ToList();

            if ( !interactionInfosToProcess.Any() )
            {
                // if there aren't interactions after removing Crawlers, exit
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                LogInteractions( interactionInfosToProcess, rockContext );
            }
        }

        /// <summary>
        /// Logs the interactions.
        /// </summary>
        /// <param name="interactionInfosToProcess">The interaction infos to process.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LogInteractions( List<InteractionInfo> interactionInfosToProcess, RockContext rockContext )
        {
            List<Interaction> interactionsToInsert = new List<Interaction>();

            foreach ( var interactionInfo in interactionInfosToProcess )
            {
                interactionInfo.InteractionChannelId = InteractionChannelCache.GetChannelIdByEntityId( interactionInfo.ChannelTypeMediumValueId, interactionInfo.ChannelEntityId, interactionInfo.ComponentEntityTypeId, interactionInfo.ComponentName );
                interactionInfo.InteractionComponentId = InteractionComponentCache.GetComponentIdByEntityId( interactionInfo.InteractionChannelId.Value, interactionInfo.ComponentEntityId, interactionInfo.ComponentName );
            }

            var interactionService = new InteractionService( rockContext );

            foreach ( var interactionInfo in interactionInfosToProcess.Where( a => a.InteractionComponentId.HasValue ) )
            {
                var interaction = interactionService.CreateInteraction( interactionInfo.InteractionComponentId.Value, interactionInfo.UserAgent, interactionInfo.Url, interactionInfo.IPAddress, interactionInfo.BrowserSessionId );
                interaction.InteractionComponentId = interactionInfo.InteractionComponentId.Value;
                interaction.EntityId = null;
                interaction.Operation = "View";
                interaction.InteractionSummary = interactionInfo.InteractionSummary;
                interaction.PersonAliasId = interactionInfo.PersonAliasId;
                interaction.InteractionDateTime = interactionInfo.InteractionDateTime;
                interaction.InteractionTimeToServe = interactionInfo.InteractionTimeToServe;
                interactionsToInsert.Add( interaction );
            }

            rockContext.BulkInsert( interactionsToInsert );

            // This logic is normally handled in the Interaction.PostSave method, but since the BulkInsert bypasses those
            // model hooks, streaks need to be updated here. Also, it is not necessary for this logic to complete before this
            // transaction can continue processing and exit, so update the streak using a task.
            interactionsToInsert.ForEach( i => Task.Run( () => StreakTypeService.HandleInteractionRecord( i ) ) );
        }
    }

    /// <summary>
    /// Any custom options on what to log with the interaction
    /// </summary>
    public class InteractionTransactionOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether [log crawlers].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log crawlers]; otherwise, <c>false</c>.
        /// </value>
        public bool LogCrawlers { get; set; } = false;

        /// <summary>
        /// Gets or sets the interaction summary.
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        public string InteractionSummary { get; set; } = null;

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId { get; set; } = null;

        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        /// <value>
        /// The user agent.
        /// </value>
        public string UserAgent { get; set; } = null;

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IPAddress { get; set; } = null;

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; } = null;

        /// <summary>
        /// Gets or sets the browser session identifier.
        /// </summary>
        /// <value>
        /// The browser session identifier.
        /// </value>
        public Guid? BrowserSessionId { get; set; } = null;

        /// <summary>
        /// Gets or sets the interaction date time.
        /// </summary>
        /// <value>
        /// The interaction date time.
        /// </value>
        public DateTime? InteractionDateTime { get; set; } = null;

        /// <summary>
        /// Gets or sets a value indicating whether [get values from HTTP request].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get values from HTTP request]; otherwise, <c>false</c>.
        /// </value>
        public bool GetValuesFromHttpRequest { get; set; } = true;
    }
}