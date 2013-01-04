//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
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
                routeTemplate: "api/Groups/GetChildren/{id}/{rootGroupId}/{limitToSecurityRoleGroups}",
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
        /// <returns></returns>
        public IQueryable<GroupName> GetChildren( int id, int rootGroupId, bool limitToSecurityRoleGroups )
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

            var groupNameQry = from a in qry
                select new GroupName { Id = a.Id, Name = a.Name };

            List<GroupName> result = groupNameQry.ToList();
            
            // try to quickly figure out which items have Children
            List<int> resultIds = result.Select( a => a.Id ).ToList();

            var qryHasChildren = from x in Get().Select( a => a.ParentGroupId )
                                 where resultIds.Contains( x.Value )
                                 select x.Value;

            var qryHasChildrenList = qryHasChildren.ToList();

            foreach ( var g in result )
            {
                g.HasChildren = qryHasChildrenList.Any( a => a == g.Id );
            }

            return result.AsQueryable();
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
        /// Gets or sets a value indicating whether this instance has children.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has children; otherwise, <c>false</c>.
        /// </value>
        public bool HasChildren { get; set; }
    }
}
