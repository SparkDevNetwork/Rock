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
using System.Linq;

using Rock.Attribute;
using Rock.Mobile;
using Rock.Data;
using Rock.Model;
using Rock.Model.Connection.ConnectionRequest.Options;
using Rock.Security;
using ConnectionRequestViewModelSortProperty = Rock.Common.Mobile.Enums.ConnectionRequestViewModelSortProperty;
namespace Rock.Blocks.Types.Mobile.Connection
{
    /// <summary>
    /// Displays the list of connection requests for a single opportunity.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Request List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the list of connection requests for a single opportunity." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

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
        DefaultValue = "2E36BC98-A18A-4524-8AC1-F14A1AC9DE2F",
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

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_REQUEST_LIST )]
    public class ConnectionRequestList : RockBlockType
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
        public override Version RequiredMobileVersion => new Version( 1, 3 );

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
                var connectionRequestService = new ConnectionRequestService( rockContext );
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
                    var filterOptions = new ConnectionRequestQueryOptions
                    {
                        ConnectionOpportunityGuids = new List<Guid> { connectionOpportunityGuid },
                        ConnectionStates = filterViewModel.ConnectionStates,
                        IsFutureFollowUpPastDueOnly = filterViewModel.OnlyPastDue,
                    };

                    if ( filterViewModel.OnlyMyConnections )
                    {
                        filterOptions.ConnectorPersonIds = new List<int> { RequestContext.CurrentPerson.Id };
                    }

                    if( filterViewModel.CampusGuid.HasValue )
                    {
                        filterOptions.CampusGuid = filterViewModel.CampusGuid;
                    }

                    var qry = connectionRequestService.GetConnectionRequestsQuery( filterOptions );

                    // We currently don't support showing connected connection requests
                    // since that could end up being a massive list for mobile.
                    qry = qry.Where( r => r.ConnectionState != ConnectionState.Connected );

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

                var sortProperty = filterViewModel.SortProperty;
                if ( sortProperty != null )
                {
                    // Sort by the selected sorting property
                    switch ( sortProperty )
                    {
                        case ConnectionRequestViewModelSortProperty.Requestor:
                            requests = requests
                                .OrderBy( cr => cr.PersonAlias.Person.LastName )
                                .ThenBy( cr => cr.PersonAlias.Person.NickName )
                                .ThenBy( cr => cr.Order )
                                .ThenBy( cr => cr.Id )
                                .ToList();
                            break;

                        case ConnectionRequestViewModelSortProperty.Connector:
                            requests = requests
                                .OrderBy( cr => cr.ConnectorPersonAlias?.Person.FirstName )
                                .ThenBy( cr => cr.ConnectorPersonAlias?.Person.NickName )
                                .ThenBy( cr => cr.Order )
                                .ThenBy( cr => cr.Id )
                                .ToList();
                            break;

                        case ConnectionRequestViewModelSortProperty.DateAdded:
                            requests = requests
                                .OrderBy( cr => cr.CreatedDateTime )
                                .ThenBy( cr => cr.Order )
                                .ThenBy( cr => cr.Id )
                                .ToList();
                            break;

                        case ConnectionRequestViewModelSortProperty.DateAddedDesc:
                            requests = requests
                                .OrderByDescending( cr => cr.CreatedDateTime )
                                .ThenByDescending( cr => cr.Order )
                                .ThenByDescending( cr => cr.Id )
                                .ToList();
                            break;

                        case ConnectionRequestViewModelSortProperty.LastActivity:
                            requests = requests
                                .OrderBy( cr => cr.ConnectionRequestActivities.Max( cra => cra.CreatedDateTime ) )
                                .ThenBy( cr => cr.Order )
                                .ThenBy( cr => cr.Id )
                                .ToList();
                            break;

                        case ConnectionRequestViewModelSortProperty.LastActivityDesc:
                            requests = requests
                                .OrderByDescending( cr => cr.ConnectionRequestActivities.Max( cra => cra.CreatedDateTime ) )
                                .ThenByDescending( cr => cr.Order )
                                .ThenByDescending( cr => cr.Id )
                                .ToList();
                            break;

                        default:
                            break;
                    }
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

            /// <summary>
            /// Gets or sets the campus to limit the results to.
            /// </summary>
            /// <value>The value of the campus to limit to, or null if you want to include all campuses.</value>
            public Guid? CampusGuid { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether to only show past due requests.
            /// </summary>
            public bool OnlyPastDue { get; set; }

            /// <summary>
            /// Gets or sets the sort property.
            /// </summary>
            public Rock.Common.Mobile.Enums.ConnectionRequestViewModelSortProperty? SortProperty { get; set; }
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
