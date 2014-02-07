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
        /// Gets the specified unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        public override BinaryFile Get( Guid guid )
        {
            BinaryFile binaryFile = base.Get( guid );
            GetFileContentFromStorageProvider( binaryFile );
            return binaryFile;
        }

        /// <summary>
        /// Gets the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override BinaryFile Get( int id )
        {
            BinaryFile binaryFile = base.Get( id );
            GetFileContentFromStorageProvider( binaryFile );
            return binaryFile;
        }

        /// <summary>
        /// Gets the file content from storage provider.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        private void GetFileContentFromStorageProvider( BinaryFile binaryFile )
        {
            Rock.Storage.ProviderComponent storageProvider = DetermineBinaryFileStorageProvider( binaryFile );

            if ( storageProvider != null )
            {
                binaryFile.Data = binaryFile.Data ?? new BinaryFileData();
                binaryFile.Data.Content = storageProvider.GetFileContent( binaryFile, HttpContext.Current );
            }
        }

        /// <summary>
        /// Saves the specified <see cref="Rock.Model.BinaryFile"/>.
        /// </summary>
        /// <param name="item">A <see cref="Rock.Model.BinaryFile"/> to save.</param>
        /// <param name="personAlias">A <see cref="Rock.Model.PersonAlias"/> representing the <see cref="Rock.Model.Person"/> who is saving the BinaryFile..</param>
        /// <returns></returns>
        public override bool Save( BinaryFile item, PersonAlias personAlias )
        {
            Rock.Storage.ProviderComponent storageProvider = DetermineBinaryFileStorageProvider( item );

            if ( storageProvider != null )
            {
                //// if this file is getting replaced, and we can determine the StorageProvider, use the provider to get and remove the file from the provider's 
                //// external storage medium before we save it again. This especially important in cases where the provider for this filetype has changed 
                //// since it was last saved

                // first get the FileContent from the old/current fileprovider in case we need to save it somewhere else
                item.Data = item.Data ?? new BinaryFileData();
                item.Data.Content = storageProvider.GetFileContent( item, HttpContext.Current );

                // now, remove it from the old/current fileprovider
                storageProvider.RemoveFile( item, HttpContext.Current );
            }

            // when a file is saved (unless it is getting Deleted/Saved), it should use the StoredEntityType that is associated with the BinaryFileType
            if ( item.BinaryFileType != null )
            {
                // make sure that it updated to use the same storage as specified by the BinaryFileType
                if ( item.StorageEntityTypeId != item.BinaryFileType.StorageEntityTypeId )
                {
                    item.SetStorageEntityTypeId( item.BinaryFileType.StorageEntityTypeId );
                    storageProvider = DetermineBinaryFileStorageProvider( item );
                }
            }

            if ( storageProvider != null )
            {
                // save the file to the provider's new storage medium
                storageProvider.SaveFile( item, HttpContext.Current );
            }

            return base.Save( item, personAlias );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="personAlias">The person alias.</param>
        /// <returns></returns>
        public override bool Delete( BinaryFile item, PersonAlias personAlias )
        {
            // if we can determine the StorageProvider, use the provider to remove the file from the provider's external storage medium
            Rock.Storage.ProviderComponent storageProvider = DetermineBinaryFileStorageProvider( item );

            if ( storageProvider != null )
            {
                storageProvider.RemoveFile( item, HttpContext.Current );
            }

            // delete the record from the database
            return base.Delete( item, personAlias );
        }

        /// <summary>
        /// Determines the storage provider that was used the last time the file was saved
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        private Storage.ProviderComponent DetermineBinaryFileStorageProvider( BinaryFile item )
        {
            Rock.Storage.ProviderComponent storageProvider = null;

            if ( item != null )
            {
                item.StorageEntityType = item.StorageEntityType ?? new EntityTypeService( this.RockContext ).Get( item.StorageEntityTypeId ?? 0 );
                if ( item.StorageEntityType != null )
                {
                    storageProvider = Rock.Storage.ProviderContainer.GetComponent( item.StorageEntityType.Name );
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
            cmd.CommandText = "spBinaryFileGet";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add( new SqlParameter( "@Id", fileId.HasValue ? fileId.Value : 0 ) );
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
                binaryFile.IsTemporary = ( (bool)reader["IsTemporary"] );
                binaryFile.IsSystem = (bool)reader["IsSystem"];
                binaryFile.BinaryFileTypeId = reader["BinaryFileTypeId"] as int?;
                binaryFile.Url = reader["Url"] as string;
                binaryFile.FileName = reader["FileName"] as string;
                binaryFile.MimeType = reader["MimeType"] as string;
                binaryFile.ModifiedDateTime = reader["ModifiedDateTime"] as DateTime?;
                binaryFile.Description = reader["Description"] as string;
                int? storageEntityTypeId = reader["StorageEntityTypeId"] as int?;
                binaryFile.SetStorageEntityTypeId( storageEntityTypeId );
                var guid = reader["Guid"];
                if ( guid is Guid )
                {
                    binaryFile.Guid = (Guid)guid;
                }

                string entityTypeName = reader["StorageEntityTypeName"] as string;

                binaryFile.Data = new BinaryFileData();

                // read the fileContent from the database just in case it's stored in the database, otherwise, the Provider will get it
                var content = reader["Content"];
                if ( content != null )
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
