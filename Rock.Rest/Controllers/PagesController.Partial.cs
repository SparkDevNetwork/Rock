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
using System.Data.Entity;
using System.Linq;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.UI.Controls;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PagesController
    {
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="hidePageIds">List of pages that should not be included in results</param>
        /// <param name="siteType">Type of the site.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Pages/GetChildren/{id}" )]
        public IQueryable<TreeViewItem> GetChildren( int id, string hidePageIds = null, int? siteType = null)
        {
            IQueryable<Page> qry;
            if ( id == 0 )
            {
                qry = Get().Where( a => a.ParentPageId == null );
            }
            else
            {
                qry = Get().Where( a => a.ParentPageId == id );
            }

            if(siteType != null )
            {
                qry = qry.Where( p => ( int ) p.Layout.Site.SiteType == siteType.Value );
            }

            List<int> hidePageIdList = ( hidePageIds ?? string.Empty ).Split( ',' ).Select( s => s.AsInteger()).ToList();
            List<Page> pageList = qry.Where( a => !hidePageIdList.Contains(a.Id) ).OrderBy( a => a.Order ).ThenBy( a => a.InternalName ).ToList();
            List<TreeViewItem> pageItemList = new List<TreeViewItem>();
            foreach ( var page in pageList )
            {
                var pageItem = new TreeViewItem();
                pageItem.Id = page.Id.ToString();
                pageItem.Name = page.InternalName;

                pageItemList.Add( pageItem );
            }

            // try to quickly figure out which items have Children
            List<int> resultIds = pageList.Select( a => a.Id ).ToList();

            var qryHasChildren = Get()
                .Where( p =>
                    p.ParentPageId.HasValue &&
                    resultIds.Contains( p.ParentPageId.Value ) )
                .Select( p => p.ParentPageId.Value )
                .Distinct()
                .ToList();

            foreach ( var g in pageItemList )
            {
                int pageId = int.Parse( g.Id );
                g.HasChildren = qryHasChildren.Any( a => a == pageId );
                g.IconCssClass = "fa fa-file-o";
            }

            return pageItemList.AsQueryable();
        }

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="hidePageIds">The hide page ids.</param>
        /// <returns></returns>
        [RockObsolete("1.11")]
        [Authenticate, Secured]
        public IQueryable<TreeViewItem> GetChildren( int id, string hidePageIds = null )
        {
            return GetChildren( id, hidePageIds, null );
        }
    }
}
