using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Blocks.Types.Mobile.Connection
{
    /// <summary>
    /// Displays the list of connection requests for a single opportunity.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Connection Request List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the list of connection requests for a single opportunity." )]
    [IconCssClass( "fa fa-list" )]

    #region Block Attributes

    [CodeEditorField(
        "Header Template",
        Description = "Lava template used to render the header above the connection request.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.HeaderTemplate,
        Order = 0 )]

    [BlockTemplateField( "Request Template",
        Description = "The template used to render the connection requests.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_REQUEST_LIST,
        DefaultValue = "787BFAA8-FF61-49BA-80DD-67074DC362C2",
        IsRequired = true,
        Key = AttributeKey.RequestTemplate,
        Order = 1 )]

    [LinkedPage(
        "Detail Page",
        Description = "Page to link to when user taps on a connection request. ConnectionRequestGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 2 )]

    [IntegerField(
        "Max Requests to Show",
        Description = "The maximum number of requests to show in a single load, a Load More button will be visible if there are more requests to show.",
        IsRequired = true,
        DefaultIntegerValue = 50,
        Key = AttributeKey.MaxRequestsToShow,
        Order = 3 )]

    #endregion

    public class ConnectionRequestList : RockMobileBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="ConnectionRequestList"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string HeaderTemplate = "HeaderTemplate";

            public const string RequestTemplate = "RequestTemplate";

            public const string DetailPage = "DetailPage";

            public const string MaxRequestsToShow = "MaxRequstsToShow";
        }

        /// <summary>
        /// Gets the header template.
        /// </summary>
        /// <value>
        /// The header template.
        /// </value>
        protected string HeaderTemplate => GetAttributeValue( AttributeKey.HeaderTemplate );

        /// <summary>
        /// Gets the request template.
        /// </summary>
        /// <value>
        /// The request template.
        /// </value>
        protected string RequestTemplate => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.RequestTemplate ) );

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        protected Guid? DetailPageGuid => GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the maximum number of requests to show per page load.
        /// </summary>
        /// <value>
        /// The maximum number of requests to show per page load.
        /// </value>
        protected int MaxRequestsToShow => GetAttributeValue( AttributeKey.MaxRequestsToShow ).AsIntegerOrNull() ?? 50;

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override int RequiredMobileAbiVersion => 3;

        /// <inheritdoc/>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Connection.ConnectionRequestList";

        /// <inheritdoc/>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the connection requests queryable that will provide the results.
        /// </summary>
        /// <param name="connectionOpportunityGuid">The connection opportunity unique identifier.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A queryable of <see cref="ConnectionRequest"/> objects.</returns>
        /// <exception cref="System.ArgumentNullException">filter</exception>
        private static IQueryable<ConnectionRequest> GetConnectionRequestsQuery( Guid connectionOpportunityGuid, Person currentPerson, GetConnectionRequestsFilter filter, RockContext rockContext )
        {
            if ( filter == null )
            {
                throw new ArgumentNullException( nameof( filter ) );
            }

            var connectionRequestService = new ConnectionRequestService( rockContext );

            var qry = connectionRequestService.Queryable()
                .Where( r => r.ConnectionOpportunity.Guid == connectionOpportunityGuid );

            if ( filter.ConnectorPersonIds != null && filter.ConnectorPersonIds.Any() )
            {
                qry = qry.Where( r => filter.ConnectorPersonIds.Contains( r.ConnectorPersonAlias.PersonId ) );
            }

            if ( filter.ConnectionStates != null && filter.ConnectionStates.Any() )
            {
                qry = qry.Where( r => filter.ConnectionStates.Contains( r.ConnectionState ) );
            }

            return qry;
        }

        /// <summary>
        /// Gets the connection requests view model that can be sent to the client.
        /// </summary>
        /// <param name="connectionOpportunityGuid">The connection opportunity unique identifier.</param>
        /// <param name="filterViewModel">The filter.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>The <see cref="GetRequestsViewModel"/> that contains the information about the response.</returns>
        private GetRequestsViewModel GetConnectionRequests( Guid connectionOpportunityGuid, GetConnectionRequestsFilterViewModel filterViewModel, int pageNumber )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionOpportunity = new ConnectionOpportunityService( rockContext ).GetNoTracking( connectionOpportunityGuid );
                bool hasMore;
                List<ConnectionRequest> requests;

                if ( filterViewModel.OnlyMyConnections && RequestContext.CurrentPerson == null )
                {
                    hasMore = false;
                    requests = new List<ConnectionRequest>();
                }
                else
                {
                    var filter = new GetConnectionRequestsFilter
                    {
                        ConnectionStates = filterViewModel.ConnectionStates
                    };

                    if ( filterViewModel.OnlyMyConnections )
                    {
                        filter.ConnectorPersonIds = new List<int> { RequestContext.CurrentPerson.Id };
                    }

                    var qry = GetConnectionRequestsQuery( connectionOpportunityGuid, RequestContext.CurrentPerson, filter, rockContext );

                    // We currently don't support showing inactive connection requests
                    // since that could end up being a massive list for mobile.
                    qry = qry.Where( r => r.ConnectionState != ConnectionState.Inactive );

                    // Put all the requests in memory so we can check security and
                    // then get the current set of requests, plus one. The extra is
                    // so that we can tell if there are more to load.
                    requests = qry.ToList()
                        .Where( r => r.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                        .Skip( ( pageNumber * MaxRequestsToShow ) )
                        .Take( MaxRequestsToShow + 1 )
                        .ToList();

                    // Determine if we have more requests to show and then properly
                    // limit the requests to the correct amount.
                    hasMore = requests.Count > MaxRequestsToShow;
                    requests = requests.Take( MaxRequestsToShow ).ToList();
                }

                // Process the connection requests with the template.
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "ConnectionRequests", requests );
                mergeFields.AddOrReplace( "DetailPage", DetailPageGuid );
                var content = RequestTemplate.ResolveMergeFields( mergeFields );

                string headerContent = string.Empty;

                // If we found a connection opportunity then process the header
                // template.
                if ( connectionOpportunity != null )
                {
                    mergeFields = RequestContext.GetCommonMergeFields();
                    mergeFields.Add( "ConnectionOpportunity", connectionOpportunity );

                    headerContent = HeaderTemplate.ResolveMergeFields( mergeFields );
                }

                return new GetRequestsViewModel
                {
                    HasMore = hasMore,
                    PageNumber = pageNumber,
                    HeaderContent = headerContent,
                    Content = content
                };
            }
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the requests to be displayed for the given page number.
        /// </summary>
        /// <param name="connectionOpportunityGuid">The connection opportunity unique identifier.</param>
        /// <param name="filter">The filter options.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetRequests( Guid connectionOpportunityGuid, GetConnectionRequestsFilterViewModel filter, int pageNumber )
        {
            if ( filter == null )
            {
                return ActionBadRequest();
            }

            return ActionOk( GetConnectionRequests( connectionOpportunityGuid, filter, pageNumber ) );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The filtering options when getting requests.
        /// </summary>
        public class GetConnectionRequestsFilter
        {
            /// <summary>
            /// Gets or sets the connector person identifiers to limit the
            /// results to.
            /// </summary>
            /// <value>
            /// The connector person identifiers.
            /// </value>
            public List<int> ConnectorPersonIds { get; set; }

            /// <summary>
            /// Gets or sets the states that results will be limited to.
            /// </summary>
            /// <value>
            /// The states that results will be limited to.
            /// </value>
            public List<ConnectionState> ConnectionStates { get; set; }
        }

        /// <summary>
        /// The view model that defines the filtering options when getting requests.
        /// </summary>
        public class GetConnectionRequestsFilterViewModel
        {
            /// <summary>
            /// Gets or sets a value indicating whether to only show connection
            /// requests that have the logged in person marked as the connector.
            /// </summary>
            /// <value>
            ///   <c>true</c> if only my connections should show; otherwise, <c>false</c>.
            /// </value>
            public bool OnlyMyConnections { get; set; }

            /// <summary>
            /// Gets or sets the states that results will be limited to.
            /// </summary>
            /// <value>
            /// The states that results will be limited to.
            /// </value>
            public List<ConnectionState> ConnectionStates { get; set; }
        }

        /// <summary>
        /// The view model returned by the GetRequests action.
        /// </summary>
        public class GetRequestsViewModel
        {
            /// <summary>
            /// Gets or sets a value indicating whether this instance has more connection requests.
            /// </summary>
            /// <value>
            ///   <c>true</c> if this instance has more connection requests; otherwise, <c>false</c>.
            /// </value>
            public bool HasMore { get; set; }

            /// <summary>
            /// Gets or sets the page number for these results.
            /// </summary>
            /// <value>
            /// The page number for these results.
            /// </value>
            public int PageNumber { get; set; }

            /// <summary>
            /// Gets or sets the rendered content for the header.
            /// </summary>
            /// <value>
            /// The rendered content for the header.
            /// </value>
            public string HeaderContent { get; set; }

            /// <summary>
            /// Gets or sets the rendered content for this page of requests.
            /// </summary>
            /// <value>
            /// The rendered content for this page of requests.
            /// </value>
            public string Content { get; set; }
        }

        #endregion
    }
}
