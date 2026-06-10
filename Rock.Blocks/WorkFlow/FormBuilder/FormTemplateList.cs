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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormTemplateList;
using Rock.Web.Cache;

namespace Rock.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Displays a list of form builder templates.
    /// </summary>
    [DisplayName( "Form Template List" )]
    [Category( "WorkFlow > FormBuilder" )]
    [Description( "Shows a list of form templates." )]
    [IconCssClass( "ti ti-notes" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Detail Page",
        Description = "The page that will show the form template details.",
        Order = 0,
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "849C0F82-2946-4CD7-88EB-CA7EC17A0A6A" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "90D3C7F4-40AF-4C6E-B908-452372B98202" )]
    [Rock.SystemGuid.BlockTypeGuid( "1DEFF313-39CF-400F-895A-82ADB9F192BD" )]
    [CustomizedGrid]
    public class FormTemplateList : RockEntityListBlockType<WorkflowFormBuilderTemplate>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string FormTemplateId = "FormTemplateId";
        }

        private static class PersonPreferenceKey
        {
            public const string FilterActiveStatus = "filter-active-status";
        }

        /// <summary>
        /// The stored values for the active status filter preference.
        /// </summary>
        private static class ActiveStatus
        {
            public const string All = "All";
            public const string Active = "Active";
            public const string Inactive = "Inactive";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the stored active status filter preference. A blank value
        /// indicates no stored preference and defaults to showing only active
        /// templates (applied in <see cref="GetListQueryable"/>).
        /// </summary>
        protected string FilterActiveStatus => GetBlockPersonPreferences()
            .GetValue( PersonPreferenceKey.FilterActiveStatus );

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<FormTemplateListOptionsBag>();
            var builder = GetGridBuilder();

            // The block exposes add, delete, and security to anyone who can access
            // the block; access to the block is the only authorization boundary.
            // Matches Webforms.
            box.IsAddEnabled = true;
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private FormTemplateListOptionsBag GetBoxOptions()
        {
            var options = new FormTemplateListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.FormTemplateId, "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<WorkflowFormBuilderTemplate> GetListQueryable( RockContext rockContext )
        {
            var queryable = base.GetListQueryable( rockContext );

            var activeStatus = FilterActiveStatus;

            // No stored preference defaults to showing only active templates.
            if ( activeStatus.IsNullOrWhiteSpace() )
            {
                activeStatus = ActiveStatus.Active;
            }

            if ( activeStatus == ActiveStatus.Active )
            {
                return queryable.Where( t => t.IsActive );
            }

            if ( activeStatus == ActiveStatus.Inactive )
            {
                return queryable.Where( t => !t.IsActive );
            }

            // ActiveStatus.All returns every template without an IsActive filter.
            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<WorkflowFormBuilderTemplate> GetOrderedListQueryable( IQueryable<WorkflowFormBuilderTemplate> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( t => t.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<WorkflowFormBuilderTemplate> GetGridBuilder()
        {
            return new GridBuilder<WorkflowFormBuilderTemplate>()
                .WithBlock( this )
                .AddTextField( "idKey", t => t.IdKey )
                .AddTextField( "name", t => t.Name )
                .AddTextField( "description", t => t.Description )
                .AddField( "isActive", t => t.IsActive );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new WorkflowFormBuilderTemplateService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{WorkflowFormBuilderTemplate.FriendlyTypeName} not found." );
            }

            // CanDelete blocks removal while any workflow type still references the
            // template, which the database foreign key would otherwise reject.
            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
