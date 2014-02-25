// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GroupsController : IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "GroupsGetChildren",
                routeTemplate: "api/Groups/GetChildren/{id}/{rootGroupId}/{limitToSecurityRoleGroups}/{groupTypeIds}",
                defaults: new
                {
                    controller = "Groups",
                    action = "GetChildren"
                } );
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="rootGroupId">The root group id.</param>
        /// <param name="limitToSecurityRoleGroups">if set to <c>true</c> [limit to security role groups].</param>
        /// <param name="groupTypeIds">The group type ids.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public IQueryable<TreeViewItem> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, string groupTypeIds )
        {
            var groupService = new GroupService();
            groupService.Repository.SetConfigurationValue( "ProxyCreationEnabled", "false" );
            var qry = groupService.GetNavigationChildren( id, rootGroupId, limitToSecurityRoleGroups, groupTypeIds );

            List<Group> groupList = new List<Group>();
            List<TreeViewItem> groupNameList = new List<TreeViewItem>();

            var person = GetPerson();

            foreach ( var group in qry )
            {
                if ( group.IsAuthorized( "View", person ) )
                {
                    groupList.Add( group );
                    var treeViewItem = new TreeViewItem();
                    treeViewItem.Id = group.Id.ToString();
                    treeViewItem.Name = group.Name;

                    // if there a IconCssClass is assigned, use that as the Icon.
                    var groupType = Rock.Web.Cache.GroupTypeCache.Read( group.GroupTypeId );
                    if ( groupType != null )
                    {
                        treeViewItem.IconCssClass = groupType.IconCssClass;
                    }

                    groupNameList.Add( treeViewItem );
                }
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = groupList.Select( a => a.Id ).ToList();

            var qryHasChildren = from x in Get().Select( a => a.ParentGroupId )
                                    where resultIds.Contains( x.Value )
                                    select x.Value;

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var g in groupNameList )
            {
                int groupId = int.Parse( g.Id );
                g.HasChildren = qryHasChildrenList.Any( a => a == groupId );
            }

            return groupNameList.AsQueryable();
        }
    }
}

    
