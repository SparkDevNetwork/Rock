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
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

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
            var person = GetPerson();

            SetProxyCreation( true );

            block.Id = id;
            Block model;
            if ( !Service.TryGet( id, out model ) )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            CheckCanEdit( model, person );

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

        /// <summary>
        /// Executes an action handler on a specific block.
        /// </summary>
        /// <param name="blockGuid">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        [System.Web.Http.Route( "api/blocks/action/{blockGuid}/{actionName}" )]
        [RockObsolete( "1.13" )]
        [Obsolete( "Does not provide access to site-level or layout-level blocks. Use api/blocks/actions/{pageGuid}/{blockGuid}/{actionName} instead.")]
        [Rock.SystemGuid.RestActionGuid( "E025B9B5-060A-4853-AC78-0D5B850771F8" )]
        public async Task<IHttpActionResult> BlockAction( Guid blockGuid, string actionName )
        {
            return await v2.BlockActionsController.ProcessAction( this, null, blockGuid, actionName, null, _serviceProvider );
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
        /// <returns></returns>
        [Authenticate]
        [HttpGet]
        [System.Web.Http.Route( "api/blocks/action/{pageGuid:guid}/{blockGuid:guid}/{actionName}" )]
        [Rock.SystemGuid.RestActionGuid( "31EA4036-31E1-4A0B-9354-44BEC3C228EB" )]
        public async Task<IHttpActionResult> BlockAction( string pageGuid, string blockGuid, string actionName )
        {
            return await v2.BlockActionsController.ProcessAction( this, pageGuid.AsGuidOrNull(), blockGuid.AsGuidOrNull(), actionName, null, _serviceProvider );
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

        /// <summary>
        /// Executes an action handler on a specific block.
        /// </summary>
        /// <param name="pageIdentifier">The page unique identifier.</param>
        /// <param name="blockIdentifier">The block unique identifier.</param>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        [RockObsolete( "1.11" )]
        [Obsolete( "Does not provide access to site-level or layout-level blocks. Use api/blocks/actions/{pageGuid}/{blockGuid}/{actionName} instead." )]
        [Authenticate]
        public async Task<IHttpActionResult> BlockAction( string pageIdentifier, string blockIdentifier, string actionName, [FromBody] JToken parameters )
        {
            return await v2.BlockActionsController.ProcessAction( this, pageIdentifier.AsGuidOrNull(), blockIdentifier.AsGuidOrNull(), actionName, parameters, _serviceProvider );
        }
    }
}