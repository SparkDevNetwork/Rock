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

using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using Rock.Data;
using Rock.Tasks;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Page
    {
        /// <summary>
        /// Save hook implementation for <see cref="Page"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Page>
        {
            /// <summary>
            /// Method that will be called on an entity immediately before the item is saved by context.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Modified || State == EntityContextState.Deleted )
                {
                    Entity._originalParentPageId = Entry.OriginalValues[nameof( Page.ParentPageId )]?.ToString().AsIntegerOrNull();
                }

                if ( State == EntityContextState.Added )
                {
                    if ( Entity.LayoutId != 0 )
                    {
                        Entity.SiteId = LayoutCache.Get( Entity.LayoutId, RockContext )?.SiteId ?? 0;
                    }
                }
                else if ( State == EntityContextState.Deleted )
                {
                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add( "PageId", Entity.Id );

                    // since routes have a cascade delete relationship (their presave won't get called), delete routes from route table.
                    var routes = RouteTable.Routes;
                    if ( routes != null )
                    {
                        var routesToRemove = new List<Route>();

                        foreach ( var existingRoute in RouteTable.Routes.OfType<Route>().Where( r => r.PageIds().Contains( Entity.Id ) ) )
                        {
                            var pageAndRouteIds = existingRoute.DataTokens["PageRoutes"] as List<Rock.Web.PageAndRouteId>;
                            pageAndRouteIds = pageAndRouteIds.Where( p => p.PageId != Entity.Id ).ToList();
                            if ( pageAndRouteIds.Any() )
                            {
                                existingRoute.DataTokens["PageRoutes"] = pageAndRouteIds;
                            }
                            else
                            {
                                routesToRemove.Add( existingRoute );
                            }
                        }

                        foreach ( var existingRoute in routesToRemove )
                        {
                            RouteTable.Routes.Remove( existingRoute );
                        }
                    }
                }
                else if ( State == EntityContextState.Modified )
                {
                    var previousInternalName = Entry.OriginalValues[nameof( Page.InternalName )].ToStringSafe();
                    _didNameChange = previousInternalName != Entity.InternalName;

                    var previousLayoutId = ( int ) Entry.OriginalValues[nameof( Page.LayoutId )];
                    if ( previousLayoutId != Entity.LayoutId && Entity.LayoutId != 0 )
                    {
                        Entity.SiteId = LayoutCache.Get( Entity.LayoutId, RockContext )?.SiteId ?? 0;
                    }
                }

                base.PreSave();
            }

            private bool _didNameChange = false;

            /// <summary>
            /// Called after the save operation has been executed.
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                base.PostSave();

                if ( _didNameChange )
                {
                    new AddPageRenameInteraction.Message()
                    {
                        PageGuid = Entity.Guid
                    }.SendWhen( this.DbContext.WrappedTransactionCompletedTask );
                }
            }
        }
    }
}
