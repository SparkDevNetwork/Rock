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
    }
}
