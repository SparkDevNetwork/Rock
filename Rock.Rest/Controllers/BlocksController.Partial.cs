//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    public partial class BlocksController : IHasCustomRoutes 
    {
        /// <summary>
        /// Add Custom route needed for block move
        /// </summary>
        /// <param name="routes"></param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "BlockMove",
                routeTemplate: "api/blocks/move/{id}",
                defaults: new
                {
                    controller = "blocks",
                    action = "move"
                } );
        }

        /// <summary>
        /// Moves a block from one zone to another
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        [HttpPut]
        [Authenticate]
        public void Move( int id, BlockDto block )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                var service = new BlockService();
                block.Id = id;
                Block model;
                if ( !service.TryGet( id, out model ) )
                    throw new HttpResponseException( HttpStatusCode.NotFound );

                if ( !model.IsAuthorized( "Edit", user.Person ) )
                    throw new HttpResponseException( HttpStatusCode.Unauthorized );

                if ( model.IsValid )
                {
                    if ( model.Layout != null && model.Layout != block.Layout )
                        Rock.Web.Cache.PageCache.FlushLayoutBlocks( model.Layout );

                    if (block.Layout != null)
                        Rock.Web.Cache.PageCache.FlushLayoutBlocks( block.Layout);
                    else
                    {
                        var page = Rock.Web.Cache.PageCache.Read( block.PageId.Value );
                        page.FlushBlocks();
                    }

                    block.CopyToModel( model );

                    service.Move( model );
                    service.Save( model, user.PersonId );
                }
                else
                    throw new HttpResponseException( HttpStatusCode.BadRequest );
            }
            else
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
        }
    }
}
