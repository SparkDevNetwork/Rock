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

using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Tests.Shared.TestFramework
{
    /// <summary>
    /// Helper methods for hosting a <see cref="RockBlockType"/> against a mocked
    /// <see cref="RockContext"/>, so that a block's initialization and block
    /// actions can be exercised directly from a unit test.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This mirrors what the real hosts (RockPage and BlockActionsController) do
    /// before calling into a block: it assigns the <see cref="RockBlockType.BlockCache"/>,
    /// <see cref="RockBlockType.PageCache"/>, <see cref="RockBlockType.RequestContext"/>
    /// and <see cref="RockBlockType.RockContext"/> properties.
    /// </para>
    /// <para>
    /// The site, layout and page rows are seeded into the mocked context so the
    /// cache objects resolve normally. The page gets a parent page by default so
    /// that <c>GetParentPageUrl()</c> returns a real URL.
    /// </para>
    /// </remarks>
    public static class MockBlockHelper
    {
        #region Methods

        /// <summary>
        /// Seeds a site, layout and page (optionally with a parent page) into the
        /// mocked context and returns the page that a block should be placed on.
        /// </summary>
        /// <param name="rockContext">The mocked context to seed.</param>
        /// <param name="hasParentPage"><c>true</c> to also seed a parent page for the returned page.</param>
        /// <returns>The <see cref="Page"/> that blocks should be placed on.</returns>
        public static Page SeedPage( RockContext rockContext, bool hasParentPage = true )
        {
            var site = new Site
            {
                Id = GetNextId<Site>( rockContext ),
                Guid = Guid.NewGuid(),
                Name = "Test Site",
                IsActive = true,
                Theme = "Rock",

                // A domain keeps SiteCache building from falling back to the empty
                // PublicApplicationRoot global attribute, which would throw.
                SiteDomains = new List<SiteDomain>
                {
                    new SiteDomain { Domain = "rock.example", Order = 0 }
                }
            };
            rockContext.Set<Site>().Add( site );

            var layout = new Layout
            {
                Id = GetNextId<Layout>( rockContext ),
                Guid = Guid.NewGuid(),
                SiteId = site.Id,
                Site = site,
                Name = "Full Width",
                FileName = "FullWidth"
            };
            rockContext.Set<Layout>().Add( layout );

            Page parentPage = null;

            if ( hasParentPage )
            {
                parentPage = new Page
                {
                    Id = GetNextId<Page>( rockContext ),
                    Guid = Guid.NewGuid(),
                    InternalName = "Parent Page",
                    LayoutId = layout.Id,
                    Layout = layout
                };
                rockContext.Set<Page>().Add( parentPage );
            }

            var page = new Page
            {
                Id = GetNextId<Page>( rockContext ),
                Guid = Guid.NewGuid(),
                InternalName = "Block Page",
                LayoutId = layout.Id,
                Layout = layout,
                ParentPageId = parentPage?.Id,
                ParentPage = parentPage
            };
            rockContext.Set<Page>().Add( page );

            return page;
        }

        /// <summary>
        /// Creates an instance of the block type <typeparamref name="TBlock"/>
        /// placed on <paramref name="page"/> and wired up to the mocked context
        /// the same way the real hosts wire up a block before calling it.
        /// </summary>
        /// <typeparam name="TBlock">The block type to instantiate.</typeparam>
        /// <param name="rockContext">The mocked context the block will use for data access.</param>
        /// <param name="page">The page the block is on, typically from <see cref="SeedPage(RockContext, bool)"/>.</param>
        /// <param name="pageParameters">The page parameters (query string and route values) for the request.</param>
        /// <param name="currentPerson">The person making the request, or <c>null</c> for an anonymous request.</param>
        /// <returns>The block instance, ready to have its initialization or actions called.</returns>
        public static TBlock CreateBlock<TBlock>( RockContext rockContext, Page page, IDictionary<string, string> pageParameters = null, Person currentPerson = null )
            where TBlock : RockBlockType, new()
        {
            var blockEntityType = EntityTypeCache.Get( typeof( TBlock ), true, rockContext );

            var blockType = new BlockType
            {
                Id = GetNextId<BlockType>( rockContext ),
                Guid = Guid.NewGuid(),
                Name = typeof( TBlock ).Name,
                Path = string.Empty,
                EntityTypeId = blockEntityType.Id
            };
            rockContext.Set<BlockType>().Add( blockType );

            var block = new Block
            {
                Id = GetNextId<Block>( rockContext ),
                Guid = Guid.NewGuid(),
                Name = typeof( TBlock ).Name,
                BlockTypeId = blockType.Id,
                BlockType = blockType,
                PageId = page.Id,
                Zone = "Main"
            };
            rockContext.Set<Block>().Add( block );

            var requestContext = new RockRequestContext();

            if ( pageParameters != null )
            {
                requestContext.SetPageParameters( pageParameters );
            }

            if ( currentPerson != null )
            {
                requestContext.CurrentUser = new UserLogin
                {
                    Guid = Guid.NewGuid(),
                    UserName = $"{currentPerson.FirstName}.{currentPerson.LastName}",
                    PersonId = currentPerson.Id,
                    Person = currentPerson
                };
            }

            return new TBlock
            {
                BlockCache = BlockCache.Get( block.Id, rockContext ),
                PageCache = PageCache.Get( page.Id, rockContext ),
                RequestContext = requestContext,
                RockContext = rockContext
            };
        }

        /// <summary>
        /// Gets the next available integer identifier for the entity type in the
        /// mocked context.
        /// </summary>
        /// <typeparam name="T">The entity type being seeded.</typeparam>
        /// <param name="rockContext">The mocked context.</param>
        /// <returns>The next Id to assign.</returns>
        private static int GetNextId<T>( RockContext rockContext ) where T : class, IEntity
        {
            var set = rockContext.Set<T>();

            return set.Any() ? set.Max( e => e.Id ) + 1 : 1;
        }

        #endregion Methods
    }
}
