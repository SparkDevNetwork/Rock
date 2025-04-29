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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.CommunicationList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Lists the status of all previously created communications.
    /// </summary>

    [DisplayName( "Communication List" )]
    [Category( "Communication" )]
    [Description( "Lists the status of all previously created communications." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "The page that will show the communication details.",
        IsRequired = true )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "e4bd5cad-579e-476d-87ec-989de975bb60" )]
    [Rock.SystemGuid.BlockTypeGuid( "c3544f53-8e2d-43d6-b165-8fefc541a4eb" )]
    [CustomizedGrid]
    public class CommunicationList : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string Communication = "Communication";
        }

        private static class PreferenceKey
        {
            public const string FilterCreatedBy = "filter-created-by";
            public const string FilterCommunicationTypes = "filter-communication-types";
            public const string FilterHideDrafts = "filter-hide-drafts";
            public const string FilterSendDateRange = "filter-send-date-range";
            public const string FilterRecipientCountLower = "filter-recipient-count-lower";
            public const string FilterRecipientCountUpper = "filter-recipient-count-upper";
            public const string FilterTopic = "filter-topic";
            public const string FilterName = "filter-name";
            public const string FilterContent = "filter-content";
        }

        private static class SqlParamKey
        {
            public const string SenderPersonAliasGuid = "@SenderPersonAliasGuid";
            public const string SendDateTimeStart = "@SendDateTimeStart";
            public const string SendDateTimeEnd = "@SendDateTimeEnd";
            public const string RecipientCountLower = "@RecipientCountLower";
            public const string RecipientCountUpper = "@RecipientCountUpper";
            public const string TopicValueGuid = "@TopicValueGuid";
            public const string Name = "@Name";
            public const string Content = "@Content";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the unique identifier of the "created by" <see cref="PersonAlias"/> by whom to filter the results.
        /// </summary>
        private Guid? FilterCreatedByPersonAliasGuid => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterCreatedBy )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the list of <see cref="CommunicationType"/> integer values by which to filter the results.
        /// </summary>
        private List<int> FilterCommunicationTypes => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterCommunicationTypes )
            .SplitDelimitedValues()
            .Select( t => t.AsIntegerOrNull() )
            .Where( t => t.HasValue )
            .Select( t => t.Value )
            .ToList();

        /// <summary>
        /// Gets whether to hide results whose status is <see cref="CommunicationStatus.Draft"/>.
        /// </summary>
        private bool FilterHideDrafts => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterHideDrafts )
            .AsBoolean();

        /// <summary>
        /// Gets the send date range by which to filter the results.
        /// </summary>
        private SlidingDateRangeBag FilterSendDateRange => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterSendDateRange )
            .ToSlidingDateRangeBagOrNull();

        /// <summary>
        /// Gets the lower recipient count limit by which to filter the results.
        /// </summary>
        private int? FilterRecipientCountLower => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterRecipientCountLower )
            .AsIntegerOrNull();

        /// <summary>
        /// Gets the upper recipient count limit by which to filter the results.
        /// </summary>
        private int? FilterRecipientCountUpper => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterRecipientCountUpper )
            .AsIntegerOrNull();

        /// <summary>
        /// Gets the unique identifier of the Topic <see cref="DefinedValue"/> by which to filter the results.
        /// </summary>
        private Guid? FilterTopicValueGuid => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterTopic )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the name by which to filter the results.
        /// </summary>
        private string FilterName => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterName );

        /// <summary>
        /// Gets the content by which to filter the results.
        /// </summary>
        private string FilterContent => BlockPersonPreferences
            .GetValue( PreferenceKey.FilterContent );

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CommunicationListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = 100;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Gets the communication list grid data.
        /// </summary>
        /// <returns>A bag containing the communication list grid data.</returns>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            var sqlSb = new StringBuilder();

            // Default to the last 6 months if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 6
            };

            var sendDateRange = FilterSendDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var sendDateTimeStart = sendDateRange.Start;
            var sendDateTimeEnd = sendDateRange.End;

            var daysBetween = ( sendDateTimeStart.HasValue && sendDateTimeEnd.HasValue )
                ? ( int? ) ( sendDateTimeEnd.Value.Date - sendDateTimeStart.Value.Date ).TotalDays
                : null;

            var sqlParams = new List<SqlParameter>
            {
                new SqlParameter( SqlParamKey.SendDateTimeStart, sendDateTimeStart.Value ),
                new SqlParameter( SqlParamKey.SendDateTimeEnd, sendDateTimeEnd.Value ),
                new SqlParameter( SqlParamKey.RecipientCountLower, ( object ) FilterRecipientCountLower ?? DBNull.Value ),
                new SqlParameter( SqlParamKey.RecipientCountUpper, ( object ) FilterRecipientCountUpper ?? DBNull.Value )
            };

            var senderPersonAliasGuid = GetCanApprove()
                ? FilterCreatedByPersonAliasGuid        // Show the communications created by the selected person or all communication if no person is selected.
                : GetCurrentPerson().PrimaryAliasGuid;  // Only show the current person's communications.

            if ( senderPersonAliasGuid.HasValue )
            {
                // We'll join against the corresponding person ID to ensure we included merged person records.
                sqlSb.AppendLine( $@"DECLARE @SenderPersonId INT = (SELECT TOP 1 [PersonId] FROM [PersonAlias] WHERE [Guid] = {SqlParamKey.SenderPersonAliasGuid});" );
                sqlParams.Add( new SqlParameter( SqlParamKey.SenderPersonAliasGuid, senderPersonAliasGuid.Value ) );
            }

            // Build a dynamic SQL query to project only the needed data into a custom POCO.
            sqlSb.AppendLine( $@"
;WITH CommunicationAggregate AS (
    SELECT c.[Id]
        , c.[Guid]
        , c.[CommunicationTemplateId]
        , c.[SystemCommunicationId]
        , c.[CommunicationType] AS [Type]
        , CASE
            WHEN c.[Subject] IS NOT NULL AND c.[Subject] <> '' THEN c.[Subject]
            WHEN c.[PushTitle] IS NOT NULL AND c.[PushTitle] <> '' THEN c.[PushTitle]
            ELSE c.[Name]
          END AS [Name]
        , c.[Summary]
        , c.[Status]
        , c.[CommunicationTopicValueId] AS [TopicValueId]
        , c.[SendDateTime]
        , c.[FutureSendDateTime]
        , c.[SenderPersonAliasId]
        , c.[ReviewedDateTime]
        , c.[ReviewerPersonAliasId]
        , COUNT(cr.[Id]) AS [RecipientCount]
        , ISNULL(SUM(CASE WHEN cr.[Status] = {CommunicationRecipientStatus.Pending.ConvertToInt()} THEN 1 END),0) AS [PendingCount]
        , ISNULL(SUM(CASE WHEN cr.[Status] IN ({CommunicationRecipientStatus.Delivered.ConvertToInt()}, {CommunicationRecipientStatus.Opened.ConvertToInt()}) THEN 1 END), 0) AS [DeliveredCount]
        , ISNULL(SUM(CASE WHEN cr.[Status] = {CommunicationRecipientStatus.Opened.ConvertToInt()} THEN 1 END), 0) AS [OpenedCount]
        , ISNULL(SUM(CASE WHEN cr.[Status] = {CommunicationRecipientStatus.Failed.ConvertToInt()} THEN 1 END), 0) AS [FailedCount]
        , ISNULL(SUM(CASE WHEN cr.[UnsubscribeDateTime] IS NOT NULL THEN 1 END), 0) AS [UnsubscribedCount]
        , CASE
            WHEN c.[Status] = {CommunicationStatus.Draft.ConvertToInt()}
                AND c.[SendDateTime] IS NULL
                AND c.[FutureSendDateTime] IS NULL
            THEN 1
            ELSE 0
          END AS [IsDraftWithoutSendDate]
    FROM [Communication] c
    LEFT OUTER JOIN [CommunicationRecipient] cr ON cr.[CommunicationId] = c.[Id]
    LEFT OUTER JOIN [PersonAlias] paSender ON paSender.[Id] = c.[SenderPersonAliasId]
    LEFT OUTER JOIN [Person] pSender ON pSender.[Id] = paSender.[PersonId]
    LEFT OUTER JOIN [DefinedValue] dvTopic ON dvTopic.[Id] = c.[CommunicationTopicValueId]
    WHERE c.[Status] <> 0" ); // Always ignore Transient records.

            if ( FilterHideDrafts )
            {
                sqlSb.AppendLine( $@"        AND c.[Status] <> {CommunicationStatus.Draft.ConvertToInt()}
        AND COALESCE(c.[SendDateTime], c.[FutureSendDateTime]) >= {SqlParamKey.SendDateTimeStart}
        AND COALESCE(c.[SendDateTime], c.[FutureSendDateTime]) < {SqlParamKey.SendDateTimeEnd}" );
            }
            else
            {
                sqlSb.AppendLine( $@"        AND (
            (
                /* Drafts might be missing both a [SendDateTime] and [FutureSendDateTime]. */
                c.[Status] = {CommunicationStatus.Draft.ConvertToInt()}
                AND c.[SendDateTime] IS NULL
                AND c.[FutureSendDateTime] IS NULL
            )
            OR (
                /* If a [SendDateTime] or [FutureSendDateTime] is provided, it must fall within the filtered range, even for drafts. */
                COALESCE(c.[SendDateTime], c.[FutureSendDateTime]) >= {SqlParamKey.SendDateTimeStart}
                AND COALESCE(c.[SendDateTime], c.[FutureSendDateTime]) < {SqlParamKey.SendDateTimeEnd}
            )
        )" );
            }

            if ( senderPersonAliasGuid.HasValue )
            {
                sqlSb.AppendLine( $"        AND pSender.[Id] = @SenderPersonId" );
            }

            if ( FilterCommunicationTypes.Any() )
            {
                sqlSb.AppendLine( $"        AND c.[CommunicationType] IN ({FilterCommunicationTypes.AsDelimited( ", " )})" );
            }

            if ( FilterTopicValueGuid.HasValue )
            {
                sqlSb.AppendLine( $"        AND dvTopic.[Guid] = {SqlParamKey.TopicValueGuid}" );
                sqlParams.Add( new SqlParameter( SqlParamKey.TopicValueGuid, FilterTopicValueGuid.Value ) );
            }

            if ( FilterName.IsNotNullOrWhiteSpace() )
            {
                sqlSb.AppendLine( $"        AND c.[Name] LIKE '%' + {SqlParamKey.Name} + '%'" );
                sqlParams.Add( new SqlParameter( SqlParamKey.Name, FilterName ) );
            }

            if ( FilterContent.IsNotNullOrWhiteSpace() )
            {
                sqlSb.AppendLine( $@"        AND (
            c.[Message] LIKE '%' + {SqlParamKey.Content} + '%'
            OR c.[SMSMessage] LIKE '%' + {SqlParamKey.Content} + '%'
            OR c.[PushMessage] LIKE '%' + {SqlParamKey.Content} + '%'
        )" );
                sqlParams.Add( new SqlParameter( SqlParamKey.Content, FilterContent ) );
            }

            sqlSb.Append( $@"    GROUP BY c.[Id]
        , c.[Guid]
        , c.[CommunicationTemplateId]
        , c.[SystemCommunicationId]
        , c.[CommunicationType]
        , c.[Subject]
        , c.[PushTitle]
        , c.[Name]
        , c.[Summary]
        , c.[Status]
        , c.[CommunicationTopicValueId]
        , c.[SendDateTime]
        , c.[FutureSendDateTime]
        , c.[SenderPersonAliasId]
        , c.[ReviewedDateTime]
        , c.[ReviewerPersonAliasId]
    HAVING
        (
            @RecipientCountLower IS NULL
            OR COUNT(cr.[Id]) >= @RecipientCountLower
        )
        AND (
            @RecipientCountUpper IS NULL
            OR COUNT(cr.[Id]) <= @RecipientCountUpper
        )
)
SELECT ca.*
    , CASE

        /* Runtime status value of `Sent (6)`: No Pending Recipients. */
        WHEN ca.[Status] = {CommunicationStatus.Approved.ConvertToInt()}
            AND ca.[PendingCount] = 0
        THEN 6

        /* Runtime status value of `Sending (5)`: Some Pending + Some Non-Pending Recipients. */
        WHEN ca.[Status] = {CommunicationStatus.Approved.ConvertToInt()}
            AND ca.[PendingCount] > 0
            AND ca.[RecipientCount] > ca.[PendingCount]
        THEN 5

        /* All other cases; use actual status value. */
        ELSE ca.[Status]

      END AS [InferredStatus]
    , pSender.[Id] AS [SenderPersonId]
    , pSender.[NickName] AS [SenderPersonNickName]
    , pSender.[LastName] AS [SenderPersonLastName]
    , pSender.[SuffixValueId] AS [SenderPersonSuffixValueId]
    , pSender.[RecordTypeValueId] AS [SenderRecordTypeValueId]
    , pReviewer.[Id] AS [ReviewerPersonId]
    , pReviewer.[NickName] AS [ReviewerPersonNickName]
    , pReviewer.[LastName] AS [ReviewerPersonLastName]
    , pReviewer.[SuffixValueId] AS [ReviewerPersonSuffixValueId]
    , pReviewer.[RecordTypeValueId] AS [ReviewerRecordTypeValueId]
FROM CommunicationAggregate ca
LEFT OUTER JOIN [PersonAlias] paSender ON paSender.[Id] = ca.[SenderPersonAliasId]
LEFT OUTER JOIN [Person] pSender ON pSender.[Id] = paSender.[PersonId]
LEFT OUTER JOIN [PersonAlias] paReviewer ON paReviewer.[Id] = ca.[ReviewerPersonAliasId]
LEFT OUTER JOIN [Person] pReviewer ON pReviewer.[Id] = paReviewer.[PersonId]
ORDER BY ca.[IsDraftWithoutSendDate] DESC
    , CASE WHEN ca.[IsDraftWithoutSendDate] = 1 THEN ca.[Id] ELSE NULL END DESC
    , COALESCE(ca.[SendDateTime], ca.[FutureSendDateTime]) DESC;" );

            var communicationRows = RockContext.Database
                .SqlQuery<CommunicationRow>( sqlSb.ToString(), sqlParams.ToArray() )
                .ToList();

            // Limit to only communications the current person is authorized to view. Communication security is based on
            // CommunicationTemplate and/or SystemCommunication.
            var communicationTemplateIds = communicationRows
                .Where( r => r.CommunicationTemplateId.HasValue )
                .Select( r => r.CommunicationTemplateId )
                .Distinct()
                .ToList();

            if ( communicationTemplateIds.Any() )
            {
                var authorizedCommunicationTemplateIds = new CommunicationTemplateService( RockContext )
                    .Queryable()
                    .Where( ct => communicationTemplateIds.Contains( ct.Id ) )
                    .ToList()
                    .Where( ct => ct.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                    .Select( ct => ct.Id )
                    .ToList();

                communicationRows = communicationRows
                    .Where( r =>
                        !r.CommunicationTemplateId.HasValue
                        || authorizedCommunicationTemplateIds.Contains( r.CommunicationTemplateId.Value )
                    )
                    .ToList();
            }

            var systemCommunicationIds = communicationRows
                .Where( r => r.SystemCommunicationId.HasValue )
                .Select( r => r.SystemCommunicationId )
                .Distinct()
                .ToList();

            if ( systemCommunicationIds.Any() )
            {
                var authorizedSystemCommunicationIds = new SystemCommunicationService( RockContext )
                    .Queryable()
                    .Where( sc => systemCommunicationIds.Contains( sc.Id ) )
                    .ToList()
                    .Where( sc => sc.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                    .Select( sc => sc.Id )
                    .ToList();

                communicationRows = communicationRows
                    .Where( r =>
                        !r.SystemCommunicationId.HasValue
                        || authorizedSystemCommunicationIds.Contains( r.SystemCommunicationId.Value )
                    )
                    .ToList();
            }

            var builder = GetGridBuilder();
            var gridDataBag = builder.Build( communicationRows );

            return ActionOk( gridDataBag );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var communicationService = new CommunicationService( RockContext );
            var communication = communicationService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( communication == null )
            {
                return ActionBadRequest( $"{Rock.Model.Communication.FriendlyTypeName} not found." );
            }

            if ( !communicationService.CanDelete( communication, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            communicationService.Delete( communication );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CommunicationListOptionsBag GetBoxOptions()
        {
            var options = new CommunicationListOptionsBag
            {
                ShowCreatedByFilter = GetCanApprove(),
                HasActiveEmailTransport = MediumContainer.HasActiveEmailTransport(),
                HasActiveSmsTransport = MediumContainer.HasActiveSmsTransport(),
                HasActivePushTransport = MediumContainer.HasActivePushTransport()
            };

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.Communication, "((Key))" )
            };
        }

        /// <summary>
        /// Gets the grid builder for the communication list grid.
        /// </summary>
        /// <returns>The grid builder for the communication list grid.</returns>
        private GridBuilder<CommunicationRow> GetGridBuilder()
        {
            return new GridBuilder<CommunicationRow>()
                .WithBlock( this )
                .AddField( "guid", a => a.Guid )
                .AddField( "type", a => a.Type )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "summary", a =>
                {
                    if ( a.Type == CommunicationType.SMS || a.Type == CommunicationType.PushNotification )
                    {
                        return a.Summary.Truncate( 250, true );
                    }

                    return a.Summary;
                } )
                .AddField( "status", a => a.InferredStatus )
                .AddField( "recipientCount", a => a.RecipientCount )
                .AddField( "deliveredCount", a => a.DeliveredCount )
                .AddField( "openedCount", a => a.OpenedCount )
                .AddField( "failedCount", a => a.FailedCount )
                .AddField( "unsubscribedCount", a => a.UnsubscribedCount )
                .AddTextField( "topic", a =>
                {
                    if ( !a.TopicValueId.HasValue )
                    {
                        return null;
                    }

                    return DefinedValueCache.Get( a.TopicValueId.Value )?.Value;
                } )
                .AddDateTimeField( "sendDateTime", a => a.SendDateTime )
                .AddDateTimeField( "futureSendDateTime", a => a.FutureSendDateTime )
                .AddPersonField( "sentByPerson", a =>
                {
                    if ( !a.SenderPersonId.HasValue )
                    {
                        return null;
                    }

                    // We're not going to display the avatar or connection status, so we need very little data here.
                    return new Person
                    {
                        Id = a.SenderPersonId.Value,
                        NickName = a.SenderPersonNickName,
                        LastName = a.SenderPersonLastName,
                        SuffixValueId = a.SenderPersonSuffixValueId,
                        RecordSourceValueId = a.SenderRecordTypeValueId
                    };
                } )
                .AddDateTimeField( "reviewedDateTime", a => a.ReviewedDateTime )
                .AddTextField( "reviewedByPersonFullName", a =>
                {
                    if ( !a.ReviewerPersonId.HasValue || a.ReviewerPersonId == a.SenderPersonId )
                    {
                        return null;
                    }

                    return Person.FormatFullName(
                        a.ReviewerPersonNickName,
                        a.ReviewerPersonLastName,
                        a.ReviewerPersonSuffixValueId,
                        a.ReviewerRecordTypeValueId
                    );
                } )
                .AddField( "isDeleteDisabled", a => a.DeliveredCount > 0 );
        }

        /// <summary>
        /// Gets whether the current person can approve communications.
        /// </summary>
        /// <returns></returns>
        private bool GetCanApprove()
        {
            return this.BlockCache.IsAuthorized( Authorization.APPROVE, GetCurrentPerson() );
        }

        #endregion

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent a communication and recipient SQL projection.
        /// </summary>
        private class CommunicationRow
        {
            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Communication"/> unique identifier.
            /// </summary>
            public Guid Guid { get; set; }

            /// <inheritdoc cref="Rock.Model.Communication.CommunicationTemplateId"/>
            public int? CommunicationTemplateId { get; set; }

            /// <inheritdoc cref="Rock.Model.Communication.SystemCommunicationId"/>
            public int? SystemCommunicationId { get; set; }

            /// <inheritdoc cref="Rock.Model.Communication.CommunicationType"/>
            public CommunicationType Type { get; set; }

            /// <summary>
            /// Gets or sets the name [or subject or push title] of the communication.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the summary of the communication.
            /// </summary>
            public string Summary { get; set; }

            /// <inheritdoc cref="Rock.Model.Communication.Status"/>
            public CommunicationStatus Status { get; set; }

            /// <summary>
            /// The inferred status of the communication, which can be an actual <see cref="CommunicationStatus"/> value
            /// or one of "Sending (5)" or "Sent (6)", which the UI knows how to display.
            /// </summary>
            public int InferredStatus { get; set; }

            /// <inheritdoc cref="Rock.Model.Communication.CommunicationTopicValueId"/>
            public int? TopicValueId { get; set; }

            /// <inheritdoc cref="Rock.Model.Communication.SendDateTime"/>
            public DateTime? SendDateTime { get; set; }

            /// <inheritdoc cref="Rock.Model.Communication.FutureSendDateTime"/>
            public DateTime? FutureSendDateTime { get; set; }

            /// <inheritdoc cref="Rock.Model.Communication.ReviewedDateTime"/>
            public DateTime? ReviewedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the count of <see cref="CommunicationRecipient"/>s tied to this <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int RecipientCount { get; set; }

            /// <summary>
            /// Gets or sets the count of <see cref="CommunicationRecipient"/>s to whom this <see cref="Rock.Model.Communication"/>
            /// was successfully delivered.
            /// </summary>
            public int DeliveredCount { get; set; }

            /// <summary>
            /// Gets or sets the count of <see cref="CommunicationRecipient"/>s who opened this <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int OpenedCount { get; set; }

            /// <summary>
            /// Gets or sets the count of <see cref="CommunicationRecipient"/>s to whom delivery of this
            /// <see cref="Rock.Model.Communication"/> failed.
            /// </summary>
            public int FailedCount { get; set; }

            /// <summary>
            /// Gets or sets the count of <see cref="CommunicationRecipient"/>s who unsubscribed as a result of receiving
            /// this <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int UnsubscribedCount { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? SenderPersonId { get; set; }

            /// <summary>
            /// Gets or sets the nickname of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public string SenderPersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public string SenderPersonLastName { get; set; }

            /// <summary>
            /// Get or sets the suffix value identifier of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? SenderPersonSuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the record type value identifier of the person who sent the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? SenderRecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person who reviewed the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? ReviewerPersonId { get; set; }

            /// <summary>
            /// Gets or sets the nickname of the person who reviewed the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public string ReviewerPersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person who reviewed the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public string ReviewerPersonLastName { get; set; }

            /// <summary>
            /// Get or sets the suffix value identifier of the person who reviewed the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? ReviewerPersonSuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the record type value identifier of the person who reviewed the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public int? ReviewerRecordTypeValueId { get; set; }
        }

        #endregion Supporting Classes
    }
}
