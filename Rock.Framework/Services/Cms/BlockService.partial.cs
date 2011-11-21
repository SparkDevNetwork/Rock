//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Rock.Models.Cms;
using Rock.Repository.Cms;

namespace Rock.Services.Cms
{
	public partial class BlockService
	{
		/// <summary>
		/// Gets a list of Blocks on the filesystem that have not yet been registered in the repository.
		/// </summary>
		/// <param name="physWebAppPath">the physical path of the web application</param>
		/// <returns>a collection of <see cref="Rock.Models.Cms.Block">Blocks</see> that are not yet registered</returns>
        public IEnumerable<Rock.Models.Cms.Block> GetUnregisteredBlocks( string physWebAppPath )
        {
            // Determine the block path (it will be "C:\blahblahblah\Blocks\")
            string blockPath = physWebAppPath;
            blockPath += ( physWebAppPath.EndsWith( @"\" ) ) ? @"Blocks\" : @"\Blocks\";

            // search for all blocks under the block path 
            string[] allBlockNames = Directory.GetFiles( blockPath, "*.ascx", SearchOption.AllDirectories );

            // Convert them to logical file/path: ~/Blocks/foo/bar.ascx
            List<string> list = new List<string>();
            string fileName = string.Empty;
            for ( int i = 0; i < allBlockNames.Length; i++ )
            {
                fileName = allBlockNames[i].Replace( blockPath, "~/Blocks/" );
                list.Add( fileName.Replace( @"\", "/" ) );
            }

            // Now remove from the list any that are already registered (via the path)
            var registered = from r in Repository.GetAll() select r.Path;
            return ( from u in list.Except( registered ) select new Block { Path = u, Guid = Guid.NewGuid() } );
        }
	}
}