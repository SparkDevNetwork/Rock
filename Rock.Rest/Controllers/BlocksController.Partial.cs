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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Blocks REST API
    /// </summary>
    public partial class BlocksController
    {
        #region Fields

        /// <summary>
        /// The service provider for this instance.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlocksController"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for this instance.</param>
        [ActivatorUtilitiesConstructor]
        public BlocksController( IServiceProvider serviceProvider )
            : base( new BlockService( new RockContext() ) )
        {
            _serviceProvider = serviceProvider;
        }

        #endregion

        /// <summary>
        /// Deletes the specified Block with extra logic to flush caches, after checking
        /// that the person can administrate the block.
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

            /*
                10/16/2026 - MSE

                The base DELETE only requires EDIT on the block. Staff can hold EDIT on blocks
                that show on every page of a layout or site, such as the person profile blocks,
                and the Rock UI only shows the delete icon to people who can administrate the
                block. Require ADMINISTRATE on the stored block before deleting it. This keeps
                the base signature so the ApiId and any custom REST security do not change.

                Reason: Deleting a block is an administrator setting.
            */
            var storedBlock = BlockCache.Get( id );
            if ( storedBlock == null || !storedBlock.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, GetPerson() ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            base.Delete( id );
        }

        /// <summary>
        /// Adds a new Block after checking that the person can administrate the page, layout or site it is placed on.
        /// </summary>
        /// <param name="value">The block.</param>
        /// <returns>The response with the new block identifier.</returns>
        /// <exception cref="HttpResponseException"></exception>
        [Authenticate, Secured]
        public override HttpResponseMessage Post( [FromBody] Block value )
        {
            if ( value == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            /*
                10/16/2026 - MSE

                The base POST only requires EDIT on the new block, which it inherits from the
                page, layout or site it is placed on. A new block's Pre and Post HTML are rendered
                as Lava with all commands enabled, and the Rock UI only lets people who can
                administrate a page add blocks to it. This override keeps the base signature so
                the ApiId and any custom REST security on this action do not change.

                Reason: Adding a block is an administrator setting.
            */
            CheckCanPlaceBlock( value.PageId, value.LayoutId, value.SiteId, GetPerson() );

            return base.Post( value );
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
        [Rock.SystemGuid.RestActionGuid( "74A94F70-73F0-41FD-AB4A-6104C971CEC2" )]
        public void Move( int id, Block block )
        {
            if ( block == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            var person = GetPerson();

            SetProxyCreation( true );

            block.Id = id;
            Block model;
            if ( !Service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( model, person );

            /*
                10/16/2026 - MSE

                Moving a block always places it under a zone, page, layout or site. The Rock UI
                only shows the move icon to people who can administrate the block and its page,
                so EDIT on the block is not enough here. The person must also be able to
                administrate the page, layout or site the block is moved to.

                Reason: Moving a block is an administrator setting.
            */
            if ( !model.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, person ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            CheckCanPlaceBlock( block.PageId, block.LayoutId, block.SiteId, person );

            model.Zone = block.Zone;
            model.PageId = block.PageId;
            model.LayoutId = block.LayoutId;
            model.SiteId = block.SiteId;

            if ( model.IsValid )
            {
                model.Order = ( ( BlockService ) Service ).GetMaxOrder( model );
                System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", GetPerson() );
                Service.Context.SaveChanges();
            }
            else
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
        }

        /// <inheritdoc/>
        protected override void CheckCanEditAfterUpdate( Block entity )
        {
            base.CheckCanEditAfterUpdate( entity );

            /*
                10/16/2026 - MSE

                A block's Pre and Post HTML are rendered as Lava with all commands enabled, and
                its zone, page, layout and site decide where it shows. The Rock UI only lets
                people who can administrate the block change these, but v1 PUT and PATCH only
                required EDIT. Require ADMINISTRATE on the stored block for every update, and
                ADMINISTRATE on the new page, layout or site when the block is moved.

                Reason: Block settings are administrator settings.
            */
            var person = GetPerson();
            var storedBlock = BlockCache.Get( entity.Id );
            if ( storedBlock == null || !storedBlock.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, person ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }

            var entry = Service.Context.Entry( entity );
            if ( entry.Property( b => b.PageId ).OriginalValue != entity.PageId
                || entry.Property( b => b.LayoutId ).OriginalValue != entity.LayoutId
                || entry.Property( b => b.SiteId ).OriginalValue != entity.SiteId )
            {
                CheckCanPlaceBlock( entity.PageId, entity.LayoutId, entity.SiteId, person );
            }
        }

        /// <summary>
        /// Checks that the person can place a block on the specified page, layout or site.
        /// </summary>
        /// <param name="pageId">The page identifier the block will be placed on.</param>
        /// <param name="layoutId">The layout identifier the block will be placed on.</param>
        /// <param name="siteId">The site identifier the block will be placed on.</param>
        /// <param name="person">The person placing the block.</param>
        /// <exception cref="HttpResponseException"></exception>
        private void CheckCanPlaceBlock( int? pageId, int? layoutId, int? siteId, Person person )
        {
            /*
                10/16/2026 - MSE

                A block without a page, layout or site, or with more than one, has no single
                parent to authorize against and would fall back to the global default. A block
                may only be placed where the person can administrate, which matches the page
                ADMINISTRATE the Rock UI requires to add or move blocks.

                Reason: Authorize the destination of a block like the Rock UI does.
            */
            if ( !HasSingleBlockParent( pageId, layoutId, siteId ) )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            ISecured destination;
            if ( pageId.HasValue )
            {
                destination = PageCache.Get( pageId.Value );
            }
            else if ( layoutId.HasValue )
            {
                destination = LayoutCache.Get( layoutId.Value );
            }
            else
            {
                destination = SiteCache.Get( siteId.Value );
            }

            if ( destination == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            if ( !destination.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, person ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }

        /// <summary>
        /// Determines whether exactly one of the page, layout or site identifiers has a value.
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <param name="layoutId">The layout identifier.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns><c>true</c> if exactly one identifier has a value; otherwise <c>false</c>.</returns>
        internal static bool HasSingleBlockParent( int? pageId, int? layoutId, int? siteId )
        {
            return ( pageId.HasValue ? 1 : 0 ) + ( layoutId.HasValue ? 1 : 0 ) + ( siteId.HasValue ? 1 : 0 ) == 1;
        }

        /// <summary>
        /// Executes an action handler on a specific block.
        /// </summary>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/blocks/action/{blockGuid}/{actionName}" )]
        [RockObsolete( "1.13" )]
        [Obsolete( "Does not provide access to site-level or layout-level blocks. Use api/blocks/actions/{pageGuid}/{blockGuid}/{actionName} instead." )]
        [Rock.SystemGuid.RestActionGuid( "227011DC-2242-4DBA-A931-526DC52951EA" )]
        public async Task<IHttpActionResult> BlockActionAsPost( string blockGuid, string actionName, [NakedBody] string parameters )
        {
            if ( parameters == string.Empty )
            {
                return await v2.BlockActionsController.ProcessAction( this, null, blockGuid.AsGuidOrNull(), actionName, null, _serviceProvider );
            }

            //
            // We have to manually parse the JSON data, otherwise any strings
            // that look like dates get converted to Date objects. This causes
            // problems because then when we later stuff that Date object into
            // an actual string, the format has been changed. This happens, for
            // example, with Attribute Values.
            //
            using ( var stringReader = new StringReader( parameters ) )
            {
                using ( var jsonReader = new JsonTextReader( stringReader ) { DateParseHandling = DateParseHandling.None } )
                {
                    var parameterToken = JToken.ReadFrom( jsonReader );

                    return await v2.BlockActionsController.ProcessAction( this, null, blockGuid.AsGuidOrNull(), actionName, parameterToken, _serviceProvider );
                }
            }
        }

        /// <summary>
        /// Executes an action handler on a specific block.
        /// </summary>
        /// <param name="pageGuid">The page unique identifier.</param>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [Authenticate]
        [System.Web.Http.Route( "api/blocks/action/{pageGuid:guid}/{blockGuid:guid}/{actionName}" )]
        [Rock.SystemGuid.RestActionGuid( "DCD0BB91-F857-4627-A420-8CAA1ACF99D3" )]
        public async Task<IHttpActionResult> BlockAction( string pageGuid, string blockGuid, string actionName, [NakedBody] string parameters )
        {
            if ( parameters == string.Empty )
            {
                return await v2.BlockActionsController.ProcessAction( this, pageGuid.AsGuidOrNull(), blockGuid.AsGuidOrNull(), actionName, null, _serviceProvider );
            }

            //
            // We have to manually parse the JSON data, otherwise any strings
            // that look like dates get converted to Date objects. This causes
            // problems because then when we later stuff that Date object into
            // an actual string, the format has been changed. This happens, for
            // example, with Attribute Values.
            //
            using ( var stringReader = new StringReader( parameters ) )
            {
                using ( var jsonReader = new JsonTextReader( stringReader ) { DateParseHandling = DateParseHandling.None } )
                {
                    var parameterToken = JToken.ReadFrom( jsonReader );

                    return await v2.BlockActionsController.ProcessAction( this, pageGuid.AsGuidOrNull(), blockGuid.AsGuidOrNull(), actionName, parameterToken, _serviceProvider );
                }
            }
        }
    }
}