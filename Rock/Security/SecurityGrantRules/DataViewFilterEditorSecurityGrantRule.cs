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
using System.ServiceModel;

namespace Rock.Security.SecurityGrantRules
{
    /// <summary>
    /// Grants permission to use the Obsidian Data View Filter Editor.
    /// </summary>
    [Rock.SystemGuid.SecurityGrantRuleGuid( "535ED78C-BC24-4CA0-916C-81C39C0C396D" )]
    internal sealed class DataViewFilterEditorSecurityGrantRule : SecurityGrantRule
    {
        #region Properties

        /// <summary>
        /// The Entity Type unique identifier that the Data View Filter Editor will be used for.
        /// This is required in order to ensure that the Data View Filter Editor is only used for the specified Entity Type,
        /// otherwise, it could be used to access other Entity Types that the user should not have access to.
        /// </summary>
        public Guid EntityTypeGuid { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize an instance of the <see cref="DataViewFilterEditorSecurityGrantRule"/> class for default access (VIEW).
        /// </summary>
        public DataViewFilterEditorSecurityGrantRule()
            : base( Authorization.VIEW )
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool IsAccessGranted( object obj, string action )
        {
            if ( obj is Guid entityTypeGuid )
            {
                return entityTypeGuid == EntityTypeGuid && Action == action;
            }

            return false;
        }

        #endregion
    }
}
