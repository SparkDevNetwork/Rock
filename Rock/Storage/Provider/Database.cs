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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;

using Rock.Model;

namespace Rock.Storage.Provider
{
    /// <summary>
    /// Storage provider for saving binary files to Rock database
    /// </summary>
    [Description( "Database-driven file storage" )]
    [Export( typeof( ProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Database" )]
    public class Database : ProviderComponent
    {
        /// <summary>
        /// Saves the binary file contents to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        public override void SaveContent( BinaryFile binaryFile )
        {
            long? fileSize = null;
            SaveContent( binaryFile, out fileSize );
        }

        /// <summary>
        /// Saves the binary file contents to the external storage medium associated with the provider.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        /// <param name="fileSize">Size of the file.</param>
        /// <exception cref="System.ArgumentException">File Data must not be null.</exception>
        public override void SaveContent( BinaryFile binaryFile, out long? fileSize )
        { 
            if ( binaryFile.DatabaseData == null )
            {
                binaryFile.DatabaseData = new BinaryFileData();
            }

            using ( var stream = binaryFile.ContentStream )
            {
                binaryFile.DatabaseData.Content = stream.ReadBytesToEnd();
            }

            fileSize = binaryFile.DatabaseData.Content.Length;
        }

        /// <summary>
        /// Deletes the content from the external storage medium associated with the provider.
        /// </summary>
        /// <param name="file">The file.</param>
        public override void DeleteContent( BinaryFile file )
        {
            file.DatabaseData = null;
        }

        /// <summary>
        /// Gets the contents from the external storage medium associated with the provider
        /// </summary>
        /// <param name="file">The file.</param>
        /// <returns></returns>
        public override Stream GetContentStream( BinaryFile file )
        {
            if ( file.DatabaseData != null && file.DatabaseData.Content != null )
            {
                return new MemoryStream( file.DatabaseData.Content );
            }
            return new MemoryStream();
        }

    }
}
