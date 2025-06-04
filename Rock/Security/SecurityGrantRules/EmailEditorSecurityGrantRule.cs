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
    /// Grants permission to use the Obsidian Email Editor.
    /// </summary>
    [Rock.SystemGuid.SecurityGrantRuleGuid( "f41f8269-1399-47f5-a872-f84190c8d30e" )]
    internal sealed class EmailEditorSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// The singleton instance that will be used for authorization checks.
        /// </summary>
        public static object AccessInstance { get; } = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize an instance of the <see cref="EmailEditorSecurityGrantRule"/> class for default access (VIEW).
        /// </summary>
        public EmailEditorSecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            // Only the exact instance of our singleton will be allowed.
            return ReferenceEquals( obj, AccessInstance );
        }

        #endregion
    }
}
