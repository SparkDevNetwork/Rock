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
using Rock.ClientService.Connection.ConnectionOpportunity;
using Rock.Data;
using Rock.Model;
using Rock.Model.Connection.ConnectionOpportunity.Options;
using Rock.Security;
using Rock.Utility;

namespace Rock.Blocks.Types.Mobile.Connection
{
    /// <summary>
    /// Displays the list of connection opportunities for a single connection type.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Opportunity List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the list of connection opportunities for a single connection type." )]
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

    [BooleanField(
        "Include Inactive",
        Description = "Whether or not to filter out inactive opportunities.",
        IsRequired = false,
        Key = AttributeKey.IncludeInactive,
        DefaultBooleanValue = false,
        Order = 3)]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_OPPORTUNITY_LIST )]
    public class ConnectionOpportunityList : RockBlockType
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

            public const string IncludeInactive = "IncludeInactive";
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

        /// <summary>
        /// Gets a value indicating whether or not to include inactive opportunities.
        /// </summary>
        /// <value><c>true</c> if include inactive; otherwise, <c>false</c>.</value>
        protected bool IncludeInactive => GetAttributeValue( AttributeKey.IncludeInactive ).AsBoolean();

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
        /// Gets the connection opportunities view model that can be sent to the client.
        /// </summary>
        /// <param name="connectionTypeGuid">The connection type unique identifier.</param>
        /// <param name="filterViewModel">The filter.</param>
        /// <returns>The <see cref="GetContentViewModel"/> that contains the information about the response.</returns>
        private GetContentViewModel GetConnectionOpportunities( Guid connectionTypeGuid, GetConnectionOpportunitiesFilterViewModel filterViewModel )
        {
            using ( var rockContext = new RockContext() )
            {
                var opportunityService = new ConnectionOpportunityService( rockContext );
                var opportunityClientService = new ConnectionOpportunityClientService( rockContext, RequestContext.CurrentPerson );
                var connectionType = new ConnectionTypeService( rockContext ).GetNoTracking( connectionTypeGuid );

                var filterOptions = new ConnectionOpportunityQueryOptions
                {
                    ConnectionTypeGuids = new List<Guid> { connectionTypeGuid },
                    IncludeInactive = IncludeInactive
                };

                if ( filterViewModel.OnlyMyConnections )
                {
                    filterOptions.ConnectorPersonIds = new List<int> { RequestContext.CurrentPerson?.Id ?? 0 };
                }

                // Put all the opportunities in memory so we can check security.
                var qry = opportunityService.GetConnectionOpportunitiesQuery( filterOptions );
                var opportunities = qry.ToList()
                    .Where( o => o.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) );

                // Get the various counts to make available to the Lava template.
                // The conversion of the value to a dictionary is a temporary work-around
                // until we have a way to mark external types as lava safe.
                var opportunityIds = opportunities.Select( o => o.Id ).ToList();
                var requestCounts = opportunityClientService.GetOpportunityRequestCounts( opportunityIds )
                    .ToDictionary( k => k.Key, k => new RockDynamic( k.Value ) );

                // Process the connection opportunities with the template.
                var mergeFields = RequestContext.GetCommonMergeFields();
                mergeFields.AddOrReplace( "ConnectionOpportunities", opportunities );
                mergeFields.AddOrReplace( "DetailPage", DetailPageGuid );
                mergeFields.AddOrReplace( "ConnectionRequestCounts", requestCounts );

                var content = OpportunityTemplate.ResolveMergeFields( mergeFields );

                // Process the header template for.
                mergeFields = RequestContext.GetCommonMergeFields();
                if ( connectionType != null )
                {
                    mergeFields.Add( "ConnectionType", connectionType );
                }

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

        #endregion
    }
}
