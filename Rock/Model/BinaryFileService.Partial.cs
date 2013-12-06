//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace Rock.Model
{
    /// <summary>
    /// Data Access Service class for <see cref="Rock.Model.BinaryFile"/> objects.
    /// </summary>
    public partial class BinaryFileService
    {
        /// <summary>
        /// Saves the specified <see cref="Rock.Model.BinaryFile"/>.
        /// </summary>
        /// <param name="item">A <see cref="Rock.Model.BinaryFile"/> to save.</param>
        /// <param name="personId">A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who is saving the BinaryFile..</param>
        /// <returns></returns>
        public override bool Save( BinaryFile item, int? personId )
        {
            item.LastModifiedDateTime = DateTime.Now;
            Rock.Storage.ProviderComponent storageProvider = DetermineBinaryFileStorageProvider( item );

            if ( storageProvider != null )
            {
                //// if this file is getting replaced, and we can determine the StorageProvider, use the provider to remove the file from the provider's 
                //// external storage medium before we save it again. This especially important in cases where the provider for this filetype has changed 
                //// since it was last saved
                storageProvider.RemoveFile( item, HttpContext.Current );

                // save the file to the provider's storage medium
                storageProvider.SaveFile( item, HttpContext.Current );
            }

            // when a file is saved, it should use the StoredEntityType that is associated with the BinaryFileType
            item.StorageEntityTypeId = item.BinaryFileType.StorageEntityTypeId;
            return base.Save( item, personId );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public override bool Delete( BinaryFile item, int? personId )
        {
            // if we can determine the StorageProvider, use the provider to remove the file from the provider's external storage medium
            Rock.Storage.ProviderComponent storageProvider = DetermineBinaryFileStorageProvider( item );

            if ( storageProvider != null )
            {
                storageProvider.RemoveFile( item, HttpContext.Current );
            }

            // delete the record from the database
            return base.Delete( item, personId );
        }

        /// <summary>
        /// Determines the storage provider that was used the last time the file was saved
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private Storage.ProviderComponent DetermineBinaryFileStorageProvider( BinaryFile item )
        {
            Rock.Storage.ProviderComponent storageProvider = null;
            item.StorageEntityType = item.StorageEntityType ?? new EntityTypeService().Get( item.StorageEntityTypeId ?? 0 );
            if ( item.StorageEntityType != null )
            {
                item.BinaryFileType = item.BinaryFileType ?? new BinaryFileTypeService().Get( item.BinaryFileTypeId ?? 0 );
                if ( item.BinaryFileType != null )
                {
                    storageProvider = Rock.Storage.ProviderContainer.GetComponent( item.BinaryFileType.StorageEntityType.Name );
                }
            }

            return storageProvider;
        }

        /// <summary>
        /// Initiates an asynchronous get of the binary file specified by fileGuid
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="context">The context.</param>
        /// <param name="fileGuid">The file unique identifier.</param>
        /// <returns></returns>
        public IAsyncResult BeginGet( AsyncCallback callback, HttpContext context, Guid fileGuid )
        {
            return BeginGet( callback, context, fileGuid, null );
        }

        /// <summary>
        /// Initiates an asynchronous get of the binary file specified by fileId
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="context">The context.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        public IAsyncResult BeginGet( AsyncCallback callback, HttpContext context, int fileId )
        {
            return BeginGet( callback, context, Guid.Empty, fileId );
        }

        /// <summary>
        /// Initiates an asynchronous get of the binary file specified by fileId or fileGuid
        /// Intended to be used by an IHttpAsyncHandler
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="context">The context.</param>
        /// <param name="fileGuid">The file unique identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <returns></returns>
        private IAsyncResult BeginGet( AsyncCallback callback, HttpContext context, Guid fileGuid, int? fileId )
        {
            SqlConnection conn = new SqlConnection( string.Format( "{0};Asynchronous Processing=true;", ConfigurationManager.ConnectionStrings["RockContext"].ConnectionString ) );
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "BinaryFile_sp_getByID";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add( new SqlParameter( "@Id", fileId ) );
            cmd.Parameters.Add( new SqlParameter( "@Guid", fileGuid ) );

            // store our Command to be later retrieved by EndGet
            context.Items.Add( "cmd", cmd );

            // start async DB read
            return cmd.BeginExecuteReader( 
                callback, 
                context,
                CommandBehavior.SequentialAccess |  // doesn't load whole column into memory
                CommandBehavior.SingleRow |         // performance improve since we only want one row
                CommandBehavior.CloseConnection );  // close connection immediately after read
        }

        /// <summary>
        /// Ends the get.
        /// </summary>
        /// <param name="asyncResult">The asynchronous result.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public BinaryFile EndGet( IAsyncResult asyncResult, HttpContext context )
        {
            // restore the command from the context
            SqlCommand cmd = (SqlCommand)context.Items["cmd"];

            using ( SqlDataReader reader = cmd.EndExecuteReader( asyncResult ) )
            {
                BinaryFile binaryFile = new BinaryFile();

                // Columns must be read in Sequential Order (see stored procedure spBinaryFileGet)
                reader.Read();
                binaryFile.Id = reader["Id"] as int? ?? 0;
                binaryFile.IsTemporary = ( reader["IsTemporary"] as int? ) == 1;
                binaryFile.IsSystem = ( reader["IsSystem"] as int? ) == 1;
                binaryFile.BinaryFileTypeId = reader["BinaryFileTypeId"] as int?;
                binaryFile.Url = reader["Url"] as string;
                binaryFile.FileName = reader["FileName"] as string;
                binaryFile.MimeType = reader["MimeType"] as string;
                binaryFile.LastModifiedDateTime = reader["LastModifiedDateTime"] as DateTime?;
                binaryFile.Description = reader["Description"] as string;
                binaryFile.StorageEntityTypeId = reader["StorageEntityTypeId"] as int?;
                var guid = reader["Guid"];
                if ( guid is Guid )
                {
                    binaryFile.Guid = (Guid)guid;
                }

                string entityTypeName = reader["StorageEntityTypeName"] as string;
                
                binaryFile.Data = new BinaryFileData();

                // read the fileContent from the database just in case it's stored in the database, otherwise, the Provider will get it
                var content = reader["Content"];
                if (content != null)
                {
                    binaryFile.Data.Content = content as byte[];
                }
                
                Rock.Storage.ProviderComponent provider = Rock.Storage.ProviderContainer.GetComponent( entityTypeName );

                binaryFile.Data.Content = provider.GetFileContent( binaryFile, context );

                return binaryFile;
            }
        }
    }
}
