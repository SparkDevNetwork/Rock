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
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Security.BackgroundCheck.CheckrRequestList;
using Rock.Web.Cache;

namespace Rock.Blocks.Security.BackgroundCheck
{
    /// <summary>
    /// Lists all the Checkr background check requests.
    /// </summary>
    [DisplayName( "Checkr Request List" )]
    [Category( "Security > Background Check" )]
    [Description( "Lists all the Checkr background check requests." )]
    [IconCssClass( "ti ti-file-type-txt" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Workflow Detail Page",
        Description = "The page to view details about the background check workflow.",
        Key = AttributeKey.WorkflowDetailPage )]

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "2e0c5dfa-cf1e-4ea1-80e9-bb94fbcb782d" )]
    // was [SystemGuid.BlockTypeGuid( "59e29033-7820-4e9d-9512-b0f7973dd501" )]
    [Rock.SystemGuid.BlockTypeGuid( "53A28B56-B7B4-472C-9305-1DC66693A6C6" )]
    [CustomizedGrid]
    public class CheckrRequestList : RockEntityListBlockType<Model.BackgroundCheck>
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
            var box = new ListBlockBox<CheckrRequestListOptionsBag>();
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
        private CheckrRequestListOptionsBag GetBoxOptions()
        {
            return new CheckrRequestListOptionsBag();
        }

        /// <inheritdoc/>
        protected override IQueryable<Model.BackgroundCheck> GetListQueryable( RockContext rockContext )
        {
            return new BackgroundCheckService( rockContext ).Queryable()
                .AsNoTracking()
                .Include( b => b.PersonAlias.Person )
                .Where( b => b.PersonAlias != null && b.PersonAlias.Person != null )
                .Where( b => b.ForeignId == 2 );
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
                .AddPersonField( "name", b => b.PersonAlias?.Person )
                .AddDateTimeField( "requestDate", b => b.RequestDate )
                .AddDateTimeField( "responseDate", b => b.ResponseDate )
                .AddTextField( "reportStatus", b => b.Status.IsNotNullOrWhiteSpace() ? b.Status.SplitCase() : string.Empty )
                .AddField( "isRecordFound", b => b.RecordFound )
                .AddField( "hasReport", b => b.RecordFound.HasValue && b.RecordFound.Value && b.ResponseId.IsNotNullOrWhiteSpace() )
                .AddField( "hasWorkflow", b => b.WorkflowId.HasValue );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the Checkr report URL for the specified background check.
        /// </summary>
        /// <param name="key">The identifier of the background check.</param>
        /// <returns>The report URL string, or an error.</returns>
        [BlockAction]
        public BlockActionResult GetReportUrl( string key )
        {
            var entity = new BackgroundCheckService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( "Background check not found." );
            }

            if ( entity.ResponseId.IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "No report is available for this background check." );
            }

            var component = BackgroundCheckContainer.GetComponent( "Rock.Checkr.Checkr" );
            if ( component == null )
            {
                return ActionBadRequest( "The Checkr background check component is not available." );
            }

            var url = component.GetReportUrl( entity.ResponseId );
            if ( url.IsNullOrWhiteSpace() || url == "Unauthorized" )
            {
                return ActionBadRequest( "Unable to retrieve the report URL." );
            }

            return ActionOk( url );
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
