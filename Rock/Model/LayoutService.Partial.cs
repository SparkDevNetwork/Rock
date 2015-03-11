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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.Site"/> entity. This inherits from the Service class
    /// </summary>
    public partial class LayoutService
    {
        /// <summary>
        /// Gets the Layout by site id.
        /// </summary>
        /// <param name="siteId">The site id.</param>
        /// <returns></returns>
        public IQueryable<Layout> GetBySiteId( int siteId )
        {
            return Queryable().Where( l => l.SiteId == siteId ).OrderBy( l => l.Name );
        }

        /// <summary>
        /// Registers any layouts in a particular site's theme folder that do not currently have any layouts registered in Rock.
        /// </summary>
        /// <param name="physWebAppPath">A <see cref="System.String" /> containing the physical path to Rock on the server.</param>
        /// <param name="site">The site.</param>
        public static void RegisterLayouts( string physWebAppPath, SiteCache site )
        {
            // Dictionary for block types.  Key is path, value is friendly name
            var list = new Dictionary<string, string>();

            // Find all the layouts in theme layout folder...
            string layoutFolder = Path.Combine(physWebAppPath, string.Format("Themes\\{0}\\Layouts", site.Theme));

            // search for all layouts (aspx files) under the physical path 
            var layoutFiles = new List<string>();
            DirectoryInfo di = new DirectoryInfo( layoutFolder );
            if ( di.Exists )
            {
                foreach(var file in di.GetFiles( "*.aspx", SearchOption.AllDirectories ))
                {
                    layoutFiles.Add( Path.GetFileNameWithoutExtension( file.Name ) );
                }
            }

            var rockContext = new RockContext();
            var layoutService = new LayoutService( rockContext );

            // Get a list of the layout filenames already registered 
            var registered = layoutService.GetBySiteId( site.Id ).Select( l => l.FileName ).Distinct().ToList();

            // for each unregistered layout
            foreach ( string layoutFile in layoutFiles.Except( registered, StringComparer.CurrentCultureIgnoreCase ) )
            {
                // Create new layout record and save it
                Layout layout = new Layout();
                layout.SiteId = site.Id;
                layout.FileName = layoutFile;
                layout.Name = layoutFile.SplitCase();
                layout.Guid = new Guid();

                layoutService.Add( layout );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets the Guid for the Layout that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = Rock.Web.Cache.LayoutCache.Read( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }
    }
}


