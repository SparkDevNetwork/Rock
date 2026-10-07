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
using System.Linq;

using Newtonsoft.Json;

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission to upload content files with the file uploader
    /// controls. Access is limited to the root folder the control was
    /// configured with, or to asset storage providers when created with
    /// <see cref="ForAssetStorageProviders"/>.
    /// </summary>
    /// <seealso cref="Rock.Security.SecurityGrantRule" />
    [Rock.SystemGuid.SecurityGrantRuleGuid( "fac83585-8c2b-4252-a8a3-24bcd90a6460" )]
    public sealed class FileUploadSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// Gets the root folder that uploads are allowed in.
        /// </summary>
        /// <value>The root folder that uploads are allowed in.</value>
        [JsonProperty( "rf", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public string RootFolder { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this rule allows uploads to asset
        /// storage providers instead of a root folder.
        /// </summary>
        /// <value><c>true</c> if this rule allows uploads to asset storage providers; otherwise, <c>false</c>.</value>
        [JsonProperty( "ap", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public bool IsAssetStorageProvider { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="FileUploadSecurityGrantRule"/> class from being created.
        /// </summary>
        private FileUploadSecurityGrantRule()
            : base( Authorization.EDIT )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUploadSecurityGrantRule"/> class
        /// for granting <see cref="Authorization.EDIT"/> access.
        /// </summary>
        /// <param name="rootFolder">The root folder that uploads are allowed in. An empty value means the default ~/Content folder.</param>
        public FileUploadSecurityGrantRule( string rootFolder )
            : base( Authorization.EDIT )
        {
            RootFolder = rootFolder;
        }

        /// <summary>
        /// Creates a rule that allows uploads to asset storage providers.
        /// </summary>
        /// <returns>A new <see cref="FileUploadSecurityGrantRule"/> instance.</returns>
        public static FileUploadSecurityGrantRule ForAssetStorageProviders()
        {
            return new FileUploadSecurityGrantRule
            {
                IsAssetStorageProvider = true
            };
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( !( obj is FileUploadAccess access ) )
            {
                return false;
            }

            if ( access.IsAssetStorageProvider || IsAssetStorageProvider )
            {
                return access.IsAssetStorageProvider && IsAssetStorageProvider;
            }

            var allowedRootFolder = NormalizeRootFolder( RootFolder );
            var requestedRootFolder = NormalizeRootFolder( access.RootFolder );

            if ( allowedRootFolder == null || requestedRootFolder == null )
            {
                return false;
            }

            // Allow the root folder itself or any folder inside it. Uploads
            // can already target sub-folders of the root, this also covers
            // user specific roots that are built from the configured root.
            // A root of the site itself (~/) contains every other folder.
            return allowedRootFolder.Length == 0
                || string.Equals( requestedRootFolder, allowedRootFolder, StringComparison.OrdinalIgnoreCase )
                || requestedRootFolder.StartsWith( allowedRootFolder + "/", StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Normalizes the root folder so that equivalent values can be compared.
        /// An empty value is treated as the default ~/Content folder.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns>The normalized root folder, an empty string for the site root or <c>null</c> if it is not valid.</returns>
        private static string NormalizeRootFolder( string rootFolder )
        {
            if ( rootFolder.IsNullOrWhiteSpace() )
            {
                rootFolder = "~/Content";
            }

            var segments = rootFolder.Trim()
                .Replace( '\\', '/' )
                .TrimStart( '~' )
                .Split( new[] { '/' }, StringSplitOptions.RemoveEmptyEntries );

            if ( segments.Any( s => s.Trim() == "." || s.Trim() == ".." ) )
            {
                return null;
            }

            return string.Join( "/", segments );
        }

        #endregion

        #region Support Classes

        /// <summary>
        /// The object that is checked for permission when a content file
        /// is uploaded.
        /// </summary>
        public sealed class FileUploadAccess
        {
            /// <summary>
            /// Gets the root folder the file is being uploaded to.
            /// </summary>
            /// <value>The root folder the file is being uploaded to.</value>
            public string RootFolder { get; }

            /// <summary>
            /// Gets a value indicating whether the file is being uploaded to
            /// an asset storage provider.
            /// </summary>
            /// <value><c>true</c> if the file is being uploaded to an asset storage provider; otherwise, <c>false</c>.</value>
            public bool IsAssetStorageProvider { get; }

            /// <summary>
            /// Gets the access object for uploads to asset storage providers.
            /// </summary>
            /// <value>The access object for uploads to asset storage providers.</value>
            public static FileUploadAccess AssetStorageProvider { get; } = new FileUploadAccess( null, true );

            /// <summary>
            /// Initializes a new instance of the <see cref="FileUploadAccess"/> class.
            /// </summary>
            /// <param name="rootFolder">The root folder the file is being uploaded to.</param>
            public FileUploadAccess( string rootFolder )
                : this( rootFolder, false )
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="FileUploadAccess"/> class.
            /// </summary>
            /// <param name="rootFolder">The root folder the file is being uploaded to.</param>
            /// <param name="isAssetStorageProvider"><c>true</c> if the file is being uploaded to an asset storage provider.</param>
            private FileUploadAccess( string rootFolder, bool isAssetStorageProvider )
            {
                RootFolder = rootFolder;
                IsAssetStorageProvider = isAssetStorageProvider;
            }
        }

        #endregion
    }
}
