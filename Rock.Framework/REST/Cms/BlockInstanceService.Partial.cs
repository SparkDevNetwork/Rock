using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

using Rock.Models.Cms;

namespace Rock.REST.Cms
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
        public void Move( string id, Rock.DataTransferObjects.Cms.BlockInstance BlockInstance )
        {
            var currentUser = System.Web.Security.Membership.GetUser();
            if ( currentUser == null )
                throw new WebFaultException<string>( "Must be logged in", System.Net.HttpStatusCode.Forbidden );

            using ( Rock.Helpers.UnitOfWorkScope uow = new Rock.Helpers.UnitOfWorkScope() )
            {
                uow.objectContext.Configuration.ProxyCreationEnabled = false;
                Rock.Services.Cms.BlockInstanceService BlockInstanceService = new Rock.Services.Cms.BlockInstanceService();
                Rock.Models.Cms.BlockInstance existingBlockInstance = BlockInstanceService.Get( int.Parse( id ) );

                if ( existingBlockInstance.Authorized( "Edit", currentUser ) )
                {
                    // If the block was moved from or to the layout section, then all the pages
                    // that use that layout need to be flushed from cache
                    if ( existingBlockInstance.Layout != BlockInstance.Layout )
                    {
                        if ( existingBlockInstance.Layout != null )
                            Rock.Cms.Cached.Page.FlushLayout( existingBlockInstance.Layout );
                        if ( BlockInstance.Layout != null )
                            Rock.Cms.Cached.Page.FlushLayout( BlockInstance.Layout );
                    }

                    uow.objectContext.Entry( existingBlockInstance ).CurrentValues.SetValues( BlockInstance );
                    BlockInstanceService.Move( existingBlockInstance );
                    BlockInstanceService.Save( existingBlockInstance, currentUser.PersonId() );
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
        public void ApiMove( string id, string apiKey, Rock.DataTransferObjects.Cms.BlockInstance BlockInstance )
        {
            using ( Rock.Helpers.UnitOfWorkScope uow = new Rock.Helpers.UnitOfWorkScope() )
            {
				Rock.Services.Cms.UserService userService = new Rock.Services.Cms.UserService();
                Rock.Models.Cms.User user = userService.Queryable().Where( u => u.ApiKey == apiKey ).FirstOrDefault();

                if ( user != null )
                {
                    uow.objectContext.Configuration.ProxyCreationEnabled = false;
                    Rock.Services.Cms.BlockInstanceService BlockInstanceService = new Rock.Services.Cms.BlockInstanceService();
                    Rock.Models.Cms.BlockInstance existingBlockInstance = BlockInstanceService.Get( int.Parse( id ) );

                    if ( existingBlockInstance.Authorized( "Edit", user.Username ) )
                    {
                        // If the block was moved from or to the layout section, then all the pages
                        // that use that layout need to be flushed from cache
                        if ( existingBlockInstance.Layout != BlockInstance.Layout )
                        {
                            if ( existingBlockInstance.Layout != null )
                                Rock.Cms.Cached.Page.FlushLayout( existingBlockInstance.Layout );
                            if ( BlockInstance.Layout != null )
                                Rock.Cms.Cached.Page.FlushLayout( BlockInstance.Layout );
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
