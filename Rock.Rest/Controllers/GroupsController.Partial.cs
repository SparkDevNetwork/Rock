//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Http;
using Rock.Model;

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
        public IQueryable<GroupName> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups, string groupTypeIds )
        {
            IQueryable<Group> qry;
            if ( id == 0 )
            {
                qry = Get().Where( a => a.ParentGroupId == null );
                if ( rootGroupId != 0 )
                {
                    qry = qry.Where( a => a.Id == rootGroupId );
                }
            }
            else
            {
                qry = Get().Where( a => a.ParentGroupId == id );
            }

            if ( limitToSecurityRoleGroups )
            {
                qry = qry.Where( a => a.IsSecurityRole );
            }

            if ( !string.IsNullOrWhiteSpace( groupTypeIds ) )
            {
                if ( groupTypeIds != "0" )
                {
                    List<int> groupTypes = groupTypeIds.SplitDelimitedValues().Select( a => int.Parse( a ) ).ToList();

                    qry = qry.Where( a => groupTypes.Contains( a.GroupTypeId ) );
                }
            }

            List<Group> groupList = qry.ToList();
            List<GroupName> groupNameList = new List<GroupName>();

            var appPath = System.Web.VirtualPathUtility.ToAbsolute( "~" );
            string imageUrlFormat = Path.Combine( appPath, "Image.ashx?id={0}&width=15&height=15" );

            GroupTypeService groupTypeService = new GroupTypeService();

            foreach ( var group in groupList )
            {
                group.GroupType = group.GroupType ?? groupTypeService.Get( group.GroupTypeId );
                var groupName = new GroupName();
                groupName.Id = group.Id;
                groupName.Name = System.Web.HttpUtility.HtmlEncode( group.Name );

                // if there a IconCssClass is assigned, use that as the Icon.  Otherwise, use the SmallIcon (if assigned)
                if ( !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass ) )
                {
                    groupName.IconCssClass = group.GroupType.IconCssClass;
                }
                else
                {
                    groupName.IconSmallUrl = group.GroupType.IconSmallFileId != null ? string.Format( imageUrlFormat, group.GroupType.IconSmallFileId ) : string.Empty;
                }

                groupNameList.Add( groupName );
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = groupNameList.Select( a => a.Id ).ToList();

            var qryHasChildren = from x in Get().Select( a => a.ParentGroupId )
                                 where resultIds.Contains( x.Value )
                                 select x.Value;

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var g in groupNameList )
            {
                g.HasChildren = qryHasChildrenList.Any( a => a == g.Id );
            }

            return groupNameList.AsQueryable();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class GroupName
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the group type icon CSS class.
        /// </summary>
        /// <value>
        /// The group type icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the group type icon small id.
        /// </summary>
        /// <value>
        /// The group type icon small id.
        /// </value>
        public string IconSmallUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren { get; set; }
    }
}
