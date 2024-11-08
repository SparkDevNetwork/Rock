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


using System.Text.RegularExpressions;

namespace Rock.Storage
{
    /// <summary>
    /// A simple model for the AssetManager control, representing an asset and each part of its path.
    /// </summary>
    public class AssetManagerAsset
    {
        /// <summary>
        /// ID for the asset storage provider this asset resides in (0 if on local file system)
        /// </summary>
        public int? ProviderId { get; set; }

        /// <summary>
        /// Encrypted version of the root folder
        /// </summary>
        public string EncryptedRoot { get; set; } = string.Empty;

        /// <summary>
        /// Unencrypted version of the root folder
        /// </summary>
        public string Root { get; set; } = string.Empty;

        /// <summary>
        /// Portion of the path that comes after the root
        /// </summary>
        public string SubPath { get; set; } = string.Empty;

        /// <summary>
        /// The combination of the root path and the sub path
        /// </summary>
        public string FullPath
        {
            get
            {
                return Regex.Replace( Root + SubPath, "[" + Regex.Escape( string.Concat( System.IO.Path.GetInvalidPathChars() ) ) + "]", string.Empty, RegexOptions.CultureInvariant ).Replace( '\\', '/' ).TrimStart( '/' );
            }
        }

        /// <summary>
        /// The combination of the root path and the sub path, excluding any file names
        /// </summary>
        public string FullDirectoryPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName( FullPath ).Replace( '\\', '/' ) + "/";
            }
        }
        /// <summary>
        /// Name of the file, minus any path
        /// </summary>
        public string FileName
        {
            get
            {
                var fileName = System.IO.Path.GetFileName( FullPath );
                return Regex.Replace( fileName, "[" + Regex.Escape( string.Concat( System.IO.Path.GetInvalidFileNameChars() ) ) + "]", string.Empty, RegexOptions.CultureInvariant );
            }
        }

        /// <summary>
        /// Whether this asset represents the root folder of the file manager/asset provider
        /// </summary>
        public bool IsRoot
        {
            get
            {
                return Root != null && Root != string.Empty && ( SubPath == null || SubPath == string.Empty );
            }
        }

        /// <summary>
        /// Whether this represents an asset that resides in an asset storage provider.
        /// </summary>
        public bool IsAssetProviderAsset
        {
            get
            {
                return ProviderId != null && ProviderId > 0;
            }
        }

        /// <summary>
        /// Whether this represents an asset that resides in the local file system.
        /// </summary>
        public bool IsLocalAsset
        {
            get
            {
                return ProviderId == 0;
            }
        }
    }
}
