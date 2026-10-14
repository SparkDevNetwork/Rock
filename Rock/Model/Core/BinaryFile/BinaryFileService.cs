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
using System.IO;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data Access Service class for <see cref="Rock.Model.BinaryFile"/> objects.
    /// </summary>
    public partial class BinaryFileService
    {
        /// <summary>
        /// Adds the file from stream. This method will save the current context.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mimeType">Type of the MIME.</param>
        /// <param name="contentLength">Length of the content.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="binaryFileTypeGuid">The binary file type unique identifier.</param>
        /// <param name="imageGuid">The image unique identifier.</param>
        /// <returns></returns>
        public BinaryFile AddFileFromStream( Stream stream, string mimeType, long contentLength, string fileName, string binaryFileTypeGuid, Guid? imageGuid )
        {
            int? binaryFileTypeId = Rock.Web.Cache.BinaryFileTypeCache.GetId( binaryFileTypeGuid.AsGuid() );

            imageGuid = imageGuid == null || imageGuid == Guid.Empty ? Guid.NewGuid() : imageGuid;
            var rockContext = ( RockContext ) this.Context;
            using ( var memoryStream = new System.IO.MemoryStream() )
            {
                stream.CopyTo( memoryStream );
                var binaryFile = new BinaryFile
                {
                    IsTemporary = false,
                    BinaryFileTypeId = binaryFileTypeId,
                    MimeType = mimeType,
                    FileName = fileName,
                    FileSize = contentLength,
                    ContentStream = memoryStream,
                    Guid = imageGuid.Value
                };

                var binaryFileService = new BinaryFileService( rockContext );
                binaryFileService.Add( binaryFile );
                rockContext.SaveChanges();
                return binaryFile;
            }
        }

        /// <summary>
        /// Determines whether a binary file sent by a client may be attached to
        /// an entity by the person. This is allowed when no file is specified,
        /// when the file is the one already attached, or when the file is a
        /// temporary file that was not uploaded by somebody else. Uploads are
        /// always temporary until they are attached, so this prevents a client
        /// from attaching an existing file that belongs to something else.
        /// </summary>
        /// <param name="binaryFileId">The identifier of the binary file sent by the client.</param>
        /// <param name="currentBinaryFileId">The identifier of the binary file currently attached to the entity.</param>
        /// <param name="person">The person that is attaching the file, usually the current person.</param>
        /// <returns><c>true</c> if the binary file may be attached; otherwise <c>false</c>.</returns>
        public bool IsUploadedBinaryFileAllowedForPerson( int? binaryFileId, int? currentBinaryFileId, Person person )
        {
            if ( !binaryFileId.HasValue || binaryFileId == currentBinaryFileId )
            {
                return true;
            }

            var id = binaryFileId.Value;
            var binaryFile = Queryable()
                .Where( f => f.Id == id )
                .Select( f => new
                {
                    f.IsTemporary,
                    f.CreatedByPersonAliasId
                } )
                .FirstOrDefault();

            return binaryFile != null
                && binaryFile.IsTemporary
                && IsCreatedByPersonOrUnknown( binaryFile.CreatedByPersonAliasId, person );
        }

        /// <summary>
        /// Determines whether a binary file sent by a client may be attached to
        /// an entity by the person. This is allowed when no file is specified,
        /// when the file is the one already attached, or when the file is a
        /// temporary file that was not uploaded by somebody else. Uploads are
        /// always temporary until they are attached, so this prevents a client
        /// from attaching an existing file that belongs to something else.
        /// </summary>
        /// <param name="binaryFileGuid">The unique identifier of the binary file sent by the client.</param>
        /// <param name="currentBinaryFileGuid">The unique identifier of the binary file currently attached to the entity.</param>
        /// <param name="person">The person that is attaching the file, usually the current person.</param>
        /// <returns><c>true</c> if the binary file may be attached; otherwise <c>false</c>.</returns>
        public bool IsUploadedBinaryFileAllowedForPerson( Guid? binaryFileGuid, Guid? currentBinaryFileGuid, Person person )
        {
            if ( !binaryFileGuid.HasValue || binaryFileGuid == currentBinaryFileGuid )
            {
                return true;
            }

            var guid = binaryFileGuid.Value;
            var binaryFile = Queryable()
                .Where( f => f.Guid == guid )
                .Select( f => new
                {
                    f.IsTemporary,
                    f.CreatedByPersonAliasId
                } )
                .FirstOrDefault();

            return binaryFile != null
                && binaryFile.IsTemporary
                && IsCreatedByPersonOrUnknown( binaryFile.CreatedByPersonAliasId, person );
        }

        /// <summary>
        /// Determines whether the file was created by the person, or it is not
        /// known who created it. Files uploaded anonymously, or before the file
        /// uploader recorded who uploaded them, do not have a creator.
        /// </summary>
        /// <param name="createdByPersonAliasId">The person alias identifier that created the file.</param>
        /// <param name="person">The person to check.</param>
        /// <returns><c>true</c> if the file was created by the person or the creator is unknown; otherwise <c>false</c>.</returns>
        private bool IsCreatedByPersonOrUnknown( int? createdByPersonAliasId, Person person )
        {
            if ( !createdByPersonAliasId.HasValue )
            {
                return true;
            }

            if ( person == null )
            {
                return false;
            }

            var createdByPersonId = new PersonAliasService( ( RockContext ) Context ).GetPersonId( createdByPersonAliasId.Value );

            return createdByPersonId.HasValue && createdByPersonId.Value == person.Id;
        }
    }
}
