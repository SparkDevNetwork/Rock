//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for the <see cref="Rock.Model.Page"/> model object. This class inherits from the Service class.
    /// </summary>
    public partial class PageService 
    {
        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Page"/> entities by the parent <see cref="Rock.Model.Page">Page's</see> Id.
        /// </summary>
        /// <param name="parentPageId">The Id of the Parent <see cref="Rock.Model.Page"/> to search by. </param>
        /// <returns>An enumerable list of <see cref="Rock.Model.Page"/> entities who's ParentPageId matches the provided value.</returns>
        public IEnumerable<Page> GetByParentPageId( int? parentPageId )
        {
            return Repository.Find( t => ( t.ParentPageId == parentPageId || ( parentPageId == null && t.ParentPageId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Gets an enumerable collection of <see cref="Rock.Model.Page" /> entities associated with a <see cref="Rock.Model.Layout" />.
        /// </summary>
        /// <param name="layoutId">The layout id.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.Page">Pages</see> that use the provided layout.
        /// </returns>
        public IEnumerable<Page> GetByLayoutId( int? layoutId )
        {
            return Repository.Find( t => ( t.LayoutId == layoutId || ( layoutId == null && t.LayoutId == null ) ) ).OrderBy( t => t.Order );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.Page">Pages</see> that are descendants of a <see cref="Rock.Model.Page"/>
        /// </summary>
        /// <param name="parentPageId">A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Page"/></param>
        /// <returns>A collection of <see cref="Rock.Model.Page"/> entities that are descendants of the provided parent <see cref="Rock.Model.Page"/>.</returns>
        public IEnumerable<Page> GetAllDescendents( int parentPageId )
        {
            return Repository.ExecuteQuery(
                @"
                with CTE as (
                select * from [Page] where [ParentPageId]={0}
                union all
                select [a].* from [Page] [a]
                inner join CTE pcte on pcte.Id = [a].[ParentPageId]
                )
                select * from CTE
                ", parentPageId );
        }
    }
}
