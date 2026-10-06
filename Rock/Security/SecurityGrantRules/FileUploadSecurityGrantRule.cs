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

using Newtonsoft.Json;

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission to upload content files with the file uploader
    /// controls. Access is limited to the root folder the control was
    /// configured with.
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
        /// <param name="rootFolder">The root folder that uploads are allowed in.</param>
        public FileUploadSecurityGrantRule( string rootFolder )
            : base( Authorization.EDIT )
        {
            RootFolder = rootFolder;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( obj is FileUploadAccess access )
            {
                return string.Equals( NormalizeRootFolder( access.RootFolder ), NormalizeRootFolder( RootFolder ), StringComparison.OrdinalIgnoreCase );
            }

            return false;
        }

        /// <summary>
        /// Normalizes the root folder so that equivalent values can be compared.
        /// </summary>
        /// <param name="rootFolder">The root folder.</param>
        /// <returns>The normalized root folder.</returns>
        private static string NormalizeRootFolder( string rootFolder )
        {
            if ( rootFolder.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return rootFolder.Trim().Replace( '\\', '/' ).TrimEnd( '/' );
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
            /// Initializes a new instance of the <see cref="FileUploadAccess"/> class.
            /// </summary>
            /// <param name="rootFolder">The root folder the file is being uploaded to.</param>
            public FileUploadAccess( string rootFolder )
            {
                RootFolder = rootFolder;
            }
        }

        #endregion
    }
}
