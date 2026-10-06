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
    /// Grants permission to use the Obsidian Group Role Picker. The rule can
    /// be limited to the roles of a single group type.
    /// </summary>
    [Rock.SystemGuid.SecurityGrantRuleGuid( "7b9f13f3-98c3-47ec-9c7a-36fd1d6e1c85" )]
    internal sealed class GroupRolePickerSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// The singleton instance that is used to check if the list of group
        /// types can be retrieved. This is only granted when the rule is not
        /// limited to a single group type.
        /// </summary>
        public static object GroupTypeListInstance { get; } = new object();

        /// <summary>
        /// Gets the identifier of the group type whose roles can be listed.
        /// If <c>null</c> then the roles of any group type can be listed.
        /// </summary>
        /// <value>The identifier of the group type whose roles can be listed.</value>
        [JsonProperty( "gt", DefaultValueHandling = DefaultValueHandling.Ignore )]
        public int? GroupTypeId { get; private set; }

        #endregion

        #region Classes

        /// <summary>
        /// The object that must be used for authorization checks when loading
        /// the roles of a group type. A dedicated type is used so this rule
        /// never grants access to the group type itself in other parts of
        /// the system.
        /// </summary>
        public sealed class Access
        {
            /// <summary>
            /// Gets the identifier of the group type whose roles will be listed.
            /// </summary>
            /// <value>The identifier of the group type whose roles will be listed.</value>
            public int GroupTypeId { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Access"/> class.
            /// </summary>
            /// <param name="groupTypeId">The identifier of the group type whose roles will be listed.</param>
            public Access( int groupTypeId )
            {
                GroupTypeId = groupTypeId;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Prevents a default instance of the <see cref="GroupRolePickerSecurityGrantRule"/> class from being created.
        /// </summary>
        private GroupRolePickerSecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        /// <summary>
        /// Initialize an instance of the <see cref="GroupRolePickerSecurityGrantRule"/> class
        /// for granting <see cref="Authorization.VIEW"/> access to group roles.
        /// </summary>
        /// <param name="groupTypeId">The identifier of the group type whose roles can be listed, or <c>null</c> to allow any group type.</param>
        public GroupRolePickerSecurityGrantRule( int? groupTypeId )
            : base( Authorization.VIEW )
        {
            GroupTypeId = groupTypeId;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( ReferenceEquals( obj, GroupTypeListInstance ) )
            {
                return !GroupTypeId.HasValue;
            }

            if ( obj is Access access )
            {
                return !GroupTypeId.HasValue || access.GroupTypeId == GroupTypeId.Value;
            }

            return false;
        }

        #endregion
    }
}
