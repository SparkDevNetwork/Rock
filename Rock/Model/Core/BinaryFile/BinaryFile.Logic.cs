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
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents any file that has either been uploaded to or generated and saved to Rock.
    /// </summary>
    public partial class BinaryFile : Model<BinaryFile>
    {
        /// <summary>
        /// Sets the type of the storage entity.
        /// Should only be set by the BinaryFileService
        /// </summary>
        /// <param name="storageEntityTypeId">The storage entity type identifier.</param>
        public void SetStorageEntityTypeId( int? storageEntityTypeId )
        {
            StorageEntityTypeId = storageEntityTypeId;
        }

        /// <summary>
        /// Reads the file's content stream and converts to a string.
        /// </summary>
        /// <returns></returns>
        public string ContentsToString()
        {
            string contents = string.Empty;

            using ( var stream = this.ContentStream )
            {
                using ( var reader = new StreamReader( stream ) )
                {
                    contents = reader.ReadToEnd();
                }
            }

            return contents;
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [NotMapped]
        [DataMember]
        public virtual string Url
        {
            get
            {
                if ( StorageProvider != null )
                {
                    return StorageProvider.GetUrl( this );
                }
                else
                {
                    return Path;
                }
            }
            private set { }
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.BinaryFileType != null ? this.BinaryFileType : base.ParentAuthority;
            }
        }
    }
}
