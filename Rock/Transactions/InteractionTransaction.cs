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
    /// Transaction that will insert <seealso cref="Rock.Model.Interaction">Interactions</seealso>. For example, Page Views.
    /// </summary>
    public class InteractionTransaction : ITransaction
    {
        /// <summary>
        /// Keep a list of all the Interactions that have been queued up then insert them all at once 
        /// </summary>
        private static readonly ConcurrentQueue<InteractionTransactionInfo> InteractionInfoQueue = new ConcurrentQueue<InteractionTransactionInfo>();

        /// <summary>
        /// Optional: Gets or sets the interaction summary. Leave null to use the Page Browser Title or Page Title
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        [Obsolete( "Use a constructor that takes InteractionTransactionInfo." )]
        [RockObsolete( "1.11" )]
        public string InteractionSummary { get; set; }

        /// <summary>
        /// Gets or sets a value the Interaction should get logged when the page is viewed by the crawler (default False)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log crawlers]; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "Use a constructor that takes InteractionTransactionInfo." )]
        [RockObsolete( "1.11" )]
        public bool LogCrawlers { get; set; } = false;

        /// <summary>
        /// Gets or sets the current person alias identifier.
        /// </summary>
        /// <value>
        /// The current person alias identifier.
        /// </value>
        [Obsolete( "Use a constructor that takes InteractionTransactionInfo." )]
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
        /// Initializes a new instance of the <see cref="Transactions.InteractionTransaction"/> class.
        /// </summary>
        /// <param name="channelTypeMediumValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        /// <param name="info">
        /// The information about the <see cref="Interaction"/> object graph to be logged.
        /// In the case of conflicting values (i.e. channelEntity.Id vs info.ChannelEntityId), any values explicitly set on the <paramref name="info"/> parameter will take precedence.
        /// </param>
        public InteractionTransaction( DefinedValueCache channelTypeMediumValue, IEntity channelEntity, IEntity componentEntity, InteractionTransactionInfo info )
        {
            if ( channelTypeMediumValue == null || channelEntity == null || componentEntity == null )
            {
                return;
            }

            Initialize( channelTypeMediumValue.Id, channelEntity.Id, channelEntity.ToString(), channelEntity.TypeId, componentEntity.Id, componentEntity.ToString(), info );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction"/> class.
        /// </summary>
        /// <param name="channelTypeMediumValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntity">The component entity.</param>
        /// <param name="info">
        /// The information about the <see cref="Interaction"/> object graph to be logged.
        /// In the case of conflicting values (i.e. channelEntity.Id vs info.ChannelEntityId), any values explicitly set on the <paramref name="info"/> parameter will take precedence.
        /// </param>
        public InteractionTransaction( DefinedValueCache channelTypeMediumValue, IEntityCache channelEntity, IEntityCache componentEntity, InteractionTransactionInfo info )
        {
            if ( channelTypeMediumValue == null || channelEntity == null || componentEntity == null )
            {
                return;
            }

            Initialize( channelTypeMediumValue.Id, channelEntity.Id, channelEntity.ToString(), channelEntity.CachedEntityTypeId, componentEntity.Id, componentEntity.ToString(), info );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction"/> class.
        /// </summary>
        /// <param name="info">The information about the <see cref="Interaction"/> object graph to be logged.</param>
        public InteractionTransaction( InteractionTransactionInfo info )
        {
            Initialize( info );
        }

        /// <summary>
        /// Merge the supplied arguments, giving precedence to the info object for any conflicting values.
        /// </summary>
        /// <param name="channelTypeMediumValueId">The channel type medium value identifier.</param>
        /// <param name="channelEntityId">The channel entity identifier.</param>
        /// <param name="channelName">Name of the channel.</param>
        /// <param name="componentEntityTypeId">The component entity type identifier.</param>
        /// <param name="componentEntityId">The component entity identifier.</param>
        /// <param name="componentName">Name of the component.</param>
        /// <param name="info">The information about the <see cref="Interaction"/> object graph to be logged.</param>
        private void Initialize( int channelTypeMediumValueId, int channelEntityId, string channelName, int componentEntityTypeId, int componentEntityId, string componentName, InteractionTransactionInfo info )
        {
            info = info ?? new InteractionTransactionInfo();

            info.ChannelTypeMediumValueId = info.ChannelTypeMediumValueId > 0 ? info.ChannelTypeMediumValueId : channelTypeMediumValueId;
            info.ChannelEntityId = info.ChannelEntityId > 0 ? info.ChannelEntityId : channelEntityId;
            info.ChannelName = info.ChannelName.IsNotNullOrWhiteSpace() ? info.ChannelName : channelName;
            info.ComponentEntityTypeId = info.ComponentEntityTypeId > 0 ? info.ComponentEntityTypeId : componentEntityTypeId;
            info.ComponentEntityId = info.ComponentEntityId > 0 ? info.ComponentEntityId : componentEntityId;
            info.ComponentName = info.ComponentName.IsNotNullOrWhiteSpace() ? info.ComponentName : componentName;

            Initialize( info );
        }

        /// <summary>
        /// Further configure this info object based on the options specified by the caller, then enqueue it for bulk insertion.
        /// </summary>
        private void Initialize( InteractionTransactionInfo info )
        {
            if ( info == null )
            {
                return;
            }

            info.Initialize( InteractionInfoQueue );
        }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            // Dequeue any interactions that have been queued and not processed up to this point.
            var interactionTransactionInfos = new List<InteractionTransactionInfo>();
            while ( InteractionInfoQueue.TryDequeue( out InteractionTransactionInfo interactionTransactionInfo ) )
            {
                interactionTransactionInfos.Add( interactionTransactionInfo );
            }

            if ( !interactionTransactionInfos.Any() )
            {
                // If all the interactions have been processed, exit.
                return;
            }

            // Get the distinct list of user agent strings within the interactions to be logged.
            var userAgentsLookup = interactionTransactionInfos.Where(a => a.UserAgent.IsNotNullOrWhiteSpace()).Select( a => a.UserAgent ).Distinct().ToList().ToDictionary( a => a, v => InteractionDeviceType.GetClientType( v ) );

            // Include/exclude crawlers based on caller input.
            interactionTransactionInfos = interactionTransactionInfos.Where( a => a.LogCrawlers || a.UserAgent.IsNullOrWhiteSpace() || userAgentsLookup.GetValueOrNull( a.UserAgent ) != "Crawler" ).ToList();

            if ( !interactionTransactionInfos.Any() )
            {
                // If there aren't interactions after considering whether to remove crawlers, exit.
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                LogInteractions( interactionTransactionInfos, rockContext );
            }
        }

        /// <summary>
        /// Logs the interactions.
        /// </summary>
        /// <param name="interactionTransactionInfos">The interaction transaction infos to process.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LogInteractions( List<InteractionTransactionInfo> interactionTransactionInfos, RockContext rockContext )
        {
            List<Interaction> interactionsToInsert = new List<Interaction>();

            var interactionService = new InteractionService( rockContext );

            foreach ( var info in interactionTransactionInfos.Where( a => a.InteractionComponentId.HasValue ) )
            {
                /*
                 * 2020-06-29 - JH
                 *
                 * The 'CreateInteraction(...)' method called below sets the following properties on the Interaction object:
                 *
                 * - InteractionComponentId
                 * - InteractionDateTime (but with the wrong value)
                 * - InteractionSessionId
                 * - Source
                 * - Medium
                 * - Campaign
                 * - Content
                 * - Term
                 */
                var interaction = interactionService.CreateInteraction( info.InteractionComponentId.Value, info.UserAgent, info.InteractionData, info.IPAddress, info.BrowserSessionId );

                // The rest of the properties need to be manually set.
                interaction.EntityId = info.InteractionEntityId;
                interaction.Operation = info.InteractionOperation.IsNotNullOrWhiteSpace() ? info.InteractionOperation.Trim() : "View";
                interaction.InteractionSummary = info.InteractionSummary?.Trim();
                interaction.PersonAliasId = info.PersonAliasId;
                interaction.InteractionDateTime = info.InteractionDateTime;
                interaction.InteractionTimeToServe = info.InteractionTimeToServe;
                interaction.RelatedEntityTypeId = info.InteractionRelatedEntityTypeId;
                interaction.RelatedEntityId = info.InteractionRelatedEntityId;
                interaction.ChannelCustom1 = info.InteractionChannelCustom1?.Trim();
                interaction.ChannelCustom2 = info.InteractionChannelCustom2?.Trim();
                interaction.ChannelCustomIndexed1 = info.InteractionChannelCustomIndexed1?.Trim();

                interaction.SetInteractionData( info.InteractionData?.Trim() );
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
    /// A class to represent the information about the <see cref="Interaction"/> object graph to be logged.
    /// </summary>
    public class InteractionTransactionInfo
    {
        #region InteractionChannel Properties

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
        public int? ComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the interaction entity type identifier.
        /// </summary>
        /// <value>
        /// The interaction entity type identifier.
        /// </value>
        public int? InteractionEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the channel entity identifier.
        /// </summary>
        /// <value>
        /// The channel entity identifier.
        /// </value>
        public int? ChannelEntityId { get; set; }

        /// <summary>
        /// Gets or sets the channel type medium value identifier.
        /// </summary>
        /// <value>
        /// The channel type medium value identifier.
        /// </value>
        public int? ChannelTypeMediumValueId { get; set; }

        #endregion InteractionChannel Properties

        #region InteractionComponent Properties

        /// <summary>
        /// Gets or sets the name of the component.
        /// </summary>
        /// <value>
        /// The name of the component.
        /// </value>
        public string ComponentName { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel identifier.
        /// </summary>
        /// <value>
        /// The interaction channel identifier.
        /// </value>
        public int InteractionChannelId { get; private set; }

        /// <summary>
        /// Gets or sets the component entity identifier.
        /// </summary>
        /// <value>
        /// The component entity identifier.
        /// </value>
        public int? ComponentEntityId { get; set; }

        #endregion InteractionComponent Properties

        #region InteractionSession Properties

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets or sets the browser session identifier.
        /// </summary>
        /// <value>
        /// The browser session identifier.
        /// </value>
        public Guid? BrowserSessionId { get; set; }

        #endregion InteractionSession Properties

        #region InteractionDeviceType Properties

        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        /// <value>
        /// The user agent.
        /// </value>
        public string UserAgent { get; set; }

        #endregion InteractionDeviceType Properties

        #region Interaction Properties

        /// <summary>
        /// Gets or sets the interaction date time.
        /// </summary>
        /// <value>
        /// The interaction date time.
        /// </value>
        public DateTime InteractionDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the interaction operation.
        /// </summary>
        /// <value>
        /// The interaction operation.
        /// </value>
        public string InteractionOperation { get; set; }

        /// <summary>
        /// Gets or sets the interaction component identifier.
        /// </summary>
        /// <value>
        /// The interaction component identifier.
        /// </value>
        public int? InteractionComponentId { get; private set; }

        /// <summary>
        /// Gets or sets the interaction entity identifier.
        /// </summary>
        /// <value>
        /// The interaction entity identifier.
        /// </value>
        public int? InteractionEntityId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the interaction summary.
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        public string InteractionSummary { get; set; }

        /// <summary>
        /// Gets or sets the interaction data.
        /// </summary>
        /// <value>
        /// The interaction data.
        /// </value>
        public string InteractionData { get; set; }

        /// <summary>
        /// Gets or sets the interaction related entity type identifier.
        /// </summary>
        /// <value>
        /// The interaction related entity type identifier.
        /// </value>
        public int? InteractionRelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the interaction related entity identifier.
        /// </summary>
        /// <value>
        /// The interaction related entity identifier.
        /// </value>
        public int? InteractionRelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel custom 1.
        /// </summary>
        /// <value>
        /// The interaction channel custom 1.
        /// </value>
        public string InteractionChannelCustom1 { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel custom 2.
        /// </summary>
        /// <value>
        /// The interaction channel custom 2.
        /// </value>
        public string InteractionChannelCustom2 { get; set; }

        /// <summary>
        /// Gets or sets the interaction channel custom indexed1.
        /// </summary>
        /// <value>
        /// The interaction channel custom indexed1.
        /// </value>
        public string InteractionChannelCustomIndexed1 { get; set; }

        /// <summary>
        /// Gets or sets the interaction time to serve. 
        /// The units on this depend on the InteractionChannel, which might have this be a Percent, Days, Seconds, Minutes, etc.
        /// For example, if this is a page view, this would be how long (in seconds) it took for Rock to generate a response.
        /// </summary>
        /// <value>
        /// The interaction time to serve.
        /// </value>
        public double? InteractionTimeToServe { get; set; }

        #endregion

        #region Helper Properties

        /// <summary>
        /// Gets or sets a value indicating whether [get values from HTTP request].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [get values from HTTP request]; otherwise, <c>false</c>.
        /// </value>
        public bool GetValuesFromHttpRequest { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [log crawlers].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [log crawlers]; otherwise, <c>false</c>.
        /// </value>
        public bool LogCrawlers { get; set; }

        #endregion Helper Properties

        /// <summary>
        /// Further configure this info object based on the options specified by the caller, then enqueue it to be bulk inserted.
        /// </summary>
        /// <param name="queue">The <see cref="ConcurrentQueue{InterationInfo}"/> into which this info object should be enqueued.</param>
        public void Initialize( ConcurrentQueue<InteractionTransactionInfo> queue )
        {
            RockPage rockPage = null;
            HttpRequest request = null;

            if ( this.GetValuesFromHttpRequest )
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
                    // Intentionally ignore exception (.Request will throw an exception instead of simply returning null if it isn't available).
                }
            }

            // Fall back to values from the HTTP request if specified by the caller AND the values weren't explicitly set on the info object:
            // (The rockPage and request variables will only be defined if this.GetValuesFromHttpRequest was true.)

            this.InteractionData = this.InteractionData ?? request?.Url.ToString();
            this.UserAgent = this.UserAgent ?? request?.UserAgent;

            try
            {
                this.IPAddress = this.IPAddress ?? RockPage.GetClientIpAddress();
            }
            catch
            {
                this.IPAddress = string.Empty;
            }

            this.BrowserSessionId = this.BrowserSessionId ?? rockPage?.Session["RockSessionId"]?.ToString().AsGuidOrNull();

            this.PersonAliasId = this.PersonAliasId ?? rockPage?.CurrentPersonAliasId;

            // Make sure we don't exceed this field's character limit.
            this.InteractionOperation = EnforceLengthLimitation( this.InteractionOperation, 25 );

            if ( this.InteractionSummary.IsNullOrWhiteSpace() )
            {
                // If InteractionSummary was not specified, use the Page title.
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

                // Remove the site name from the title.
                if ( title?.Contains( "|" ) == true )
                {
                    title = title.Substring( 0, title.LastIndexOf( '|' ) ).Trim();
                }

                this.InteractionSummary = title;
            }

            // Make sure we don't exceed these fields' character limit.
            this.InteractionSummary = EnforceLengthLimitation( this.InteractionSummary, 500 );
            this.InteractionChannelCustom1 = EnforceLengthLimitation( this.InteractionChannelCustom1, 500 );
            this.InteractionChannelCustom2 = EnforceLengthLimitation( this.InteractionChannelCustom2, 2000 );
            this.InteractionChannelCustomIndexed1 = EnforceLengthLimitation( this.InteractionChannelCustomIndexed1, 500 );

            // Get existing (or create new) interaction channel and interaction component for this interaction.
            this.InteractionChannelId = InteractionChannelCache.GetChannelIdByTypeIdAndEntityId( this.ChannelTypeMediumValueId, this.ChannelEntityId, this.ChannelName, this.ComponentEntityTypeId, this.InteractionEntityTypeId );
            this.InteractionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( this.InteractionChannelId, this.ComponentEntityId, this.ComponentName );

            queue.Enqueue( this );
        }

        /// <summary>
        /// Enforces the length limitation.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="limit">The limit.</param>
        /// <returns></returns>
        private string EnforceLengthLimitation( string input, int limit )
        {
            input = input?.Trim();

            if ( input != null && input.Length > limit )
            {
                return input.LeftWithEllipsis( limit - 1 );
            }

            return input;
        }
    }
}