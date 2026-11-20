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
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;
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
        /// <param name="rootPageId">The root group identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/Pages/GetChildren/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "8E291629-C998-49EA-AB9A-59E810688171" )]
        public IQueryable<TreeViewItem> GetChildren( int id,
            string hidePageIds = null,
            int? siteType = null,
            int rootPageId = 0 )
        {
            var person = GetPerson();
            IQueryable<Page> qry;
            if ( id == 0 )
            {
                if ( rootPageId != 0 )
                {
                    qry = Get().Where( a => a.ParentPageId == rootPageId );
                }
                else
                {
                    qry = Get().Where( a => a.ParentPageId == null );
                }
            }
            else
            {
                qry = Get().Where( a => a.ParentPageId == id );
            }

            if ( siteType != null )
            {
                qry = qry.Where( p => ( int ) p.Layout.Site.SiteType == siteType.Value );
            }

            List<int> hidePageIdList = ( hidePageIds ?? string.Empty ).Split( ',' ).Select( s => s.AsInteger() ).ToList();
            /*
                11/20/2026 - MSE

                The REST action grant only says the person may use this endpoint. Each page has
                its own VIEW rule, inherited from its parent pages and site, so only list the pages
                (and count the children) the person can view. PageCache is used because it walks
                that inheritance; the Page entities here are loaded without their parents.

                Reason: Anyone with the REST action could list the names and ids of every page.
            */
            List<Page> pageList = qry.Where( a => !hidePageIdList.Contains( a.Id ) ).OrderBy( a => a.Order ).ThenBy( a => a.InternalName ).ToList()
                .Where( p => PageCache.Get( p.Id )?.IsAuthorized( Rock.Security.Authorization.VIEW, person ) == true )
                .ToList();
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
                .Select( p => new { p.Id, ParentPageId = p.ParentPageId.Value } )
                .ToList()
                .Where( p => PageCache.Get( p.Id )?.IsAuthorized( Rock.Security.Authorization.VIEW, person ) == true )
                .Select( p => p.ParentPageId )
                .Distinct()
                .ToList();

            foreach ( var g in pageItemList )
            {
                int pageId = int.Parse( g.Id );
                g.HasChildren = qryHasChildren.Any( a => a == pageId );
                g.IconCssClass = "ti ti-file";
            }

            return pageItemList.AsQueryable();
        }
    }
}