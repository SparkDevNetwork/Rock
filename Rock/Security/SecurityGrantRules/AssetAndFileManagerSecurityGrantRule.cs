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

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission to use the Obsidian Asset Manager.
    /// </summary>
    /// <seealso cref="Rock.Security.SecurityGrantRule" />
    [Rock.SystemGuid.SecurityGrantRuleGuid( "6fc5fe64-e913-4d73-8047-a0d7a3a800d1" )]
    internal class AssetAndFileManagerSecurityGrantRule : SecurityGrantRule
    {
        #region Constructors

        /// <summary>
        /// Initialize an instance of the <see cref="AssetAndFileManagerSecurityGrantRule"/> class for default access (VIEW).
        /// </summary>
        public AssetAndFileManagerSecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initialize an instance of the <see cref="AssetAndFileManagerSecurityGrantRule"/> class for the specified access.
        /// </summary>
        public AssetAndFileManagerSecurityGrantRule( string action )
            : base( action )
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            // Only the asset manager API endpoints will pass an instance of
            // the AssetAndFileManagerAccess class. In that case we authorize.
            return obj is AssetAndFileManagerAccess;
        }

        #endregion

        #region Singleton

        /// <summary>
        /// This grant rule doesn't have an entity or other specific object to
        /// check when authorizing. So instead we use this singleton class to
        /// be the object. If the object being authorized is an instance of
        /// this class, then we authorize.
        /// </summary>
        internal sealed class AssetAndFileManagerAccess
        {
            /// <summary>
            /// The singleton that will be used by API endpoints when authorizing
            /// requests to the asset manager stuff.
            /// </summary>
            public static AssetAndFileManagerAccess Instance { get; } = new AssetAndFileManagerAccess();

            /// <summary>
            /// This prevents creating additional instances of this class.
            /// </summary>
            private AssetAndFileManagerAccess()
            {
            }
        }

        #endregion
    }
}
