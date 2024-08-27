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
using System.Reflection;

using Rock.Attribute;
using Rock.ClientService.Connection.ConnectionType;
using Rock.Data;
using Rock.Model;
using Rock.Model.Connection.ConnectionType.Options;
using Rock.Utility;

namespace Rock.Blocks.Types.Mobile.Connection
{
    /// <summary>
    /// Displays the list of connection types.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Connection Type List" )]
    [Category( "Mobile > Connection" )]
    [Description( "Displays the list of connection types." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [CodeEditorField(
        "Header Template",
        Description = "Lava template used to render the header above the connection request.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.HeaderTemplate,
        DefaultValue = @"<ContentView>
    <Label Text=""Types""
           StyleClass=""title1, bold, text-interface-strongest"" />
</ContentView>",
        Order = 0 )]

    [BlockTemplateField( "Type Template",
        Description = "The template used to render the connection types.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CONNECTION_CONNECTION_TYPE_LIST,
        DefaultValue = "F9F29166-A080-4179-A210-AE42CC473D6F",
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

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CONNECTION_CONNECTION_TYPE_LIST )]
    public class ConnectionTypeList : RockBlockType
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
        /// Gets the connection types view model that can be sent to the client.
        /// </summary>
        /// <returns>The <see cref="GetContentViewModel"/> that contains the information about the response.</returns>
        private GetContentViewModel GetConnectionTypes( GetConnectionTypesFilterViewModel filterViewModel )
        {
            using ( var rockContext = new RockContext() )
            {
                var connectionTypeService = new ConnectionTypeService( rockContext );
                var clientTypeService = new ConnectionTypeClientService( rockContext, RequestContext.CurrentPerson );
                var filterOptions = new ConnectionTypeQueryOptions
                {
                    IncludeInactive = true
                };

                // If requesting to only have my connections returned then specify
                // the current person identifier. If they are not logged in then
                // use an invalid identifier value so that no matches will be returned.
                if ( filterViewModel.OnlyMyConnections )
                {
                    filterOptions.ConnectorPersonIds = new List<int> { RequestContext.CurrentPerson?.Id ?? 0 };
                }

                // Get the connection types.
                var qry = connectionTypeService.GetConnectionTypesQuery( filterOptions );
                var types = connectionTypeService.GetViewAuthorizedConnectionTypes( qry, RequestContext.CurrentPerson );

                // Get the various counts to make available to the Lava template.
                // The conversion of the value to a dictionary is a temporary work-around
                // until we have a way to mark external types as lava safe.
                var connectionTypeIds = types.Select( t => t.Id ).ToList();
                var requestCounts = clientTypeService.GetConnectionTypeCounts( connectionTypeIds )
                    .ToDictionary( k => k.Key, k => new RockDynamic( k.Value ) );

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
