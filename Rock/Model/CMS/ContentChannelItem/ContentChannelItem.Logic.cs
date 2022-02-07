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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Rock.Lava;

namespace Rock.Model
{
    public partial class ContentChannelItem
    {
        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        [NotMapped]
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return ContentChannel != null ? ContentChannel : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets the primary slug.
        /// </summary>
        /// <value>
        /// The primary alias.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string PrimarySlug
        {
            get
            {
                return ContentChannelItemSlugs.Select( a => a.Slug ).FirstOrDefault();
            }
        }

        /// <summary>
        /// Gets the supported actions.
        /// </summary>
        /// <value>
        /// The supported actions.
        /// </value>
        [NotMapped]
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( Rock.Security.Authorization.INTERACT, "The roles and/or users that have access to interact with the channel item." );
                return supportedActions;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [allows interactive bulk indexing].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allows interactive bulk indexing]; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        [NotMapped]
        public bool AllowsInteractiveBulkIndexing
        {
            get
            {
                return true;
            }
        }
    }
}
