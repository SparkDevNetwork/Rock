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

using Newtonsoft.Json;

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission to use the Obsidian Group Member Picker to list the
    /// members of a single group.
    /// </summary>
    [Rock.SystemGuid.SecurityGrantRuleGuid( "9815c9d2-a89f-4537-b54f-5188c4315245" )]
    internal sealed class GroupMemberPickerSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// Gets the identifier of the group whose members can be listed.
        /// </summary>
        /// <value>The identifier of the group whose members can be listed.</value>
        [JsonProperty( "g", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public int GroupId { get; private set; }

        #endregion

        #region Classes

        /// <summary>
        /// The object that must be used for authorization checks with this
        /// rule. A dedicated type is used so this rule never grants access
        /// to the group entity itself in other parts of the system.
        /// </summary>
        public sealed class Access
        {
            /// <summary>
            /// Gets the identifier of the group whose members will be listed.
            /// </summary>
            /// <value>The identifier of the group whose members will be listed.</value>
            public int GroupId { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Access"/> class.
            /// </summary>
            /// <param name="groupId">The identifier of the group whose members will be listed.</param>
            public Access( int groupId )
            {
                GroupId = groupId;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="GroupMemberPickerSecurityGrantRule"/> class from being created.
        /// </summary>
        private GroupMemberPickerSecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initialize an instance of the <see cref="GroupMemberPickerSecurityGrantRule"/> class
        /// for granting <see cref="Authorization.VIEW"/> access to the members of a group.
        /// </summary>
        /// <param name="groupId">The identifier of the group whose members can be listed.</param>
        public GroupMemberPickerSecurityGrantRule( int groupId )
            : base( Authorization.VIEW )
        {
            GroupId = groupId;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( GroupId == 0 )
            {
                return false;
            }

            return obj is Access access && access.GroupId == GroupId;
        }

        #endregion
    }
}
