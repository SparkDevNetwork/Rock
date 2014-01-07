//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        [Authenticate]
        public IQueryable<TreeViewItem> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, string groupTypeIds )
        {
            var user = CurrentUser();
            if ( user != null )
            {
                var groupService = new GroupService();
                groupService.Repository.SetConfigurationValue( "ProxyCreationEnabled", "false" );
                var qry = groupService.GetNavigationChildren( id, rootGroupId, limitToSecurityRoleGroups, groupTypeIds );

                List<Group> groupList = new List<Group>();
                List<TreeViewItem> groupNameList = new List<TreeViewItem>();

                foreach ( var group in qry )
                {
                    if ( group.IsAuthorized( "View", user.Person ) )
                    {
                        groupList.Add( group );
                        var treeViewItem = new TreeViewItem();
                        treeViewItem.Id = group.Id.ToString();
                        treeViewItem.Name = System.Web.HttpUtility.HtmlEncode( group.Name );

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
            else
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }
    }
}

    
