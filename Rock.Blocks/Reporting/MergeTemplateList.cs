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
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Reporting.MergeTemplateList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reporting
{
    /// <summary>
    /// Displays a list of merge templates.
    /// </summary>
    [DisplayName( "Merge Template List" )]
    [Category( "Core" )]
    [Description( "Displays a list of all merge templates." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the merge template details.",
        Key = AttributeKey.DetailPage )]

    [EnumField( "Merge Templates Ownership",
        Description = "Set this to limit to merge templates depending on ownership type.",
        EnumSourceType = typeof( MergeTemplateOwnership ),
        IsRequired = true,
        DefaultEnumValue = (int) MergeTemplateOwnership.Personal,
        Key = AttributeKey.MergeTemplatesOwnership )]

    [Rock.SystemGuid.EntityTypeGuid( "edaaaf0c-ba30-40c9-8e7c-9d1118fefd87" )]
    [Rock.SystemGuid.BlockTypeGuid( "740f7de3-d5f5-4eeb-beee-99c3bfb23b52" )]
    [CustomizedGrid]
    public class MergeTemplateList : RockEntityListBlockType<MergeTemplate>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string MergeTemplatesOwnership = "MergeTemplatesOwnership";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterPerson = "filter-person";
            public const string FilterShowGlobalTemplates = "filter-show-global-templates";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the person the MergeTemplates returned in the result should belong to.
        /// </summary>
        /// <value>
        /// The filter person.
        /// </value>
        protected Guid? FilterPerson => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterPerson )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the filter indicating whether global templates should be included in the results.
        /// </summary>
        /// <value>
        /// The filter show global templates.
        /// </value>
        protected string FilterShowGlobalTemplates => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterShowGlobalTemplates );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<MergeTemplateListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = GetIsAddEditDeleteEnabled();
            box.IsDeleteEnabled = GetIsAddEditDeleteEnabled();
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
        private MergeTemplateListOptionsBag GetBoxOptions()
        {
            var options = new MergeTemplateListOptionsBag()
            {
                MergeTemplateOwnership = ( Enums.Controls.MergeTemplateOwnership ) GetTemplateOwnership()
            };

            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEditDeleteEnabled()
        {
            var mergeTemplateOwnership = GetTemplateOwnership();
            return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || mergeTemplateOwnership == MergeTemplateOwnership.Personal;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "MergeTemplateId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<MergeTemplate> GetListQueryable( RockContext rockContext )
        {
            var service = new MergeTemplateService( rockContext );

            var queryable = service.Queryable();

            var mergeTemplateOwnership = GetTemplateOwnership();

            // Only Authorization.EDIT should be able to use the grid filter
            if ( mergeTemplateOwnership == MergeTemplateOwnership.Personal )
            {
                var currentPersonId = GetCurrentPerson()?.Id ?? 0;
                queryable = queryable.Where( a => a.PersonAlias.PersonId == currentPersonId );
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.PersonalAndGlobal )
            {
                var showGlobalMergeTemplates = FilterShowGlobalTemplates.AsBooleanOrNull() ?? true;

                if ( FilterPerson.HasValue )
                {
                    if ( showGlobalMergeTemplates )
                    {
                        queryable = queryable.Where( a => !a.PersonAliasId.HasValue || a.PersonAlias.Guid == FilterPerson.Value );
                    }
                    else
                    {
                        queryable = queryable.Where( a => a.PersonAliasId.HasValue && a.PersonAlias.Guid == FilterPerson.Value );
                    }
                }
                else
                {
                    if ( showGlobalMergeTemplates )
                    {
                        queryable = queryable.Where( a => !a.PersonAliasId.HasValue );
                    }
                    else
                    {
                        queryable = queryable.Where( a => a.PersonAliasId.HasValue );
                    }
                }
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.Global )
            {
                queryable = queryable.Where( a => !a.PersonAliasId.HasValue );
            }

            return queryable;
        }

        /// <inheritdoc/>
        protected override IQueryable<MergeTemplate> GetOrderedListQueryable( IQueryable<MergeTemplate> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( m => m.Name );
        }

        /// <inheritdoc/>
        protected override List<MergeTemplate> GetListItems( IQueryable<MergeTemplate> queryable, RockContext rockContext )
        {
            var items = base.GetListItems( queryable, rockContext );
            var mergeTemplateOwnership = GetTemplateOwnership();
            if ( mergeTemplateOwnership == MergeTemplateOwnership.Global || mergeTemplateOwnership == MergeTemplateOwnership.PersonalAndGlobal )
            {
                items = items.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, GetCurrentPerson() ) ).ToList();
            }

            return items;
        }

        /// <summary>
        /// Gets the template ownership attribute setting value.
        /// </summary>
        /// <returns></returns>
        private MergeTemplateOwnership GetTemplateOwnership()
        {
            return this.GetAttributeValue( AttributeKey.MergeTemplatesOwnership ).ConvertToEnum<MergeTemplateOwnership>( MergeTemplateOwnership.Personal );
        }

        /// <inheritdoc/>
        protected override GridBuilder<MergeTemplate> GetGridBuilder()
        {
            return new GridBuilder<MergeTemplate>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "ownerIdKey", a => a.PersonAlias?.Person?.IdKey )
                .AddTextField( "ownerName", a => $"{a.PersonAlias?.Person?.NickName} {a.PersonAlias?.Person?.LastName}" );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new MergeTemplateService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{MergeTemplate.FriendlyTypeName} not found." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {MergeTemplate.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
