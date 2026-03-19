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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.BackgroundCheck.RequestList;
using Rock.Web.Cache;

namespace Rock.Blocks.Security.BackgroundCheck
{
    /// <summary>
    /// Lists all the background check requests (for ForeignId 1, Protect My Ministry).
    /// </summary>
    [DisplayName( "Request List" )]
    [Category( "Security > Background Check" )]
    [Description( "Lists all the background check requests (for ForeignId 1, Protect My Ministry)." )]
    [IconCssClass( "ti ti-file-type-txt" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Workflow Detail Page",
        Description = "The page to view details about the background check workflow.",
        Key = AttributeKey.WorkflowDetailPage )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "1A4B96C1-CB60-46C7-B780-B6F509FD1F51" )]
    [Rock.SystemGuid.BlockTypeGuid( "A25BE440-6A54-4A8C-9359-74DB5AE7E5F3" )]

    [CustomizedGrid]
    public class RequestList : RockEntityListBlockType<Model.BackgroundCheck>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string WorkflowDetailPage = "WorkflowDetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<RequestListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private RequestListOptionsBag GetBoxOptions()
        {
            return new RequestListOptionsBag();
        }

        /// <inheritdoc/>
        protected override IQueryable<Model.BackgroundCheck> GetListQueryable( RockContext rockContext )
        {
            return new BackgroundCheckService( rockContext ).Queryable()
                .AsNoTracking()
                .Include( b => b.PersonAlias.Person )
                .Include( b => b.ResponseDocument )
                .Where( b => b.PersonAlias != null && b.PersonAlias.Person != null )
                .Where( b => b.ForeignId == 1 ); // PMM v1 and v2 use this field to indicate that the background check was initiated from PMM. This ensures that only PMM-initiated background checks are shown in this list.
        }

        /// <inheritdoc/>
        protected override IQueryable<Model.BackgroundCheck> GetOrderedListQueryable( IQueryable<Model.BackgroundCheck> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( b => b.RequestDate );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Model.BackgroundCheck> GetGridBuilder()
        {
            return new GridBuilder<Model.BackgroundCheck>()
                .WithBlock( this )
                .AddTextField( "idKey", b => b.IdKey )
                .AddPersonField( "name", b => b.PersonAlias.Person )
                .AddDateTimeField( "requestDate", b => b.RequestDate )
                .AddDateTimeField( "responseDate", b => b.ResponseDate )
                .AddField( "isRecordFound", b => b.RecordFound )
                .AddField( "hasResponseDocument", b => b.ResponseDocumentId.HasValue )
                .AddTextField( "responseDocumentUrl", b => b.ResponseDocumentId.HasValue ? FileUrlHelper.GetFileUrl( b.ResponseDocument.Guid ) : string.Empty )
                .AddField( "hasResponseData", b => !string.IsNullOrWhiteSpace( b.ResponseData ) )
                .AddField( "hasWorkflow", b => b.WorkflowId.HasValue );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the response data for the specified background check.
        /// </summary>
        /// <param name="key">The identifier of the background check.</param>
        /// <returns>The response data string, or an error.</returns>
        [BlockAction]
        public BlockActionResult GetResponseData( string key )
        {
            var entity = new BackgroundCheckService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( "Background check not found." );
            }

            return ActionOk( entity.ResponseData ?? string.Empty );
        }

        /// <summary>
        /// Gets the workflow detail page URL for the specified background check.
        /// </summary>
        /// <param name="key">The identifier of the background check.</param>
        /// <returns>The workflow page URL string, or an error.</returns>
        [BlockAction]
        public BlockActionResult GetWorkflowPageUrl( string key )
        {
            var entity = new BackgroundCheckService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( "Background check not found." );
            }

            if ( !entity.WorkflowId.HasValue )
            {
                return ActionBadRequest( "No workflow is associated with this background check." );
            }

            var queryParams = new Dictionary<string, string>
            {
                { "WorkflowId", entity.WorkflowId.Value.ToString() }
            };

            var url = this.GetLinkedPageUrl( AttributeKey.WorkflowDetailPage, queryParams );
            if ( url.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "The Workflow Detail Page is not configured." );
            }

            return ActionOk( url );
        }

        #endregion Block Actions
    }
}
