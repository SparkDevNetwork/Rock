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

namespace Rock.Storage.Provider
{
    /// <summary>
    /// Storage provider for saving binary files to Rock database
    /// </summary>
    [Description( "Database-driven document storage" )]
    [Export( typeof( ProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Database" )]
    public class Database : ProviderComponent
    {
        /// <summary>
        /// Removes the file from the external storage medium associated with the provider.
        /// Note: This does not delete the BinaryFile record from the database
        /// </summary>
        /// <param name="file">The file.</param>
        public override void RemoveFile( BinaryFile file)
        {
            // Database storage just stores everything in the BinaryFile table, so there is no external file data to delete
        }

        public override void SaveFile( BinaryFile file )
        {
            // Database storage just stores everything in the BinaryFile table, so there is no external file data to save
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override string GetUrl( BinaryFile file )
        {
            string wsPath;

            switch ( file.MimeType.ToLower() )
            {
                case "image/jpeg":
                case "image/gif":
                case "image/png":
                case "image/bmp":
                    wsPath = "~/GetImage.ashx";
                    break;
                default:
                    wsPath = "~/GetFile.ashx";
                    break;
            }

            return string.Format( "{0}?guid={1}", wsPath, file.Guid );
        }
    }
}
