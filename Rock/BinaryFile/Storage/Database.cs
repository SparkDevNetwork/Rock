//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.BinaryFile.Storage
{
    [Description( "Database-driven document storage" )]
    [Export( typeof( StorageComponent ) )]
    [ExportMetadata( "ComponentName", "Database" )]
    public class Database : StorageComponent
    {
        /// <summary>
        /// Saves the files.
        /// </summary>
        /// <param name="files">The files.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void SaveFiles( IEnumerable<Model.BinaryFile> files )
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void RemoveFile( Model.BinaryFile file )
        {
            throw new System.NotImplementedException();
        }
    }
}
