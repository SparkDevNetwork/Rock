//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
            return Repository.AsQueryable().Where( l => l.SiteId == siteId ).OrderBy( l => l.Name );
        }

        /// <summary>
        /// Registers any layouts in a particular site's theme folder that do not currently have any layouts registered in RockChMS.
        /// </summary>
        /// <param name="physWebAppPath">A <see cref="System.String" /> containing the physical path to RockChMS on the server.</param>
        /// <param name="site">The site.</param>
        /// <param name="currentPersonId">A <see cref="System.Int32" /> that contains the Id of the currently logged on <see cref="Rock.Model.Person" />.</param>
        public void RegisterLayouts( string physWebAppPath, SiteCache site, int? currentPersonId )
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

            // Get a list of the layout filenames already registered 
            var registered = GetBySiteId( site.Id ).Select( l => l.FileName ).Distinct().ToList();
                
            // for each unregistered layout
            foreach ( string layoutFile in layoutFiles.Except( registered, StringComparer.CurrentCultureIgnoreCase ) )
            {
                // Create new layout record and save it
                Layout layout = new Layout();
                layout.SiteId = site.Id;
                layout.FileName = layoutFile;
                layout.Name = layoutFile.SplitCase();
                layout.Guid = new Guid();

                this.Add( layout, currentPersonId );
                this.Save( layout, currentPersonId );
            }
        }

    }
}


