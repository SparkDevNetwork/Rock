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

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.HtmlContentApproval;
using Rock.ViewModels.Utility;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays a list of HTML content blocks that need approval.
    /// </summary>
    [DisplayName( "HTML Content Approval" )]
    [Category( "CMS" )]
    [Description( "Lists HTML content blocks that need approval." )]
    [IconCssClass( "fa fa-check" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [SecurityAction(
        Authorization.APPROVE,
        "The roles and/or users that have access to approve HTML content." )]

    [SystemGuid.EntityTypeGuid( "2FF50C7C-9F27-4D70-AF11-416457598BE1" )]
    // was [SystemGuid.BlockTypeGuid( "B74C43CE-8222-404F-AE0D-481EDFD6D494" )]
    [Rock.SystemGuid.BlockTypeGuid( "79E4D7D2-3F18-43A9-9A62-E02F09C6051C" )]
    [CustomizedGrid]
    public class HtmlContentApproval : RockEntityListBlockType<HtmlContent>
    {
        #region Keys

        private static class PersonPreferenceKey
        {
            public const string FilterSite = "filter-site";
            public const string FilterApprovalStatus = "filter-approval-status";
            public const string FilterApprovedBy = "filter-approved-by";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => this.GetBlockPersonPreferences();

        /// <summary>
        /// Gets the site filter value as a Guid string.
        /// </summary>
        protected string FilterSite => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.FilterSite );

        /// <summary>
        /// Gets the approval status filter value (All, Approved, or Unapproved).
        /// </summary>
        protected string FilterApprovalStatus => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.FilterApprovalStatus );

        /// <summary>
        /// Gets the approved by person Guid filter value, parsed from the
        /// JSON-serialized ListItemBag stored in preferences.
        /// </summary>
        protected System.Guid? FilterApprovedByGuid => BlockPersonPreferences
            .GetValue( PersonPreferenceKey.FilterApprovedBy )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<HtmlContentApprovalOptionsBag>();
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
        private HtmlContentApprovalOptionsBag GetBoxOptions()
        {
            var options = new HtmlContentApprovalOptionsBag
            {
                IsApproveVisible = IsPersonApproveAuthorized(),
                Sites = SiteCache.All()
                    .Where( s => s != null )
                    .OrderBy( s => s.Name )
                    .Select( s => new ListItemBag
                    {
                        Value = s.Guid.ToString(),
                        Text = s.Name
                    } )
                    .ToList()
            };

            return options;
        }

        /// <summary>
        /// Determines whether the current person has authorization to approve
        /// HTML content via the block's "Approve" security action.
        /// </summary>
        /// <returns><see langword="true"/> if the current person can approve; otherwise <see langword="false"/>.</returns>
        private bool IsPersonApproveAuthorized()
        {
            return BlockCache.IsAuthorized( Authorization.APPROVE, RequestContext.CurrentPerson );
        }

        /// <inheritdoc/>
        protected override IQueryable<HtmlContent> GetListQueryable( RockContext rockContext )
        {
            /*
                4/7/2026 - MSE

                The query joins HtmlContent to AttributeValue to find only content
                belonging to blocks where RequireApproval is set to True on the
                HTML Content block type. This mirrors the original WebForms logic.

                Reason: Only content requiring approval should appear in this list.
            */
            var entityTypeIdBlock = EntityTypeCache.Get( typeof( Block ), true, rockContext ).Id;
            var htmlContentBlockTypeId = BlockTypeCache.Get( SystemGuid.BlockType.HTML_CONTENT.AsGuid(), rockContext ).Id.ToString();

            var attributeValueQry = new AttributeValueService( rockContext ).Queryable()
                .Where( a => a.Attribute.Key == "RequireApproval" && a.Attribute.EntityTypeId == entityTypeIdBlock )
                .Where( a => a.Attribute.EntityTypeQualifierColumn == "BlockTypeId" && a.Attribute.EntityTypeQualifierValue == htmlContentBlockTypeId )
                .Where( a => a.Value == "True" )
                .Select( a => a.EntityId );

            var qry = base.GetListQueryable( rockContext )
                .Include( a => a.Block.Page.Layout.Site )
                .Include( a => a.Block.Layout.Site )
                .Include( a => a.Block.Site )
                .Include( a => a.ApprovedByPersonAlias.Person )
                .Where( a => a.BlockId.HasValue && attributeValueQry.Contains( a.BlockId.Value ) );

            // Filter by approval status. Default to unapproved when no
            // preference is saved since this block's purpose is reviewing
            // content that needs approval.
            var approvalStatus = FilterApprovalStatus;
            if ( approvalStatus.IsNullOrWhiteSpace() || approvalStatus == "Unapproved" )
            {
                qry = qry.Where( a => a.IsApproved == false );
            }
            else if ( approvalStatus == "Approved" )
            {
                qry = qry.Where( a => a.IsApproved == true );
            }

            // Filter by the person that approved the content.
            if ( IsPersonApproveAuthorized() && FilterApprovedByGuid.HasValue )
            {
                var approvedByGuid = FilterApprovedByGuid.Value;
                qry = qry.Where( a => a.ApprovedByPersonAlias != null && a.ApprovedByPersonAlias.Person.Guid == approvedByGuid );
            }

            // Filter by site.
            if ( !string.IsNullOrWhiteSpace( FilterSite ) )
            {
                var siteGuid = FilterSite.AsGuidOrNull();
                if ( siteGuid.HasValue )
                {
                    qry = qry.Where( a =>
                        ( a.Block.PageId.HasValue && a.Block.Page.Layout.Site.Guid == siteGuid.Value ) ||
                        ( a.Block.LayoutId.HasValue && a.Block.Layout.Site.Guid == siteGuid.Value ) ||
                        ( a.Block.SiteId.HasValue && a.Block.Site.Guid == siteGuid.Value ) );
                }
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override IQueryable<HtmlContent> GetOrderedListQueryable( IQueryable<HtmlContent> queryable, RockContext rockContext )
        {
            return queryable
                .OrderByDescending( a => a.ModifiedDateTime )
                .ThenBy( a => a.Id );
        }

        /// <inheritdoc/>
        protected override GridBuilder<HtmlContent> GetGridBuilder()
        {
            return new GridBuilder<HtmlContent>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "siteName", a => a.Block?.Page?.Layout?.Site?.Name ?? a.Block?.Layout?.Site?.Name ?? a.Block?.Site?.Name ?? string.Empty )
                .AddTextField( "pageName", a => a.Block?.Page?.InternalName ?? string.Empty )
                .AddDateTimeField( "modifiedDateTime", a => a.ModifiedDateTime )
                .AddField( "isApproved", a => a.IsApproved )
                .AddPersonField( "approvedByPersonAlias", a => a.IsApproved && a.ApprovedByPersonAlias != null ? a.ApprovedByPersonAlias.Person : null )
                .AddDateTimeField( "approvedDateTime", a => a.IsApproved ? a.ApprovedDateTime : null );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Toggles the approval status of an HTML content item.
        /// </summary>
        /// <param name="key">The identifier of the HTML content to toggle.</param>
        /// <returns>A result indicating if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ToggleApproval( string key )
        {
            var htmlContentService = new HtmlContentService( RockContext );
            var htmlContent = htmlContentService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( htmlContent == null )
            {
                return ActionBadRequest( "Unable to find the specified HTML content." );
            }

            if ( !IsPersonApproveAuthorized() )
            {
                return ActionBadRequest( "Not authorized to approve HTML content." );
            }

            // Toggle the approval state.
            if ( htmlContent.IsApproved )
            {
                htmlContent.IsApproved = false;
                htmlContent.ApprovedByPersonAliasId = null;
                htmlContent.ApprovedDateTime = null;
            }
            else
            {
                htmlContent.IsApproved = true;
                htmlContent.ApprovedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                htmlContent.ApprovedDateTime = RockDateTime.Now;
            }

            RockContext.SaveChanges();

            // Flush the cached content so the change is reflected immediately.
            if ( htmlContent.BlockId.HasValue )
            {
                HtmlContentService.FlushCachedContent( htmlContent.BlockId.Value, htmlContent.EntityValue );
            }

            return ActionOk();
        }

        /// <summary>
        /// Gets the HTML content for previewing in a modal.
        /// </summary>
        /// <param name="key">The identifier of the HTML content to preview.</param>
        /// <returns>The HTML content string.</returns>
        [BlockAction]
        public BlockActionResult GetContentPreview( string key )
        {
            var htmlContentService = new HtmlContentService( RockContext );
            var htmlContent = htmlContentService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( htmlContent == null )
            {
                return ActionBadRequest( "Unable to find the specified HTML content." );
            }

            return ActionOk( htmlContent.Content ?? string.Empty );
        }

        #endregion Block Actions
    }
}
