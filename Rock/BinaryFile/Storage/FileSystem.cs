//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Web;
using Rock.Model;

namespace Rock.BinaryFile.Storage
{
    [Description( "File System-driven document storage" )]
    [Export( typeof( StorageComponent ) )]
    [ExportMetadata( "ComponentName", "FileSystem" )]
    public class FileSystem : StorageComponent
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
                if ( file.Data == null )
                {
                    throw new ArgumentException( "File Data must not be null." );
                }

                // TODO: Should the file upload location be determined by the BinaryFileType?
                var relativePath = string.Format( "~/Assets/Uploads/{0}", file.FileName );
                var physicalPath = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, relativePath );
                File.WriteAllBytes( physicalPath, file.Data.Content );

                // Set Data to null after successful OS save so the the binary data is not 
                // written into the database.
                file.Data = null;
                file.Url = relativePath;
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
            File.Delete( HttpContext.Current.Server.MapPath( file.Url ) );
            fileService.Delete( file, personId );
        }
    }
}
