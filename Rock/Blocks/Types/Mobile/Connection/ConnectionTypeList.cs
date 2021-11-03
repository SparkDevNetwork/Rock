using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Blocks.Types.Mobile.Connection
{
    /// <summary>
    /// Displays the list of connection types.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Connection Type List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the list of connection types." )]
    [IconCssClass( "fa fa-list" )]

    #region Block Attributes

    [CodeEditorField(
        "Header Template",
        Description = "Lava template used to render the header above the connection request.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.HeaderTemplate,
        Order = 0 )]

    [BlockTemplateField( "Type Template",
        Description = "The template used to render the connection types.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_TYPE_LIST,
        DefaultValue = "E0D00422-7895-4081-9C06-16DE9BF48E1A",
        IsRequired = true,
        Key = AttributeKey.TypeTemplate,
        Order = 1 )]

    [LinkedPage(
        "Detail Page",
        Description = "Page to link to when user taps on a connection type. ConnectionTypeGuid is passed in the query string.",
        IsRequired = false,
        Key = AttributeKey.DetailPage,
        Order = 2 )]

    #endregion

    public class ConnectionTypeList : RockMobileBlockType
    {
        #region Block Attributes

        /// <summary>
        /// The block setting attribute keys for the <see cref="ConnectionTypeList"/> block.
        /// </summary>
        private static class AttributeKey
        {
            public const string HeaderTemplate = "HeaderTemplate";

            public const string TypeTemplate = "TypeTemplate";

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
        protected string TypeTemplate => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.TypeTemplate ) );

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
        public override string MobileBlockType => "Rock.Mobile.Blocks.Connection.ConnectionTypeList";

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
        /// <param name="filter">The filter to apply to the query.</param>
        /// <param name="rockContext">The Rock database context.</param>
        /// <returns>A queryable of <see cref="ConnectionType"/> objects.</returns>
        /// <exception cref="System.ArgumentNullException">filter</exception>
        internal static IQueryable<ConnectionType> GetConnectionTypesQuery( GetConnectionTypesFilter filter, RockContext rockContext )
        {
            if ( filter == null )
            {
                throw new ArgumentNullException( nameof( filter ) );
            }

            var connectionTypeService = new ConnectionTypeService( rockContext );

            var qry = connectionTypeService.Queryable();

            if ( filter.ConnectorPersonIds != null && filter.ConnectorPersonIds.Any() )
            {
                var connectorRequestsQry = new ConnectionRequestService( rockContext ).Queryable()
                    .Where( r => r.ConnectionState != ConnectionState.Connected
                        && r.ConnectorPersonAliasId.HasValue
                        && filter.ConnectorPersonIds.Contains( r.ConnectorPersonAlias.PersonId ) )
                    .Select( r => r.Id );

                qry = qry.Where( t => t.ConnectionOpportunities.SelectMany( o => o.ConnectionRequests ).Any( r => connectorRequestsQry.Contains( r.Id ) ) );
            }

            if ( !filter.IncludeInactive )
            {
                qry = qry.Where( t => t.IsActive && t.IsActive );
            }

            return qry;
        }

        /// <summary>
        /// Gets the type request counts for the given connection types.
        /// </summary>
        /// <param name="connectionTypeIds">The connection type identifiers.</param>
        /// <param name="currentPerson">The current person to use for count checks.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A dictionary of connection request count objects.</returns>
        internal static Dictionary<int, ConnectionOpportunityList.ConnectionRequestCountsViewModel> GetConnectionTypeCounts( List<int> connectionTypeIds, Person currentPerson, RockContext rockContext )
        {
            var opportunities = new ConnectionOpportunityService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( o => connectionTypeIds.Contains( o.ConnectionTypeId ) )
                .ToList();

            var requestCounts = ConnectionOpportunityList.GetOpportunityRequestCounts( opportunities, currentPerson, rockContext )
                .Select( c => new
                {
                    TypeId = opportunities.Single( o => o.Id == c.Key ).ConnectionTypeId,
                    Counts = c.Value
                } )
                .GroupBy( c => c.TypeId )
                .ToDictionary( g => g.Key, g => new ConnectionOpportunityList.ConnectionRequestCountsViewModel
                {
                    AssignedToYouCount = g.Sum( c => c.Counts.AssignedToYouCount )
                } );

            // Fill in any missing types with empty counts.
            foreach ( var typeId in connectionTypeIds )
            {
                if ( !requestCounts.ContainsKey( typeId ) )
                {
                    requestCounts.Add( typeId, new ConnectionOpportunityList.ConnectionRequestCountsViewModel() );
                }
            }

            return requestCounts;
        }

        /// <summary>
        /// Gets the connection types view model that can be sent to the client.
        /// </summary>
        /// <returns>The <see cref="GetContentViewModel"/> that contains the information about the response.</returns>
        private GetContentViewModel GetConnectionTypes( GetConnectionTypesFilterViewModel filterViewModel )
        {
            using ( var rockContext = new RockContext() )
            {
                var filter = new GetConnectionTypesFilter
                {
                    IncludeInactive = true
                };

                if ( filterViewModel.OnlyMyConnections )
                {
                    filter.ConnectorPersonIds = new List<int> { RequestContext.CurrentPerson?.Id ?? 0 };
                }

                var qry = GetConnectionTypesQuery( filter, rockContext );

                // Make a list of any type identifiers that are configured
                // for request security and the person is assigned as the
                // connector to any request.
                var currentPersonId = RequestContext.CurrentPerson?.Id;
                var selfAssignedSecurityTypes = new ConnectionRequestService( rockContext )
                    .Queryable()
                    .Where( r => r.ConnectorPersonAlias.PersonId == currentPersonId
                        && r.ConnectionOpportunity.ConnectionType.EnableRequestSecurity )
                    .Select( r => r.ConnectionOpportunity.ConnectionTypeId )
                    .Distinct()
                    .ToList();

                // Put all the types in memory so we can check security.
                var types = qry.ToList()
                    .Where( o => o.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson )
                        || selfAssignedSecurityTypes.Contains( o.Id ) )
                    .ToList();

                // Get the various counts to make available to the Lava template.
                var connectionTypeIds = types.Select( t => t.Id ).ToList();
                var requestCounts = GetConnectionTypeCounts( connectionTypeIds, RequestContext.CurrentPerson, rockContext );

                // Process the connection opportunities with the template.
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "ConnectionTypes", types );
                mergeFields.AddOrReplace( "DetailPage", DetailPageGuid );
                mergeFields.AddOrReplace( "ConnectionRequestCounts", requestCounts );

                var content = TypeTemplate.ResolveMergeFields( mergeFields );

                // If we found a connection opportunity then process the header
                // template.
                mergeFields = RequestContext.GetCommonMergeFields();

                var headerContent = HeaderTemplate.ResolveMergeFields( mergeFields );

                return new GetContentViewModel
                {
                    HeaderContent = headerContent,
                    Content = content
                };
            }
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Gets the connection types to be displayed.
        /// </summary>
        /// <returns>A response that describes the result of the operation.</returns>
        [BlockAction]
        public BlockActionResult GetContent()
        {
            return ActionOk( GetConnectionTypes( new GetConnectionTypesFilterViewModel() ) );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The filtering options when getting connection types.
        /// </summary>
        internal class GetConnectionTypesFilter
        {
            /// <summary>
            /// Gets or sets a value indicating whether inactive types
            /// should be included.
            /// </summary>
            /// <value>
            ///   <c>true</c> if inactive types are included; otherwise, <c>false</c>.
            /// </value>
            public bool IncludeInactive { get; set; }

            /// <summary>
            /// Gets or sets the connector person identifiers to limit the
            /// results to. If an type does not have a non-connected
            /// request that is assigned to one of these identifiers it will
            /// not be included.
            /// </summary>
            /// <value>
            /// The connector person identifiers.
            /// </value>
            public List<int> ConnectorPersonIds { get; set; }
        }

        /// <summary>
        /// The view model that defines the filtering options when getting types.
        /// </summary>
        public class GetConnectionTypesFilterViewModel
        {
            /// <summary>
            /// Gets or sets a value indicating whether to only show connection
            /// types that have any requests with the logged in person marked
            /// as the connector.
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

        #endregion
    }
}
