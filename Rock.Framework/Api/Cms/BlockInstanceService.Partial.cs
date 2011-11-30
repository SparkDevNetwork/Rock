using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;

using Rock.Models.Cms;
using Rock.Cms.Security;

namespace Rock.Api.Cms
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
        public void Move( string id, BlockInstanceDTO BlockInstance )
        {
            var currentUser = System.Web.Security.Membership.GetUser();
            if ( currentUser == null )
                throw new FaultException( "Must be logged in" );

            using ( Rock.Helpers.UnitOfWorkScope uow = new Rock.Helpers.UnitOfWorkScope() )
            {
                uow.objectContext.Configuration.ProxyCreationEnabled = false;

                Rock.Services.Cms.BlockInstanceService BlockInstanceService = new Rock.Services.Cms.BlockInstanceService();
                Rock.Models.Cms.BlockInstance existingBlockInstance = BlockInstanceService.Get( int.Parse( id ) );
                if ( existingBlockInstance.Authorized( "Edit", currentUser ) )
                {
                    int? order = BlockInstanceService.Queryable().
                        Where( b => b.Layout == BlockInstance.Layout &&
                            b.PageId == BlockInstance.PageId &&
                            b.Zone == BlockInstance.Zone ).
                        Select( b => ( int? )b.Order ).Max();

                    BlockInstance.Order = order.HasValue ? order.Value + 1 : 0;

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
                    BlockInstanceService.Save( existingBlockInstance, currentUser.PersonId() );
                }
                else
                    throw new FaultException( "Unauthorized" );
            }
        }
    }
}
