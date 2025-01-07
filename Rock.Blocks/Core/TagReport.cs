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
using System.Data.Entity;
using System.Linq;
using System.Reflection;

using DotLiquid;

using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.TaggedItemList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of tagged items.
    /// </summary>
    [DisplayName( "Tag Report" )]
    [Category( "Core" )]
    [Description( "Block for viewing entities with a selected tag" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "a9017995-c914-4ee4-b1ba-a852d162bdd7" )]
    [Rock.SystemGuid.BlockTypeGuid( "f140b415-9bb3-4492-844e-5a529517a484" )]
    public class TagReport : RockListBlockType<TaggedItemListBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string TagId = "TagId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<TaggedItemListOptionsBag>();
            var service = new TaggedItemService( RockContext );

            var tagId = PageParameter( PageParameterKey.TagId );
            var tag = new TagService( RockContext ).Get( tagId );
            if ( tag == null || !tag.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                box.Options.IsBlockHidden = true;
                return box;
            }

            var tagEntityType = EntityTypeCache.Get( tag?.EntityTypeId ?? 0 );
            if ( tagEntityType != null )
            {
                if ( tagEntityType.Name == "Rock.Model.Person" )
                {
                    box.IsAddEnabled = tag.IsAuthorized( Authorization.TAG, RequestContext.CurrentPerson );
                }

                var entityType = tagEntityType.GetEntityType();
                if ( entityType != null )
                {
                    box.Options.Title = "Tagged " + entityType.Name.Pluralize().SplitCase();
                    box.Options.TagName = tag.Name;
                    box.Options.EntityTypeName = entityType.Name;
                    box.Options.EntityTypeGuid = tagEntityType.Guid.ToStringSafe();
                    box.Options.IsPersonTag = tagEntityType.Name == "Rock.Model.Person";
                    box.Options.TagId = tag.Id;
                }
            }

            var builder = GetGridBuilder();

            box.IsDeleteEnabled = tag.IsAuthorized( Authorization.TAG, RequestContext.CurrentPerson );
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private TaggedItemListOptionsBag GetBoxOptions()
        {
            var options = new TaggedItemListOptionsBag();

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
            };
        }


        /// <inheritdoc/>
        protected override GridBuilder<TaggedItemListBag> GetGridBuilder()
        {
            return new GridBuilder<TaggedItemListBag>()
                .WithBlock( this )
                .AddDateTimeField( "dateTagged", a => a.CreatedDateTime )
                .AddTextField( "entityName", a => GetItemName( a.EntityTypeId, a.EntityGuid ) )
                .AddTextField( "entityGuid", a => a.EntityGuid.ToString() )
                .AddField( "entityId", a => a.EntityId.ToString() );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string entityGuid, int tagId )
        {
            var entityService = new TaggedItemService( RockContext );

            var entity = entityService.Queryable()
                .FirstOrDefault( t => t.TagId == tagId && t.EntityGuid.ToString() == entityGuid );

            if ( entity == null )
            {
                return ActionBadRequest( $"{TaggedItem.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.TAG, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {TaggedItem.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        [BlockAction]
        public BlockActionResult AddPerson( ListItemBag personAlias )
        {
            var tagService = new TagService( RockContext );
            var taggedItemService = new TaggedItemService( RockContext );
            var tagId = PageParameter( PageParameterKey.TagId );
            var tag = new TagService( RockContext ).Get( tagId );

            var personAliasGuid = personAlias.Value.AsGuid();
            var personAliasService = new PersonAliasService( RockContext );
            var personGuid = personAliasService.GetPerson( personAliasGuid ).Guid;

            if ( taggedItemService.Get( tag.Id, personGuid ) != null )
            {
                return ActionConflict( "Person already exists in the tag." );
            }

            var tagEntityTypeId = tagService.Queryable()
                    .Where( t => t.Id == tag.Id )
                    .Select( t => t.EntityTypeId )
                    .FirstOrDefault() ?? 0;
            var taggedItem = new TaggedItem
            {
                TagId = tag.Id,
                EntityTypeId = tagEntityTypeId,
                EntityGuid = personGuid
            };
            taggedItemService.Add( taggedItem );
            RockContext.SaveChanges();

            var newTaggedItem = taggedItemService.Get( taggedItem.Id );
            var newTaggedItemBag = new TaggedItemListBag
            {
                Id = newTaggedItem.Id,
                EntityTypeId = newTaggedItem.EntityTypeId,
                EntityGuid = newTaggedItem.EntityGuid,
                CreatedDateTime = newTaggedItem.CreatedDateTime
            };

            var gridData = GetGridBuilder().Build( new List<TaggedItemListBag> { newTaggedItemBag } );

            return ActionOk( gridData.Rows[0] );
        }

        [BlockAction]
        public BlockActionResult SelectRow( string entityGuid, int tagId )
        {
            var taggedItem = new TaggedItemService( RockContext ).Queryable()
                .AsNoTracking()
                .Where ( t => t.TagId == tagId && t.EntityGuid.ToString() == entityGuid )
                .FirstOrDefault();
            if ( taggedItem == null )
            {
                return ActionNotFound();
            }

            var entityType = EntityTypeCache.Get( taggedItem.EntityTypeId );
            if ( entityType == null )
            {
                return ActionNotFound();
            }
            var entity = Reflection.GetIEntityForEntityType( entityType.GetEntityType(), taggedItem.EntityGuid );
            if ( entity == null )
            {
                ActionNotFound();
            }

            string url = string.Format( "~/{0}/{1}", entityType.FriendlyName.Replace( " ", "" ), entity.Id );
            if ( entityType.LinkUrlLavaTemplate.IsNotNullOrWhiteSpace() )
            {
                url = entityType.LinkUrlLavaTemplate.ResolveMergeFields( new Dictionary<string, object> { { "Entity", entity } } );
            }

            return ActionOk( url );
        }

        #endregion

        protected override IQueryable<TaggedItemListBag> GetListQueryable( RockContext rockContext )
        {
            var tagId = PageParameter( PageParameterKey.TagId );
            var tag = new TagService( RockContext ).Get( tagId );

            var tagEntityType = EntityTypeCache.Get( tag?.EntityTypeId ?? 0 );
            var service = new TaggedItemService( rockContext );
            if ( tagEntityType == null )
            {
                return Enumerable.Empty<TaggedItemListBag>().AsQueryable();
            }
            var entityType = tagEntityType.GetEntityType();
            IService serviceInstance = Reflection.GetServiceForEntityType( entityType, RockContext );
            MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
            var entityQuery = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

            IQueryable<TaggedItemListBag> results = service.Queryable().AsNoTracking()
                .Where( t => t.TagId == tag.Id )
                    .Join( entityQuery, t => t.EntityGuid, e => e.Guid, ( t, e ) => new TaggedItemListBag
                    {
                        Id = t.Id,
                        EntityTypeId = t.EntityTypeId,
                        EntityGuid = t.EntityGuid,
                        CreatedDateTime = t.CreatedDateTime,
                        EntityId = e.Id
                    } );
            return results;
        }

        /// <summary>
        /// Gets the name of the item.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <returns></returns>
        public string GetItemName( int entityTypeId, Guid entityGuid )
        {
            var entityType = EntityTypeCache.Get( entityTypeId );
            if ( entityType != null )
            {
                var entity = Reflection.GetIEntityForEntityType( entityType.GetEntityType(), entityGuid );
                if ( entity != null )
                {
                    return entity.ToString();
                }
            }

            return "Item?";
        }
    }
}
