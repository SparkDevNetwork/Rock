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
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Security;

namespace Rock.Blocks.Types.Mobile.Connection
{
    /// <summary>
    /// Displays the list of connection opportunities for a single connection type.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Connection Opportunity List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the list of connection opportunities for a single connection type." )]
    [IconCssClass( "fa fa-list" )]

    #region Block Attributes

    [CodeEditorField(
        "Header Template",
        Description = "Lava template used to render the header above the connection request.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.HeaderTemplate,
        Order = 0 )]

    [BlockTemplateField( "Opportunity Template",
        Description = "The template used to render the connection opportunities.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST,
        DefaultValue = "1FB8E236-DF34-4BA2-B5C6-CA8B542ABC7A",
        IsRequired = true,
        Key = AttributeKey.OpportunityTemplate,
        Order = 1 )]

    [LinkedPage(
        "Detail Page",
        Description = "Page to link to when user taps on a connection opportunity. ConnectionOpportunityGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 2 )]

    #endregion

    public class ConnectionOpportunityList : RockMobileBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="ConnectionOpportunityList"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string HeaderTemplate = "HeaderTemplate";

            public const string OpportunityTemplate = "OpportunityTemplate";

            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Gets the header template.
        /// </summary>
        /// <value>
        /// The header template.
        /// </value>
        protected string HeaderTemplate => GetAttributeValue( AttributeKey.HeaderTemplate );

        /// <summary>
        /// Gets the opportunity template.
        /// </summary>
        /// <value>
        /// The opportunity template.
        /// </value>
        protected string OpportunityTemplate => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.OpportunityTemplate ) );

        /// <summary>
        /// Gets the detail page unique identifier.
        /// </summary>
        /// <value>
        /// The detail page unique identifier.
        /// </value>
        protected Guid? DetailPageGuid => GetAttributeValue( AttributeKey.DetailPage ).AsGuidOrNull();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override int RequiredMobileAbiVersion => 3;

        /// <inheritdoc/>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Connection.ConnectionOpportunityList";

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
        /// Gets the connection opportunities queryable that will provide the results.
        /// </summary>
        /// <param name="connectionTypeGuid">The connection type unique identifier.</param>
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A queryable of <see cref="ConnectionOpportunity"/> objects.</returns>
        /// <exception cref="System.ArgumentNullException">filter</exception>
        internal static IQueryable<ConnectionOpportunity> GetConnectionOpportunitiesQuery( Guid connectionTypeGuid, GetConnectionOpportunitiesFilter filter, RockContext rockContext )
        {
            if ( filter == null )
            {
                throw new ArgumentNullException( nameof( filter ) );
            }

            var connectionOpportunityService = new ConnectionOpportunityService( rockContext );

            var qry = connectionOpportunityService.Queryable()
                .Where( o => o.ConnectionType.Guid == connectionTypeGuid );

            if ( filter.ConnectorPersonIds != null && filter.ConnectorPersonIds.Any() )
            {
                var connectorRequestsQry = new ConnectionRequestService( rockContext ).Queryable()
                    .Where( r => r.ConnectionState != ConnectionState.Connected
                        && r.ConnectorPersonAliasId.HasValue
                        && filter.ConnectorPersonIds.Contains( r.ConnectorPersonAlias.PersonId ) )
                    .Select( r => r.Id );

                qry = qry.Where( o => o.ConnectionRequests.Any( r => connectorRequestsQry.Contains( r.Id ) ) );
            }

            if ( !filter.IncludeInactive )
            {
                qry = qry.Where( o => o.IsActive && o.ConnectionType.IsActive );
            }

            return qry;
        }

        /// <summary>
        /// Gets the connection opportunities view model that can be sent to the client.
        /// </summary>
        /// <param name="connectionTypeGuid">The connection type unique identifier.</param>
        /// <param name="filterViewModel">The filter.</param>
        /// <returns>The <see cref="GetContentViewModel"/> that contains the information about the response.</returns>
        private GetContentViewModel GetConnectionOpportunities( Guid connectionTypeGuid, GetConnectionOpportunitiesFilterViewModel filterViewModel )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionType = new ConnectionTypeService( rockContext ).GetNoTracking( connectionTypeGuid );

                var filter = new GetConnectionOpportunitiesFilter
                {
                    IncludeInactive = true
                };

                if ( filterViewModel.OnlyMyConnections )
                {
                    filter.ConnectorPersonIds = new List<int> { RequestContext.CurrentPerson?.Id ?? 0 };
                }

                var qry = GetConnectionOpportunitiesQuery( connectionTypeGuid, filter, rockContext );

                // Make a list of any opportunity identifiers that are
                // configured for request security and the person is assigned
                // as the connector to any request.
                var currentPersonId = RequestContext.CurrentPerson?.Id;
                var selfAssignedSecurityOpportunities = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.ConnectorPersonAlias.PersonId == currentPersonId
                        && r.ConnectionOpportunity.ConnectionType.EnableRequestSecurity )
                    .Select( r => r.ConnectionOpportunityId )
                    .Distinct()
                    .ToList();

                // Put all the opportunities in memory so we can check security.
                var opportunities = qry.ToList()
                    .Where( o => o.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson )
                        || selfAssignedSecurityOpportunities.Contains( o.Id ) )
                    .ToList();

                // Get the various counts to make available to the Lava template.
                var requestCounts = GetOpportunityRequestCounts( opportunities, RequestContext.CurrentPerson, rockContext );

                // Process the connection opportunities with the template.
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "ConnectionOpportunities", opportunities );
                mergeFields.AddOrReplace( "DetailPage", DetailPageGuid );
                mergeFields.AddOrReplace( "ConnectionRequestCounts", requestCounts );

                var content = OpportunityTemplate.ResolveMergeFields( mergeFields );

                // If we found a connection opportunity then process the header
                // template.
                string headerContent = string.Empty;

                if ( connectionType != null )
                {
                    mergeFields = RequestContext.GetCommonMergeFields();
                    mergeFields.Add( "ConnectionType", connectionType );

                    headerContent = HeaderTemplate.ResolveMergeFields( mergeFields );
                }

                return new GetContentViewModel
                {
                    HeaderContent = headerContent,
                    Content = content
                };
            }
        }

        /// <summary>
        /// Gets the opportunity request counts for the given opportunities.
        /// </summary>
        /// <param name="opportunities">The opportunities.</param>
        /// <param name="currentPerson">The current person to use for count checks.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A dictionary of connection request count objects.</returns>
        internal static Dictionary<int, ConnectionRequestCountsViewModel> GetOpportunityRequestCounts( List<ConnectionOpportunity> opportunities, Person currentPerson, RockContext rockContext )
        {
            var connectionRequestService = new ConnectionRequestService( rockContext );

            // Fast out, if there is no logged in person then just return a
            // bunch of zeros for now. Later if we add other counts we might
            // need more complex logic.
            if ( currentPerson == null )
            {
                return opportunities.ToDictionary( o => o.Id, _ => new ConnectionRequestCountsViewModel
                {
                    AssignedToYouCount = 0
                } );
            }

            var opportunityIds = opportunities.Select( o => o.Id ).ToList();

            // Find all the connection requests assigned to the current person.
            var assignedToYouRequestQry = connectionRequestService.Queryable()
                .Where( r => opportunityIds.Contains( r.ConnectionOpportunityId )
                    && r.ConnectionState == ConnectionState.Active
                    && r.ConnectorPersonAliasId.HasValue
                    && r.ConnectorPersonAlias.PersonId == currentPerson.Id );

            // Group them by the connection opportunity and get the counts for
            // each opportunity.
            var requestCounts = assignedToYouRequestQry
                .GroupBy( r => r.ConnectionOpportunityId )
                .Select( g => new
                {
                    Id = g.Key,
                    Count = g.Count()
                } )
                .ToList()
                .ToDictionary( o => o.Id, o => new ConnectionRequestCountsViewModel
                {
                    AssignedToYouCount = o.Count
                } );

            // Fill in any missing opportunities with empty counts.
            foreach ( var opportunityId in opportunityIds )
            {
                if ( !requestCounts.ContainsKey( opportunityId ) )
                {
                    requestCounts.Add( opportunityId, new ConnectionRequestCountsViewModel() );
                }
            }

            return requestCounts;
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the opportunities to be displayed.
        /// </summary>
        /// <param name="connectionTypeGuid">The connection type unique identifier.</param>
        /// <param name="filter">The filter options.</param>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetContent( Guid connectionTypeGuid, GetConnectionOpportunitiesFilterViewModel filter = null )
        {
            filter = filter ?? new GetConnectionOpportunitiesFilterViewModel();

            return ActionOk( GetConnectionOpportunities( connectionTypeGuid, filter ) );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The filtering options when getting opportunities.
        /// </summary>
        internal class GetConnectionOpportunitiesFilter
        {
            /// <summary>
            /// Gets or sets a value indicating whether inactive opportunities
            /// should be included.
            /// </summary>
            /// <value>
            ///   <c>true</c> if inactive opportunities are included; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeInactive { get; set; }

            /// <summary>
            /// Gets or sets the connector person identifiers to limit the
            /// results to. If an opportunity does not have a non-connected
            /// request that is assigned to one of these identifiers it will
            /// not be included.
            /// </summary>
            /// <value>
            /// The connector person identifiers.
            /// </value>
            public List<int> ConnectorPersonIds { get; set; }
        }

        /// <summary>
        /// The view model that defines the filtering options when getting opportunities.
        /// </summary>
        public class GetConnectionOpportunitiesFilterViewModel
        {
            /// <summary>
            /// Gets or sets a value indicating whether to only show connection
            /// opportunities that have any requests with the logged in person
            /// marked as the connector.
            /// </summary>
            /// <value>
            ///   <c>true</c> if only my connections should show; otherwise, <c>false</c>.
            /// </value>
            public bool OnlyMyConnections { get; set; }
        }

        /// <summary>
        /// The view model returned by the GetContent action.
        /// </summary>
        public class GetContentViewModel
        {
            /// <summary>
            /// Gets or sets the rendered content for the header.
            /// </summary>
            /// <value>
            /// The rendered content for the header.
            /// </value>
            public string HeaderContent { get; set; }

            /// <summary>
            /// Gets or sets the rendered content for this page of opportunities.
            /// </summary>
            /// <value>
            /// The rendered content for this page of opportunities.
            /// </value>
            public string Content { get; set; }
        }

        /// <summary>
        /// View model that contains the request counts for a single connection
        /// opportunity.
        /// </summary>
        [LavaType]
        [DotLiquid.LiquidType( nameof( AssignedToYouCount ) )]
        internal class ConnectionRequestCountsViewModel
        {
            /// <summary>
            /// Gets or sets the number of requests in the opportunity that
            /// are assigned to the specified person.
            /// </summary>
            /// <value>
            /// The number of requests assigned to you.
            /// </value>
            public int AssignedToYouCount { get; set; }
        }

        #endregion
    }
}
