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
using System.Data;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Reporting;
using Rock.ViewModels.Blocks.Communication.CommunicationSaturationReport;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays a list of communication recipients.
    /// </summary>

    [DisplayName( "Mass Communication Analytics" )]
    [Category( "Communication" )]
    [Description( "Shows analytics for communications." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField( "Email Bucket Ratio",
        Key = AttributeKey.EmailBucketRatio,
        Description = "This ratio determines the number of days each x-axis bucket represents on the chart. A value of 10 means that for every 10 days in the date range, 1 day is added to the bucket size.",
        IsRequired = false,
        DefaultValue = "10",
        Order = 1 )]

    [IntegerField( "SMS Bucket Ratio",
        Key = AttributeKey.SmsBucketRatio,
        Description = "This ratio determines the number of days each x-axis bucket represents on the chart. A value of 10 means that for every 10 days in the date range, 1 day is added to the bucket size.",
        IsRequired = false,
        DefaultValue = "20",
        Order = 2 )]

    [IntegerField( "Push Notifications Bucket Ratio",
        Key = AttributeKey.PushNotificationBucketRatio,
        Description = "This ratio determines the number of days each x-axis bucket represents on the chart. A value of 10 means that for every 10 days in the date range, 1 day is added to the bucket size.",
        IsRequired = false,
        DefaultValue = "20",
        Order = 3 )]

    [IntegerField( "Max Recipients To List",
        Key = AttributeKey.MaxRecipientsToList,
        Description = "The maximum number of rows to display in the report when listing by recipient.",
        IsRequired = false,
        DefaultValue = "100",
        Order = 4 )]

    [IntegerField( "Max Communications To List",
        Key = AttributeKey.MaxCommunicationsToList,
        Description = "The maximum number of rows to display in the report when listing by communication.",
        IsRequired = false,
        DefaultValue = "100",
        Order = 5 )]

    [LinkedPage( "Communication Detail Page",
        Description = "The page that will show the communication details.",
        Key = AttributeKey.CommunicationDetailPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "44637bbe-3460-4133-8164-a14351389fb5" )]
    [Rock.SystemGuid.BlockTypeGuid( "6ee7bcf5-88a4-4484-a590-c8c03a4c143f" )]
    [CustomizedGrid]
    public class CommunicationSaturationReport : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string EmailBucketRatio = "EmailBucketRatio";
            public const string SmsBucketRatio = "SmsBucketRatio";
            public const string PushNotificationBucketRatio = "PushNotificationBucketRatio";
            public const string MaxRecipientsToList = "MaxRecipientsToList";
            public const string MaxCommunicationsToList = "MaxCommunicationsToList";
            public const string CommunicationDetailPage = "CommunicationDetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string CommunicationDetailPage = "CommunicationDetailPage";
        }

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "FilterDateRange";
            public const string FilterDataView = "FilterDataView";
            public const string FilterConnectionStatus = "FilterConnectionStatus";
            public const string FilterMedium = "FilterMedium";
            public const string FilterBulkOnly = "FilterBulkOnly";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CommunicationSaturationReportBlockBox();

            box.Filters = GetFilterOptions();
            box.RecipientsGridBox = GetRecipientsGridBuilder().BuildDefinition();
            box.CommunicationsGridBox = GetCommunicationsGridBuilder().BuildDefinition();
            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list. We prioritize getting the options from
        /// the query string first because if it's a shared/bookmarked URL, we want it to have the exact same filters
        /// as the previous viewing of the page.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CommunicationSaturationReportFiltersBag GetFilterOptions()
        {
            var options = new CommunicationSaturationReportFiltersBag();

            var personPreferences = GetBlockPersonPreferences();
            var dateRangePref = personPreferences.GetValue( PreferenceKey.FilterDateRange );
            var dataViewPref = personPreferences.GetValue( PreferenceKey.FilterDataView );
            var connectionStatusPref = personPreferences.GetValue( PreferenceKey.FilterConnectionStatus );
            var mediumPref = personPreferences.GetValue( PreferenceKey.FilterMedium )?.SplitDelimitedValues().ToList().Where( a => a?.Trim()?.IsNotNullOrWhiteSpace() ?? false ).ToList();
            var bulkOnlyPref = personPreferences.GetValue( PreferenceKey.FilterBulkOnly );

            options.DateRangeDelimitedString = dateRangePref.IsNotNullOrWhiteSpace() ? dateRangePref : options.DateRangeDelimitedString;
            options.DataView = DataViewCache.Get( dataViewPref.AsGuid() )?.ToListItemBag();
            options.ConnectionStatus = DefinedValueCache.Get( connectionStatusPref.AsGuid() )?.ToListItemBag();
            options.Medium = mediumPref.Any() ? mediumPref : options.Medium;
            options.BulkOnly = bulkOnlyPref.AsBooleanOrNull() ?? options.BulkOnly;

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
                [NavigationUrlKey.CommunicationDetailPage] = this.GetLinkedPageUrl( AttributeKey.CommunicationDetailPage, PageParameterKey.CommunicationId, "((Key))" )
            };
        }

        /// <summary>
        /// Gets the grid builder that will provide all the details and values
        /// of the recipients grid.
        /// </summary>
        /// <returns>An instance of <see cref="GridBuilder{T}"/>.</returns>
        private GridBuilder<RecipientGridDataBag> GetRecipientsGridBuilder()
        {
            return new GridBuilder<RecipientGridDataBag>()
                //.WithBlock( this )
                .AddField( "idKey", a => a.Person.IdKey )
                .AddPersonField( "person", a => a.Person )
                .AddTextField( "connectionStatus", a => a.ConnectionStatus )
                .AddField( "messageCount", a => a.MessageCount );
        }

        /// <summary>
        /// Gets the grid builder that will provide all the details and values
        /// of the communications grid.
        /// </summary>
        /// <returns>An instance of <see cref="GridBuilder{T}"/>.</returns>
        private GridBuilder<CommunicationGridDataBag> GetCommunicationsGridBuilder()
        {
            return new GridBuilder<CommunicationGridDataBag>()
                //.WithBlock( this )
                .AddField( "id", a => a.Id )
                .AddTextField( "name", a => a.Name )
                .AddDateTimeField( "dateSent", a => a.SendDateTime )
                .AddField( "messageCount", a => a.MessageCount )
                .AddTextField( "sentBy", a => a.SentBy.FullName )
                .AddTextField( "reviewedBy", a => a.ReviewedBy == null || a.ReviewedBy.Id == a.SentBy.Id ? "" : a.ReviewedBy?.FullName );
        }

        /// <summary>
        /// Gets the chart data bag that will be sent to the client.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>An intance of <see cref="object"/> containing the data for filling the chart.</returns>
        private List<CommunicationSaturationReportChartDataBag> GetChartDataBag( RockContext rockContext )
        {
            var parameters = new Dictionary<string, object>();
            var filters = GetFilterOptions();

            var computedFilters = new ComputedFilters( filters, BlockCache );

            parameters.Add( "StartDate", computedFilters.StartDate );
            parameters.Add( "EndDate", computedFilters.EndDate );
            parameters.Add( "BucketSize", computedFilters.BucketSize );
            parameters.Add( "CommunicationType", computedFilters.CommunicationType.Select( ct => ct.ConvertToInt() ).ToList().AsDelimited( "," ) );
            parameters.Add( "IncludeNonBulk", computedFilters.IncludeNonBulk );
            parameters.Add( "DataViewId", computedFilters.DataViewId );
            parameters.Add( "ConnectionStatusValueId", computedFilters.ConnectionStatusValueId );

            var chartData = new DbService( rockContext ).GetDataTableFromSqlCommand( "spCommunication_SaturationReport", System.Data.CommandType.StoredProcedure, parameters );

            var data = new List<CommunicationSaturationReportChartDataBag>();

            foreach ( DataRow row in chartData.Rows )
            {
                data.Add( new CommunicationSaturationReportChartDataBag
                {
                    Bucket = row["Name"].ToStringSafe(),
                    NumberOfRecipients = row["NumberOfRecipients"].ToIntSafe()
                } );
            }

            return data;
        }

        /// <summary>
        /// Gets the grid data bag for the recipients grid that will be sent to the client.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>An instance of <see cref="GridDataBag"/>.</returns>
        private GridDataBag GetRecipientsGridDataBag( RockContext rockContext )
        {
            int namelessRecordTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;
            var anonymousVisitorGuid = Rock.SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid();
            var activeRecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE ).Id;

            var filters = new ComputedFilters( GetFilterOptions(), BlockCache );

            // Get the queryable and make sure it is ordered correctly.
            var recipientsQry = new CommunicationRecipientService( rockContext )
                .Queryable().AsNoTracking()
                // Filter out nameless, anonymous, deceased, and inactive people
                .Where( cr => !cr.PersonAlias.Person.IsDeceased
                    && cr.PersonAlias.Person.RecordTypeValueId != namelessRecordTypeValueId
                    && cr.PersonAlias.Person.Guid != anonymousVisitorGuid
                    && cr.PersonAlias.Person.RecordStatusValueId == activeRecordStatusValueId
                    && cr.PersonAlias.Person.AgeClassification == AgeClassification.Adult
                    && cr.CreatedDateTime >= filters.StartDate
                    && cr.CreatedDateTime < filters.EndDate
                    && filters.CommunicationType.Contains( cr.Communication.CommunicationType )
                );

            if ( filters.IncludeNonBulk == 0 )
            {
                recipientsQry = recipientsQry.Where( cr => cr.Communication.IsBulkCommunication == true );
            }

            if ( filters.ConnectionStatusValueId != null )
            {
                recipientsQry = recipientsQry.Where( cr => cr.PersonAlias.Person.ConnectionStatusValueId == filters.ConnectionStatusValueId );
            }

            if ( filters.DataViewId != null )
            {
                var dataView = DataViewCache.Get( filters.DataViewId.Value );
                if ( dataView != null )
                {
                    var dvPeopleIds = dataView.GetQuery( new GetQueryableOptions { DbContext = rockContext } ).Select( a => a.Id );
                    recipientsQry = recipientsQry.Where( cr => dvPeopleIds.Contains( cr.PersonAlias.PersonId ) );
                }
            }

            var recipients = recipientsQry
                .Select( cr => cr.PersonAlias.PersonId )
                .GroupBy( a => a )
                .Select( grp => new { PersonId = grp.Key, MessageCount = grp.Count() } )
                .OrderByDescending( a => a.MessageCount )
                .Take( GetAttributeValue( AttributeKey.MaxRecipientsToList ).ToIntSafe( 100 ) )
                .ToList()
                .ToDictionary( a => a.PersonId, a => a.MessageCount );

            var personIds = recipients.Keys.ToList();

            var gridData = new PersonService( rockContext )
                .Queryable().AsNoTracking()
                .Include( a => a.ConnectionStatusValue )
                .Where( a => personIds.Contains( a.Id ) )
                .ToList()
                .Select( a => new RecipientGridDataBag
                {
                    Person = a,
                    ConnectionStatus = a.ConnectionStatusValue?.Value,
                    MessageCount = recipients[a.Id]
                } )
                .OrderByDescending( a => a.MessageCount );

            return GetRecipientsGridBuilder().Build( gridData );
        }

        /// <summary>
        /// Gets the grid data bag that will be sent to the client.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <returns>An instance of <see cref="GridDataBag"/>.</returns>
        protected GridDataBag GetCommunicationsGridDataBag( RockContext rockContext )
        {
            int namelessRecordTypeValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;
            var anonymousVisitorGuid = Rock.SystemGuid.Person.ANONYMOUS_VISITOR.AsGuid();
            var activeRecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE ).Id;

            var filters = new ComputedFilters( GetFilterOptions(), BlockCache );

            // Get the queryable and make sure it is ordered correctly.
            var qry = new CommunicationService( rockContext )
                .Queryable().AsNoTracking()
                .Where( c => c.SendDateTime >= filters.StartDate
                    && c.SendDateTime < filters.EndDate
                    && filters.CommunicationType.Contains( c.CommunicationType )
                    && ( filters.IncludeNonBulk == 1 || c.IsBulkCommunication == true )
                    // Filter to make sure we're only looking at communications to at least one living and active adult
                    && c.Recipients.Any( cr => !cr.PersonAlias.Person.IsDeceased
                        && cr.PersonAlias.Person.RecordTypeValueId != namelessRecordTypeValueId
                        && cr.PersonAlias.Person.Guid != anonymousVisitorGuid
                        && cr.PersonAlias.Person.RecordStatusValueId == activeRecordStatusValueId
                        && cr.PersonAlias.Person.AgeClassification == AgeClassification.Adult
                    )
                );

            if ( filters.ConnectionStatusValueId != null )
            {
                qry = qry.Where( c => c.Recipients.Any( cr => cr.PersonAlias.Person.ConnectionStatusValueId == filters.ConnectionStatusValueId ) );
            }

            if ( filters.DataViewId != null )
            {
                var dataView = DataViewCache.Get( filters.DataViewId.Value );
                if ( dataView != null )
                {
                    var dvPeopleIds = dataView.GetQuery( new GetQueryableOptions { DbContext = rockContext } ).Select( a => a.Id );
                    qry = qry.Where( c => c.Recipients.Any( cr => dvPeopleIds.Contains( cr.PersonAlias.PersonId ) ) );
                }
            }

            var results = qry.Select( c => new
            {
                Communication = c,
                SentBy = c.SenderPersonAlias.Person,
                ReviewedBy = c.ReviewerPersonAlias.Person,
                MessageCount = c.Recipients.Count()
            } )
                .OrderByDescending( c => c.MessageCount )
                .Take( GetAttributeValue( AttributeKey.MaxCommunicationsToList ).ToIntSafe( 100 ) )
                .ToList()
                .Select( c => new CommunicationGridDataBag
                {
                    Id = c.Communication.Id,
                    Name = c.Communication.Name,
                    SentBy = c.SentBy,
                    ReviewedBy = c.ReviewedBy,
                    MessageCount = c.MessageCount,
                    SendDateTime = c.Communication.SendDateTime
                } );

            return GetCommunicationsGridBuilder().Build( results );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the bag that describes the grid data to be displayed in the block.
        /// </summary>
        /// <returns>An action result that contains the grid data.</returns>
        [BlockAction]
        public virtual BlockActionResult GetBlockData()
        {
            var rockContext = new RockContext();

            var bag = new CommunicationSaturationReportBlockDataBag
            {
                ChartData = GetChartDataBag( rockContext ),
                RecipientsGridData = GetRecipientsGridDataBag( rockContext ),
                CommunicationsGridData = GetCommunicationsGridDataBag( rockContext )
            };

            return ActionOk( bag );
        }

        #endregion

        private class RecipientGridDataBag
        {
            /// <summary>
            /// Data representing a person
            /// </summary>
            public Person Person { get; set; }

            /// <summary>
            /// The number of messages sent to this person
            /// </summary>
            public int MessageCount { get; set; }

            /// <summary>
            /// The connection status of the person
            /// </summary>
            public string ConnectionStatus { get; set; }
        }

        private class CommunicationGridDataBag
        {
            /// <summary>
            /// The identifier for the communication
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// The name of the communication
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The person who sent the communication
            /// </summary>
            public Person SentBy { get; set; }

            /// <summary>
            /// The person who reviewed the communication
            /// </summary>
            public Person ReviewedBy { get; set; }

            /// <summary>
            /// The number of messages sent to this person
            /// </summary>
            public int MessageCount { get; set; }

            /// <summary>
            /// The date the message was sent
            /// </summary>
            public DateTime? SendDateTime { get; set; }
        }

        class ComputedFilters
        {
            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }

            public int? DataViewId { get; set; }

            public int? ConnectionStatusValueId { get; set; }

            public List<CommunicationType> CommunicationType { get; set; }

            public int? BucketSize { get; set; }

            public int? IncludeNonBulk { get; set; }

            public int? MaxRecipientsToList { get; set; }

            public int? MaxCommunicationsToList { get; set; }

            /// <summary>
            /// Create the computed filters from the data in the filter bag and the attributes found on the block cache.
            /// </summary>
            public ComputedFilters( CommunicationSaturationReportFiltersBag filterBag, BlockCache blockCache )
            {
                StartDate = null;
                EndDate = null;
                DataViewId = null;
                ConnectionStatusValueId = null;
                CommunicationType = new List<CommunicationType>();
                BucketSize = null;
                IncludeNonBulk = null;
                MaxRecipientsToList = blockCache.GetAttributeValue( AttributeKey.MaxRecipientsToList ).AsIntegerOrNull();
                MaxCommunicationsToList = blockCache.GetAttributeValue( AttributeKey.MaxCommunicationsToList ).AsIntegerOrNull();

                // Generate a date range from the SlidingDateRangePicker's value
                var dateRange = RockDateTimeHelper.CalculateDateRangeFromDelimitedValues( filterBag.DateRangeDelimitedString );

                if ( dateRange.Start != null )
                {
                    StartDate = dateRange.Start;
                }

                if ( dateRange.End != null )
                {
                    EndDate = dateRange.End;
                }

                var includedRatios = new List<int> { 1 };

                if ( filterBag.Medium.Contains( "1" ) )
                {
                    includedRatios.Add( blockCache.GetAttributeValue( AttributeKey.EmailBucketRatio ).AsInteger() );
                }

                if ( filterBag.Medium.Contains( "2" ) )
                {
                    includedRatios.Add( blockCache.GetAttributeValue( AttributeKey.SmsBucketRatio ).AsInteger() );
                }

                if ( filterBag.Medium.Contains( "3" ) )
                {
                    includedRatios.Add( blockCache.GetAttributeValue( AttributeKey.PushNotificationBucketRatio ).AsInteger() );
                }

                decimal bucketRatio = includedRatios.Max();
                decimal totalDays = ( dateRange.End - dateRange.Start )?.Days ?? 1;

                BucketSize = ( int ) Math.Ceiling( totalDays / bucketRatio );
                CommunicationType = filterBag.Medium.Select( m => ( CommunicationType ) m.ToIntSafe() ).ToList();
                IncludeNonBulk = filterBag.BulkOnly ? 0 : 1;

                if ( filterBag.DataView != null )
                {
                    DataViewId = DataViewCache.Get( filterBag.DataView.Value.AsGuid() )?.Id;
                }

                if ( filterBag.ConnectionStatus != null )
                {
                    ConnectionStatusValueId = DefinedValueCache.Get( filterBag.ConnectionStatus.Value.AsGuid() )?.Id;
                }
            }
        }
    }
}
