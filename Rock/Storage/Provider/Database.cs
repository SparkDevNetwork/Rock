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
        /// <param name="file">The file.</param>
        public override void SaveContent( BinaryFile file )
        {
            if ( file.DatabaseData == null )
            {
                file.DatabaseData = new BinaryFileData();
            }

            using ( var stream = file.ContentStream )
            {
                file.DatabaseData.Content = stream.ReadBytesToEnd();
            }
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
