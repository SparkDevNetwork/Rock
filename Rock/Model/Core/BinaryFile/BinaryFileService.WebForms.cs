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
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

using Rock.Utility.Settings;

namespace Rock.Model
{
    /// <summary>
    /// Data Access Service class for <see cref="Rock.Model.BinaryFile"/> objects.
    /// </summary>
    public partial class BinaryFileService
    {
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
            SqlConnection conn = new SqlConnection( string.Format( "{0};Asynchronous Processing=true;", RockInstanceConfig.Database.ConnectionString ) );
            conn.Open();
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = "spCore_BinaryFileGet";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add( new SqlParameter( "@Id", fileId.HasValue ? fileId.Value : 0 ) );
            cmd.Parameters.Add( new SqlParameter( "@Guid", fileGuid ) );

            // store our Command to be later retrieved by EndGet
            context.AddOrReplaceItem( "Rock.Model.BinaryFileService:SqlCommand", cmd );

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
            bool requiresViewSecurity;
            return EndGet( asyncResult, context, out requiresViewSecurity );
        }

        /// <summary>
        /// Ends the get.
        /// </summary>
        /// <param name="asyncResult">The asynchronous result.</param>
        /// <param name="context">The context.</param>
        /// <param name="requiresViewSecurity">if set to <c>true</c> [requires security].</param>
        /// <returns></returns>
        public BinaryFile EndGet( IAsyncResult asyncResult, HttpContext context, out bool requiresViewSecurity )
        {
            // restore the command from the context
            SqlCommand cmd = ( SqlCommand ) context.Items["Rock.Model.BinaryFileService:SqlCommand"];

            using ( SqlDataReader reader = cmd.EndExecuteReader( asyncResult ) )
            {
                if ( !reader.Read() )
                {
                    requiresViewSecurity = false;
                    return null;
                }

                BinaryFile binaryFile = new BinaryFile();

                // Columns must be read in Sequential Order (see stored procedure spCore_BinaryFileGet)
                binaryFile.Id = reader["Id"] as int? ?? 0;
                binaryFile.IsTemporary = ( bool ) reader["IsTemporary"];
                binaryFile.IsSystem = ( bool ) reader["IsSystem"];
                binaryFile.BinaryFileTypeId = reader["BinaryFileTypeId"] as int?;

                // return requiresViewSecurity to let caller know that security needs to be checked on this binaryFile before viewing
                requiresViewSecurity = ( bool ) reader["RequiresViewSecurity"];

                binaryFile.FileName = reader["FileName"] as string;
                binaryFile.MimeType = reader["MimeType"] as string;
                binaryFile.ModifiedDateTime = reader["ModifiedDateTime"] as DateTime?;
                binaryFile.Description = reader["Description"] as string;
                int? storageEntityTypeId = reader["StorageEntityTypeId"] as int?;
                binaryFile.SetStorageEntityTypeId( storageEntityTypeId );
                var guid = reader["Guid"];
                if ( guid is Guid )
                {
                    binaryFile.Guid = ( Guid ) guid;
                }

                binaryFile.StorageEntitySettings = reader["StorageEntitySettings"] as string;
                binaryFile.Path = reader["Path"] as string;
                binaryFile.FileSize = reader["FileSize"] as long?;
                binaryFile.ParentEntityTypeId = reader["ParentEntityTypeId"] as int?;
                binaryFile.ParentEntityId = reader["ParentEntityId"] as int?;
                binaryFile.DatabaseData = new BinaryFileData();

                // read the fileContent from the database just in case it's stored in the database, otherwise, the Provider will get it
                // TODO do as a stream instead
                var content = reader["Content"] as byte[];
                if ( content != null )
                {
                    binaryFile.DatabaseData.Content = content;
                }

                return binaryFile;
            }
        }
    }
}
