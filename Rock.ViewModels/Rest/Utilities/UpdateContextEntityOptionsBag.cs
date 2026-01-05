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

using Rock.ViewModels.Cms;

namespace Rock.ViewModels.Rest.Utilities
{
    /// <summary>
    /// Defines the options that can be sent to the ContextEntities API
    /// endpoints when setting context entity values.
    /// </summary>
    public class UpdateContextEntityOptionsBag
    {
        /// <summary>
        /// The context entity to be set. At least one of either EntityType
        /// or EntityTypeGuid must be set. The EntityGuid must also be set.
        /// </summary>
        public ContextEntityItemBag ContextEntity { get; set; }

        /// <summary>
        /// Determines whether the context entity is specific to a page.
        /// </summary>
        public int? PageId { get; set; }
    }
}
