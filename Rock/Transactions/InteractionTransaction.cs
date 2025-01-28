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
    /// Ít will remain using the ITransaction interface and not be converted to a bus event yet
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
            if ( channelTypeMediumValue == null || componentEntity == null )
            {
                return;
            }

            if ( info?.InteractionChannelId == default && channelEntity == null )
            {
                // we need either an InteractionChannelId or a channelEntity
                return;
            }

            // NOTE: Just in case this seem confusing, the EntityType of ChannelEntity tells us what the *component* entity type id is!
            var componentEntityTypeId = channelEntity?.TypeId;

            Initialize( channelTypeMediumValue.Id, channelEntity?.Id, channelEntity?.ToString(), componentEntityTypeId, componentEntity.Id, componentEntity.ToString(), info );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction" /> class.
        /// </summary>
        /// <param name="channelTypeMediumValue">The channel medium type value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntityCache">The component entity cache.</param>
        /// <param name="info">The information about the <see cref="Interaction" /> object graph to be logged.
        /// In the case of conflicting values (i.e. channelEntity.Id vs info.ChannelEntityId), any values explicitly set on the <paramref name="info" /> parameter will take precedence.</param>
        public InteractionTransaction( DefinedValueCache channelTypeMediumValue, IEntityCache channelEntity, IEntityCache componentEntityCache, InteractionTransactionInfo info )
        {
            if ( channelTypeMediumValue == null || componentEntityCache == null )
            {
                return;
            }

            if ( info?.InteractionChannelId == default && channelEntity == null )
            {
                // we need either an InteractionChannelId or a channelEntity
                return;
            }

            // NOTE: Just in case this seem confusing, the EntityType of ChannelEntity tells us what the *component* entity type id is!
            var componentEntityTypeId = channelEntity?.CachedEntityTypeId;

            Initialize( channelTypeMediumValue.Id, channelEntity?.Id, channelEntity?.ToString(), componentEntityTypeId, componentEntityCache.Id, componentEntityCache.ToString(), info );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionTransaction" /> class.
        /// </summary>
        /// <param name="channelTypeMediumValue">The channel type medium value.</param>
        /// <param name="channelEntity">The channel entity.</param>
        /// <param name="componentEntityCache">The component entity cache.</param>
        /// <param name="info">The information.</param>
        public InteractionTransaction( DefinedValueCache channelTypeMediumValue, IEntity channelEntity, IEntityCache componentEntityCache, InteractionTransactionInfo info )
        {
            if ( channelTypeMediumValue == null || componentEntityCache == null )
            {
                return;
            }

            if ( info?.InteractionChannelId == default && channelEntity == null )
            {
                // we need either an InteractionChannelId or a channelEntity
                return;
            }

            // NOTE: Just in case this seem confusing, the EntityType of ChannelEntity tells us what the *component* entity type id is!
            var componentEntityTypeId = channelEntity?.TypeId;

            Initialize( channelTypeMediumValue.Id, channelEntity?.Id, channelEntity?.ToString(), componentEntityTypeId, componentEntityCache.Id, componentEntityCache.ToString(), info );
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
        private void Initialize( int channelTypeMediumValueId, int? channelEntityId, string channelName, int? componentEntityTypeId, int componentEntityId, string componentName, InteractionTransactionInfo info )
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
            var userAgentsLookup = interactionTransactionInfos.Where( a => a.UserAgent.IsNotNullOrWhiteSpace() ).Select( a => a.UserAgent ).Distinct().ToList().ToDictionary( a => a, v => InteractionDeviceType.GetClientType( v ) );

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
                interaction.Guid = info.InteractionGuid ?? Guid.NewGuid();
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
                interaction.Content = interaction.Content ?? info.InteractionContent?.Trim();
                interaction.Term = interaction.Term ?? info.InteractionTerm?.Trim();
                interaction.InteractionLength = info.InteractionLength;
                interaction.InteractionEndDateTime = info.InteractionEndDateTime;

                interaction.SetUtmSource( interaction.Source.IsNotNullOrWhiteSpace() ? interaction.Source : info.InteractionSource );
                interaction.SetUtmMedium( interaction.Medium.IsNotNullOrWhiteSpace() ? interaction.Medium : info.InteractionMedium );
                interaction.SetUtmCampaign( interaction.Campaign.IsNotNullOrWhiteSpace() ? interaction.Campaign : info.InteractionCampaign );

                interaction.SetInteractionData( info.InteractionData?.Trim() );
                interactionsToInsert.Add( interaction );
            }

            /*
                1/14/2025 - KBH

                Added code to check for duplicate Guids before inserting into the Interaction table.
                    1. Check for any duplicate interaction Guids within our current list of interactions
                       to insert.
                    2. Check for any interaction guids in the database that match any of the interaction
                       guids we are attempting to insert.

                REASON FOR THE CODE:
                    A malicious user can send multiple requests to the RegisterPageInteraction route. If
                    the requests made by the malicious user contain duplicate interaction Guids, the
                    attempt to Bulk Insert will fail and rollback. Any queued interactions (good or bad)
                    will be lost.

                    This added code may also prevent similar (duplicate guid) scenarios when a Web Farm
                    is introduced. 
             */

            // Remove any duplicate interactions within the current list of interactions to insert.
            interactionsToInsert = interactionsToInsert
                .DistinctBy( i => i.Guid )
                .ToList();

            // Cross checking to verify that Guids aren't present in the interaction table.
            var interactionGuidsToInsert = interactionsToInsert.Select( i => i.Guid ).ToList();
            var duplicateInteractionGuidsFromDatabase = new InteractionService( new RockContext() ).Queryable()
                                    .Where( i => interactionGuidsToInsert.Contains( i.Guid ) )
                                    .Select( i => i.Guid )
                                    .ToList();
            interactionsToInsert.RemoveAll( a => duplicateInteractionGuidsFromDatabase.Contains( a.Guid ) );

            rockContext.BulkInsert( interactionsToInsert );

            // This logic is normally handled in the Interaction.PostSave method, but since the BulkInsert bypasses those
            // model hooks, streaks need to be updated here. Also, it is not necessary for this logic to complete before this
            // transaction can continue processing and exit, so update the streak using a task.

            // Only launch this task if there are StreakTypes configured that have interactions. Otherwise several
            // database calls are made only to find out there are no streak types defined.
            if ( StreakTypeCache.All().Any( s => s.IsInteractionRelated ) )
            {
                // Ids do not exit for the interactions in the collection since they were bulk imported.
                // Read their ids from their guids and append the id.
                var insertedGuids = interactionsToInsert.Select( i => i.Guid ).ToList();

                var interactionIds = new InteractionService( new RockContext() ).Queryable()
                                        .Where( i => insertedGuids.Contains( i.Guid ) )
                                        .Select( i => new { i.Id, i.Guid } )
                                        .ToList();

                foreach ( var interactionId in interactionIds )
                {
                    var interaction = interactionsToInsert.Where( i => i.Guid == interactionId.Guid ).FirstOrDefault();
                    if ( interaction != null )
                    {
                        interaction.Id = interactionId.Id;
                    }
                }

                // Launch task
                interactionsToInsert.ForEach( i => Task.Run( () => StreakTypeService.HandleInteractionRecord( i.Id ) ) );
            }
        }
    }

    /// <summary>
    /// A class to represent the information about the <see cref="Interaction"/> object graph to be logged.
    /// </summary>
    public class InteractionTransactionInfo
    {
        #region InteractionChannel Properties

        /// <inheritdoc cref="InteractionChannel.Name"/>
        public string ChannelName { get; set; }

        /// <inheritdoc cref="InteractionChannel.ComponentEntityTypeId"/>
        public int? ComponentEntityTypeId { get; set; }

        /// <inheritdoc cref="InteractionChannel.InteractionEntityTypeId"/>
        public int? InteractionEntityTypeId { get; set; }

        /// <inheritdoc cref="InteractionChannel.ChannelEntityId"/>
        public int? ChannelEntityId { get; set; }

        /// <inheritdoc cref="InteractionChannel.ChannelTypeMediumValueId"/>
        public int? ChannelTypeMediumValueId { get; set; }

        #endregion InteractionChannel Properties

        #region InteractionComponent Properties

        /// <inheritdoc cref="InteractionComponent.Name"/>
        public string ComponentName { get; set; }

        /// <summary>
        /// <inheritdoc cref="InteractionComponent.InteractionChannelId"/>
        /// </summary>
        /// <remarks>
        /// If this is not set, it will be determined from <seealso cref="InteractionChannelCache.GetChannelIdByTypeIdAndEntityId"/>
        /// </remarks>
        /// <value>
        /// The interaction channel identifier.
        /// </value>
        public int InteractionChannelId { get; set; }

        /// <inheritdoc cref="InteractionComponent.EntityId"/>
        public int? ComponentEntityId { get; set; }

        #endregion InteractionComponent Properties

        #region InteractionSession Properties

        /// <inheritdoc cref="InteractionSession.IpAddress"/>
        ///<remarks>
        /// If this is not specified, it will be determined from <see cref="RockPage.GetClientIpAddress()"/>
        ///</remarks>
        public string IPAddress { get; set; }

        /// <summary>
        /// The <c>RockSessionId</c> (Guid) (set in Global.Session_Start)
        /// </summary>
        /// <remarks>
        /// This usually doesn't need to be set, the default is the RockSessionId.
        /// </remarks>
        /// <value>
        /// The browser session identifier.
        /// </value>
        public Guid? BrowserSessionId { get; set; }

        #endregion InteractionSession Properties

        #region InteractionDeviceType Properties

        /// <summary>
        /// The UserAgent value that will used for the Interaction.
        /// </summary>
        /// <remarks>
        /// If this is not specified, it will be determined from the current <see cref="HttpRequest"/>.
        /// </remarks>
        /// <value>
        /// The user agent.
        /// </value>
        public string UserAgent { get; set; }

        #endregion InteractionDeviceType Properties

        #region Interaction Properties

        /// <inheritdoc cref="IEntity.Guid"/>
        /// <remarks>
        /// If this is not specified then a new Guid will be created.
        /// </remarks>
        public Guid? InteractionGuid { get; set; }

        /// <inheritdoc cref="Interaction.InteractionDateTime"/>
        /// <remarks>
        /// If this is not specified, <see cref="RockDateTime.Now"/> will be used.
        /// </remarks>
        public DateTime InteractionDateTime { get; set; } = RockDateTime.Now;

        /// <inheritdoc cref="Interaction.InteractionLength"/>
        public double? InteractionLength { get; set; }

        /// <inheritdoc cref="Interaction.InteractionEndDateTime"/>
        public DateTime? InteractionEndDateTime { get; set; }

        /// <inheritdoc cref="Interaction.Operation"/>
        /// <remarks>
        /// If this is not specified, it will set to 'View'.
        /// </remarks>
        public string InteractionOperation { get; set; }

        /// <inheritdoc cref="Interaction.InteractionComponentId"/>
        /// <remarks>
        /// This will be determined automatically from <seealso cref="InteractionComponentCache.GetComponentIdByChannelIdAndEntityId"/> 
        /// </remarks>
        public int? InteractionComponentId { get; private set; }

        /// <inheritdoc cref="Interaction.EntityId"/>
        public int? InteractionEntityId { get; set; }

        /// <summary>
        /// The PersonAliasId that will be saved to <seealso cref="Interaction.PersonAliasId"/>.
        /// </summary>
        /// <remarks>
        /// If this is not specified, it will be determined from <see cref="RockPage.CurrentPersonAliasId"/> or <see cref="RockPage.CurrentVisitor"/>.
        /// </remarks>
        /// <value>The person alias identifier.</value>
        public int? PersonAliasId { get; set; }

        /// <inheritdoc cref="Interaction.InteractionSummary"/>
        /// <remarks>
        /// If this is not set, it will be determined from <see cref="RockPage.BrowserTitle"/> or <see cref="RockPage.Title"/>.
        /// </remarks>
        public string InteractionSummary { get; set; }

        /// <inheritdoc cref="Interaction.InteractionData"/>
        /// <remarks>
        /// If this is not specified, it be determined from current request's URL.
        /// </remarks>
        public string InteractionData { get; set; }

        /// <inheritdoc cref="Interaction.RelatedEntityTypeId"/>
        public int? InteractionRelatedEntityTypeId { get; set; }

        /// <inheritdoc cref="Interaction.RelatedEntityId"/>
        public int? InteractionRelatedEntityId { get; set; }

        /// <inheritdoc cref="Interaction.ChannelCustom1"/>
        public string InteractionChannelCustom1 { get; set; }

        /// <inheritdoc cref="Interaction.ChannelCustom2"/>
        public string InteractionChannelCustom2 { get; set; }

        /// <inheritdoc cref="Interaction.ChannelCustomIndexed1"/>
        public string InteractionChannelCustomIndexed1 { get; set; }

        /// <inheritdoc cref="Interaction.InteractionTimeToServe"/>
        public double? InteractionTimeToServe { get; set; }

        /// <inheritdoc cref="Interaction.Source"/>
        public string InteractionSource { get; set; }

        /// <inheritdoc cref="Interaction.Medium"/>
        public string InteractionMedium { get; set; }

        /// <inheritdoc cref="Interaction.Campaign"/>
        public string InteractionCampaign { get; set; }

        /// <inheritdoc cref="Interaction.Content"/>
        public string InteractionContent { get; set; }

        /// <inheritdoc cref="Interaction.Term"/>
        public string InteractionTerm { get; set; }

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
                    rockPage = HttpContext.Current?.Handler as RockPage;
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

            this.InteractionData = this.InteractionData ?? request?.UrlProxySafe().ToString();
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

            this.PersonAliasId = this.PersonAliasId ?? rockPage?.CurrentPersonAliasId ?? rockPage?.CurrentVisitor?.Id;

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
            this.InteractionSource = EnforceLengthLimitation( this.InteractionSource, 25 );
            this.InteractionMedium = EnforceLengthLimitation( this.InteractionMedium, 25 );
            this.InteractionCampaign = EnforceLengthLimitation( this.InteractionCampaign, 50 );
            this.InteractionContent = EnforceLengthLimitation( this.InteractionContent, 50 );
            this.InteractionTerm = EnforceLengthLimitation( this.InteractionTerm, 50 );

            // Get existing (or create new) interaction channel and interaction component for this interaction.
            if ( this.InteractionChannelId == default )
            {
                if ( this.ChannelName != null && this.ChannelEntityId == null )
                {
                    // If channel name is specified and entity is not, get the channel by name.
                    this.InteractionChannelId = InteractionChannelCache.GetOrCreateChannelIdByName( this.ChannelTypeMediumValueId.GetValueOrDefault(), this.ChannelName, this.ComponentEntityTypeId, this.InteractionEntityTypeId );
                }
                else
                {
                    this.InteractionChannelId = InteractionChannelCache.GetChannelIdByTypeIdAndEntityId( this.ChannelTypeMediumValueId, this.ChannelEntityId, this.ChannelName, this.ComponentEntityTypeId, this.InteractionEntityTypeId );
                }
            }

            if ( this.ComponentName != null && this.ComponentEntityId == null )
            {
                this.InteractionComponentId = InteractionComponentCache.GetOrCreateComponentIdByName( this.InteractionChannelId, this.ComponentName );
            }
            else
            {
                this.InteractionComponentId = InteractionComponentCache.GetComponentIdByChannelIdAndEntityId( this.InteractionChannelId, this.ComponentEntityId, this.ComponentName );
            }

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