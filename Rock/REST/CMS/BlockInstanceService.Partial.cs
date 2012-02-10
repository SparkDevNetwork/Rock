//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Linq;
using System.ServiceModel.Web;

namespace Rock.REST.CMS
{
    public partial class BlockInstanceService 
    {
        /// <summary>
        /// Moves a block from one zone to another zone.
        /// </summary>
        /// <remarks>
        /// The zone will be inserted as the last block in the new zone relative to the
        /// other zones with the same parent (layout or page).
        /// </remarks>
        /// <param name="id">The id of the block instance.</param>
        /// <param name="BlockInstance">The block instance.</param>
        /// <returns></returns>
        [WebInvoke( Method = "PUT", UriTemplate = "Move/{id}" )]
        public void Move( string id, Rock.CMS.DTO.BlockInstance BlockInstance )
        {
            var currentUser = Rock.CMS.UserService.GetCurrentUser();
            if ( currentUser == null )
                throw new WebFaultException<string>( "Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
                uow.objectContext.Configuration.ProxyCreationEnabled = false;
                Rock.CMS.BlockInstanceService BlockInstanceService = new Rock.CMS.BlockInstanceService();
                Rock.CMS.BlockInstance existingBlockInstance = BlockInstanceService.Get( int.Parse( id ) );

                if ( existingBlockInstance.Authorized( "Edit", currentUser ) )
                {
                    // If the block was moved from or to the layout section, then all the pages
                    // that use that layout need to be flushed from cache
                    if ( existingBlockInstance.Layout != BlockInstance.Layout )
                    {
                        if ( existingBlockInstance.Layout != null )
                            Rock.Web.Cache.Page.FlushLayout( existingBlockInstance.Layout );
                        if ( BlockInstance.Layout != null )
                            Rock.Web.Cache.Page.FlushLayout( BlockInstance.Layout );
                    }

                    uow.objectContext.Entry( existingBlockInstance ).CurrentValues.SetValues( BlockInstance );
                    BlockInstanceService.Move( existingBlockInstance );
                    BlockInstanceService.Save( existingBlockInstance, currentUser.PersonId );
                }
                else
                    throw new WebFaultException<string>( "Not Authorized to Edit this BlockInstance", System.Net.HttpStatusCode.Forbidden );
            }
        }

        /// <summary>
        /// Moves a block from one zone to another zone.
        /// </summary>
        /// <param name="id">The id of the block instance.</param>
        /// <param name="apiKey">The API key.</param>
        /// <param name="BlockInstance">The block instance.</param>
        /// <remarks>
        /// The zone will be inserted as the last block in the new zone relative to the
        /// other zones with the same parent (layout or page).
        /// </remarks>
        [WebInvoke( Method = "PUT", UriTemplate = "Move/{id}/{apiKey}" )]
        public void ApiMove( string id, string apiKey, Rock.CMS.DTO.BlockInstance BlockInstance )
        {
            using ( Rock.Data.UnitOfWorkScope uow = new Rock.Data.UnitOfWorkScope() )
            {
				Rock.CMS.UserService userService = new Rock.CMS.UserService();
                Rock.CMS.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

                if ( user != null )
                {
                    uow.objectContext.Configuration.ProxyCreationEnabled = false;
                    Rock.CMS.BlockInstanceService BlockInstanceService = new Rock.CMS.BlockInstanceService();
                    Rock.CMS.BlockInstance existingBlockInstance = BlockInstanceService.Get( int.Parse( id ) );

                    if ( existingBlockInstance.Authorized( "Edit", user ) )
                    {
                        // If the block was moved from or to the layout section, then all the pages
                        // that use that layout need to be flushed from cache
                        if ( existingBlockInstance.Layout != BlockInstance.Layout )
                        {
                            if ( existingBlockInstance.Layout != null )
                                Rock.Web.Cache.Page.FlushLayout( existingBlockInstance.Layout );
                            if ( BlockInstance.Layout != null )
                                Rock.Web.Cache.Page.FlushLayout( BlockInstance.Layout );
                        }

                        uow.objectContext.Entry( existingBlockInstance ).CurrentValues.SetValues( BlockInstance );
                        BlockInstanceService.Move( existingBlockInstance );
                        BlockInstanceService.Save( existingBlockInstance, user.PersonId );
                    }
                    else
                        throw new WebFaultException<string>( "Not Authorized to Edit this BlockInstance", System.Net.HttpStatusCode.Forbidden );
                }
                else
                    throw new WebFaultException<string>( "Invalid API Key", System.Net.HttpStatusCode.Forbidden );
            }
        }
    }
}
