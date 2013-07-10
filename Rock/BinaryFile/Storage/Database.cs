//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Rock.Model;

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
        /// <param name="personId"></param>
        public override void SaveFiles( IEnumerable<Model.BinaryFile> files, int? personId )
        {
            var fileService = new BinaryFileService();
            
            foreach ( var file in files )
            {
                if ( file.Id == 0 )
                {
                    fileService.Add( file, personId );
                }

                // Since we're expecting a hydrated model, throw if no binary
                // data is included.
                if ( file.Data == null )
                {
                    throw new ArgumentException( "File Data must not be null." );
                }

                // Save once to persist data...
                fileService.Save( file, personId );

                // Set the URL now that we have a Guid...
                file.Url = string.Format( "~/File.ashx?guid={0}", file.Guid );

                // Then save again to persist the URL
                fileService.Save( file, personId );
            }
        }

        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="personId"></param>
        public override void RemoveFile( Model.BinaryFile file, int? personId )
        {
            var fileService = new BinaryFileService();
            fileService.Delete( file, personId );
        }
    }
}
