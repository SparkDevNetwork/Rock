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
    /// Grants permission to a specific entity given its entity type identifier
    /// and its entity identifier.
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
            // Grant all access to anyone with a token.
            return true;
        }

        #endregion
    }
}
