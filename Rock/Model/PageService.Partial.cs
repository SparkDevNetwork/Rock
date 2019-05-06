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
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for the <see cref="Rock.Model.Page"/> model object. This class inherits from the Service class.
    /// </summary>
    public partial class PageService 
    {

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Page" /> entities by the parent <see cref="Rock.Model.Page">Page's</see> Id.
        /// </summary>
        /// <param name="parentPageId">The Id of the Parent <see cref="Rock.Model.Page" /> to search by.</param>
        /// <param name="includes">The includes.</param>
        /// <returns>
        /// An enumerable list of <see cref="Rock.Model.Page" /> entities who's ParentPageId matches the provided value.
        /// </returns>
        public IOrderedQueryable<Page> GetByParentPageId( int? parentPageId, string includes = null )
        {
            return Queryable( includes ).Where( t => ( t.ParentPageId == parentPageId || ( parentPageId == null && t.ParentPageId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Page" /> entities associated with a <see cref="Rock.Model.Layout" />.
        /// </summary>
        /// <param name="layoutId">The layout id.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Page">Pages</see> that use the provided layout.
        /// </returns>
        public IOrderedQueryable<Page> GetByLayoutId( int? layoutId )
        {
            return Queryable().Where( t => t.LayoutId == layoutId ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Page" /> entities associated with a <see cref="Rock.Model.Site" />.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Page">Pages</see> that use the given site.
        /// </returns>
        public IOrderedQueryable<Page> GetBySiteId( int? siteId )
        {
            return Queryable().Where( t => t.Layout.SiteId == siteId ).OrderBy( t => t.Layout.Name ).ThenBy( t => t.InternalName );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Page">Pages</see> that are descendants of a <see cref="Rock.Model.Page"/>
        /// </summary>
        /// <param name="parentPageId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Page"/></param>
        /// <returns>A collection of <see cref="Rock.Model.Page"/> entities that are descendants of the provided parent <see cref="Rock.Model.Page"/>.</returns>
        public IEnumerable<Page> GetAllDescendents( int parentPageId )
        {
            return ExecuteQuery(
                @"
                with CTE as (
                select * from [Page] where [ParentPageId]={0}
                union all
                select [a].* from [Page] [a]
                inner join CTE pcte on pcte.Id = [a].[ParentPageId]
                )
                select * from CTE
                ", parentPageId );
        }

        /// <summary>
        /// Determines whether the specified page can be deleted.
        /// Performs some additional checks that are missing from the
        /// auto-generated PageService.CanDelete().
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="includeSecondLvl">If set to true, verifies that the item is not referenced by any second level relationships.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Page item, out string errorMessage, bool includeSecondLvl )
        {
            errorMessage = string.Empty;

            bool canDelete = CanDelete( item, out errorMessage );

            var site = new Service<Site>( this.Context ).Queryable().Where( s => ( s.DefaultPageId == item.Id || s.LoginPageId == item.Id
                || s.RegistrationPageId == item.Id || s.PageNotFoundPageId == item.Id ) ).FirstOrDefault();
            if ( canDelete && includeSecondLvl && site != null )
            {
                errorMessage = string.Format( "This {0} is used by a special page on the {1} {2}.", Page.FriendlyTypeName, site.Name, Site.FriendlyTypeName );
                canDelete = false;
            }

            return canDelete;
        }

        /// <summary>
        /// Gets the Guid for the Page that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = PageCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }

        #region Page Copy Methods

        /// <summary>
        /// Copies the page.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="includeChildPages">if set to <c>true</c> [include child pages].</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <returns></returns>
        public Guid? CopyPage( int pageId, bool includeChildPages, int? currentPersonAliasId = null )
        {
            var rockContext = new RockContext();
            var pageService = new PageService( rockContext );
            Guid? newPageGuid = null;

            var page = pageService.Get( pageId );
            if ( page != null )
            {
                Dictionary<Guid, Guid> pageGuidDictionary = new Dictionary<Guid, Guid>();
                Dictionary<Guid, Guid> blockGuidDictionary = new Dictionary<Guid, Guid>();
                var newPage = GeneratePageCopy( page, pageGuidDictionary, blockGuidDictionary, includeChildPages, currentPersonAliasId );

                pageService.Add( newPage );
                rockContext.SaveChanges();
                newPageGuid= newPage.Guid;

                GenerateBlockAttributeValues( pageGuidDictionary, blockGuidDictionary, rockContext, currentPersonAliasId );
                GeneratePageBlockAuths( pageGuidDictionary, blockGuidDictionary, rockContext, currentPersonAliasId );
                CloneHtmlContent( blockGuidDictionary, rockContext, currentPersonAliasId );
            }

            return newPageGuid;
        }

        /// <summary>
        /// Copies the page.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <returns></returns>
        public Guid? CopyPage( int pageId, int? currentPersonAliasId = null )
        {
            return CopyPage( pageId, true, currentPersonAliasId );
        }

        /// <summary>
        /// This method generates a copy of the given page along with any descendant pages, as well as any blocks on
        /// any of those pages.
        /// </summary>
        /// <param name="sourcePage">The source page.</param>
        /// <param name="pageGuidDictionary">The dictionary containing the original page guids and the corresponding copied page guids.</param>
        /// <param name="blockGuidDictionary">The dictionary containing the original block guids and the corresponding copied block guids.</param>
        /// <param name="includeChildPages">if set to <c>true</c> [include child pages].</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <param name="isRootOfTheCopyOperation">Is this source page the root of the copy operation. Recursive calls to this method set this param as false.</param>
        /// <returns></returns>
        private Page GeneratePageCopy( Page sourcePage, Dictionary<Guid, Guid> pageGuidDictionary, Dictionary<Guid, Guid> blockGuidDictionary, bool includeChildPages, int? currentPersonAliasId = null, bool isRootOfTheCopyOperation = true )
        {
            var targetPage = sourcePage.Clone( false );
            targetPage.CreatedByPersonAlias = null;
            targetPage.CreatedByPersonAliasId = currentPersonAliasId;
            targetPage.CreatedDateTime = RockDateTime.Now;
            targetPage.ModifiedByPersonAlias = null;
            targetPage.ModifiedByPersonAliasId = currentPersonAliasId;
            targetPage.ModifiedDateTime = RockDateTime.Now;
            targetPage.BodyCssClass = sourcePage.BodyCssClass;
            targetPage.Id = 0;
            targetPage.Guid = Guid.NewGuid();
            targetPage.IsSystem = false;
            pageGuidDictionary.Add( sourcePage.Guid, targetPage.Guid );

            if ( isRootOfTheCopyOperation )
            {
                targetPage.InternalName = sourcePage.InternalName + " - Copy";
            }

            foreach ( var block in sourcePage.Blocks )
            {
                var newBlock = block.Clone( false );
                newBlock.CreatedByPersonAlias = null;
                newBlock.CreatedByPersonAliasId = currentPersonAliasId;
                newBlock.CreatedDateTime = RockDateTime.Now;
                newBlock.ModifiedByPersonAlias = null;
                newBlock.ModifiedByPersonAliasId = currentPersonAliasId;
                newBlock.ModifiedDateTime = RockDateTime.Now;
                newBlock.Id = 0;
                newBlock.Guid = Guid.NewGuid();
                newBlock.PageId = 0;
                newBlock.IsSystem = false;

                blockGuidDictionary.Add( block.Guid, newBlock.Guid );
                targetPage.Blocks.Add( newBlock );
            }

            if ( includeChildPages )
            {
                foreach ( var oldchildPage in sourcePage.Pages )
                {
                    targetPage.Pages.Add( GeneratePageCopy( oldchildPage, pageGuidDictionary, blockGuidDictionary, includeChildPages, currentPersonAliasId, false ) );
                }
            }

            return targetPage;
        }

        /// <summary>
        /// Copies any auths for the original pages and blocks over to the copied pages and blocks.
        /// </summary>
        /// <param name="pageGuidDictionary">The dictionary containing the original page guids and the corresponding copied page guids.</param>
        /// <param name="blockGuidDictionary">The dictionary containing the original block guids and the corresponding copied block guids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        private void GeneratePageBlockAuths( Dictionary<Guid, Guid> pageGuidDictionary, Dictionary<Guid, Guid> blockGuidDictionary, RockContext rockContext, int? currentPersonAliasId = null )
        {
            var authService = new AuthService( rockContext );
            var pageService = new PageService( rockContext );
            var blockService = new BlockService( rockContext );
            var pageGuid = Rock.SystemGuid.EntityType.PAGE.AsGuid();
            var blockGuid = Rock.SystemGuid.EntityType.BLOCK.AsGuid();

            Dictionary<Guid, int> pageIntDictionary = pageService.Queryable()
                .Where( p => pageGuidDictionary.Keys.Contains( p.Guid ) || pageGuidDictionary.Values.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid, p => p.Id );

            Dictionary<Guid, int> blockIntDictionary = blockService.Queryable()
                .Where( p => blockGuidDictionary.Keys.Contains( p.Guid ) || blockGuidDictionary.Values.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid, p => p.Id );

            var pageAuths = authService.Queryable().Where( a =>
                a.EntityType.Guid == pageGuid && pageIntDictionary.Values.Contains( a.EntityId.Value ) )
                .ToList();

            var blockAuths = authService.Queryable().Where( a =>
                a.EntityType.Guid == blockGuid && blockIntDictionary.Values.Contains( a.EntityId.Value ) )
                .ToList();

            foreach ( var pageAuth in pageAuths )
            {
                var newPageAuth = pageAuth.Clone( false );
                newPageAuth.CreatedByPersonAlias = null;
                newPageAuth.CreatedByPersonAliasId = currentPersonAliasId;
                newPageAuth.CreatedDateTime = RockDateTime.Now;
                newPageAuth.ModifiedByPersonAlias = null;
                newPageAuth.ModifiedByPersonAliasId = currentPersonAliasId;
                newPageAuth.ModifiedDateTime = RockDateTime.Now;
                newPageAuth.Id = 0;
                newPageAuth.Guid = Guid.NewGuid();
                newPageAuth.EntityId = pageIntDictionary[pageGuidDictionary[pageIntDictionary.Where( d => d.Value == pageAuth.EntityId.Value ).FirstOrDefault().Key]];
                authService.Add( newPageAuth );
            }

            foreach ( var blockAuth in blockAuths )
            {
                var newBlockAuth = blockAuth.Clone( false );
                newBlockAuth.CreatedByPersonAlias = null;
                newBlockAuth.CreatedByPersonAliasId = currentPersonAliasId;
                newBlockAuth.CreatedDateTime = RockDateTime.Now;
                newBlockAuth.ModifiedByPersonAlias = null;
                newBlockAuth.ModifiedByPersonAliasId = currentPersonAliasId;
                newBlockAuth.ModifiedDateTime = RockDateTime.Now;
                newBlockAuth.Id = 0;
                newBlockAuth.Guid = Guid.NewGuid();
                newBlockAuth.EntityId = blockIntDictionary[blockGuidDictionary[blockIntDictionary.Where( d => d.Value == blockAuth.EntityId.Value ).FirstOrDefault().Key]];
                authService.Add( newBlockAuth );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// This method takes the attribute values of the original blocks, and creates copies of them that point to the copied blocks. 
        /// In addition, any block attribute value pointing to a page in the original page tree is now updated to point to the
        /// corresponding page in the copied page tree.
        /// </summary>
        /// <param name="pageGuidDictionary">The dictionary containing the original page guids and the corresponding copied page guids.</param>
        /// <param name="blockGuidDictionary">The dictionary containing the original block guids and the corresponding copied block guids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        private void GenerateBlockAttributeValues( Dictionary<Guid, Guid> pageGuidDictionary, Dictionary<Guid, Guid> blockGuidDictionary, RockContext rockContext, int? currentPersonAliasId = null )
        {
            var attributeValueService = new AttributeValueService( rockContext );
            var pageService = new PageService( rockContext );
            var blockService = new BlockService( rockContext );
            var pageGuid = Rock.SystemGuid.EntityType.PAGE.AsGuid();
            var blockGuid = Rock.SystemGuid.EntityType.BLOCK.AsGuid();

            Dictionary<Guid, int> blockIntDictionary = blockService.Queryable()
                .Where( p => blockGuidDictionary.Keys.Contains( p.Guid ) || blockGuidDictionary.Values.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid, p => p.Id );

            var attributeValues = attributeValueService.Queryable().Where( a =>
                a.Attribute.EntityType.Guid == blockGuid && blockIntDictionary.Values.Contains( a.EntityId.Value ) )
                .ToList();

            foreach ( var attributeValue in attributeValues )
            {
                var newAttributeValue = attributeValue.Clone( false );
                newAttributeValue.CreatedByPersonAlias = null;
                newAttributeValue.CreatedByPersonAliasId = currentPersonAliasId;
                newAttributeValue.CreatedDateTime = RockDateTime.Now;
                newAttributeValue.ModifiedByPersonAlias = null;
                newAttributeValue.ModifiedByPersonAliasId = currentPersonAliasId;
                newAttributeValue.ModifiedDateTime = RockDateTime.Now;
                newAttributeValue.Id = 0;
                newAttributeValue.Guid = Guid.NewGuid();
                newAttributeValue.EntityId = blockIntDictionary[blockGuidDictionary[blockIntDictionary.Where( d => d.Value == attributeValue.EntityId.Value ).FirstOrDefault().Key]];

                if ( attributeValue.Attribute.FieldType.Guid == Rock.SystemGuid.FieldType.PAGE_REFERENCE.AsGuid() )
                {
                    if ( pageGuidDictionary.ContainsKey( attributeValue.Value.AsGuid() ) )
                    {
                        newAttributeValue.Value = pageGuidDictionary[attributeValue.Value.AsGuid()].ToString();
                    }
                }

                attributeValueService.Add( newAttributeValue );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Copies any HtmlContent in the original page tree over to the corresponding blocks on the copied page tree.
        /// </summary>
        /// <param name="blockGuidDictionary">The dictionary containing the original block guids and the corresponding copied block guids.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        private void CloneHtmlContent( Dictionary<Guid, Guid> blockGuidDictionary, RockContext rockContext, int? currentPersonAliasId = null )
        {
            var htmlContentService = new HtmlContentService( rockContext );
            var blockService = new BlockService( rockContext );

            Dictionary<Guid, int> blockIntDictionary = blockService.Queryable()
                .Where( p => blockGuidDictionary.Keys.Contains( p.Guid ) || blockGuidDictionary.Values.Contains( p.Guid ) )
                .ToDictionary( p => p.Guid, p => p.Id );

            var htmlContents = htmlContentService.Queryable().Where( a =>
                blockIntDictionary.Values.Contains( a.BlockId ) )
                .ToList();

            foreach ( var htmlContent in htmlContents )
            {
                var newHtmlContent = htmlContent.Clone( false );
                newHtmlContent.CreatedByPersonAlias = null;
                newHtmlContent.CreatedByPersonAliasId = currentPersonAliasId;
                newHtmlContent.CreatedDateTime = RockDateTime.Now;
                newHtmlContent.ModifiedByPersonAlias = null;
                newHtmlContent.ModifiedByPersonAliasId = currentPersonAliasId;
                newHtmlContent.ModifiedDateTime = RockDateTime.Now;
                newHtmlContent.Id = 0;
                newHtmlContent.Guid = Guid.NewGuid();
                newHtmlContent.BlockId = blockIntDictionary[blockGuidDictionary[blockIntDictionary.Where( d => d.Value == htmlContent.BlockId ).FirstOrDefault().Key]];

                htmlContentService.Add( newHtmlContent );
            }

            rockContext.SaveChanges();
        }

        #endregion
    }
}
