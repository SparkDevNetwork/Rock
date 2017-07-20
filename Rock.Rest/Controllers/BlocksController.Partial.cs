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
using System.Net;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Blocks REST API
    /// </summary>
    public partial class BlocksController
    {
        /// <summary>
        /// Deletes the specified Block with extra logic to flush caches.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [Authenticate, Secured]
        public override void Delete( int id )
        {
            SetProxyCreation( true );

            // get the ids of the page and layout so we can flush stuff after the base.Delete
            int? pageId = null;
            int? layoutId = null;
            int? siteId = null;

            var block = this.Get( id );
            if ( block != null )
            {
                pageId = block.PageId;
                layoutId = block.LayoutId;
                siteId = block.SiteId;
            }

            base.Delete( id );

            // flush all the cache stuff that involves the block
            Rock.Web.Cache.BlockCache.Flush( id );

            if ( siteId.HasValue )
            {
                Rock.Web.Cache.PageCache.FlushSiteBlocks( siteId.Value );
            }

            if ( layoutId.HasValue )
            {
                Rock.Web.Cache.PageCache.FlushLayoutBlocks( layoutId.Value );
            }

            if ( pageId.HasValue )
            {
                Rock.Web.Cache.PageCache.Flush( pageId.Value );
                var page = Rock.Web.Cache.PageCache.Read( pageId.Value );
                page.FlushBlocks();
            }
        }

        /// <summary>
        /// Moves a block from one zone to another
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="block">The block.</param>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/blocks/move/{id}" )]
        public void Move( int id, Block block )
        {
            var person = GetPerson();

            SetProxyCreation( true );

            block.Id = id;
            Block model;
            if ( !Service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( model, person );

            Rock.Web.Cache.BlockCache.Flush( id );

            if ( model.SiteId.HasValue && model.SiteId != block.SiteId )
            {
                Rock.Web.Cache.PageCache.FlushSiteBlocks( model.SiteId.Value );
            }
            else if ( model.LayoutId.HasValue && model.LayoutId != block.LayoutId )
            {
                Rock.Web.Cache.PageCache.FlushLayoutBlocks( model.LayoutId.Value );
            }

            if ( block.SiteId.HasValue )
            {
                Rock.Web.Cache.PageCache.FlushSiteBlocks( block.SiteId.Value );
            }
            else if ( block.LayoutId.HasValue )
            {
                Rock.Web.Cache.PageCache.FlushLayoutBlocks( block.LayoutId.Value );
            }
            else
            {
                var page = Rock.Web.Cache.PageCache.Read( block.PageId.Value );
                page.FlushBlocks();
            }

            model.Zone = block.Zone;
            model.PageId = block.PageId;
            model.LayoutId = block.LayoutId;
            model.SiteId = block.SiteId;

            if ( model.IsValid )
            {
                model.Order = ( (BlockService)Service ).GetMaxOrder( model );
                System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
                Service.Context.SaveChanges();
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
        }
    }
}
