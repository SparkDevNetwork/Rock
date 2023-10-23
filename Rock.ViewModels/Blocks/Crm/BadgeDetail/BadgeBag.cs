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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.BadgeDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class BadgeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the badge component Rock.Model.EntityType.
        /// </summary>
        public ListItemBag BadgeComponentEntityType { get; set; }

        /// <summary>
        /// Gets or sets a description of the badge.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the subject entity Rock.Model.EntityType.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column that contains the value (see Rock.Model.Badge.EntityTypeQualifierValue) that is used narrow the scope of the Badge to a subset or specific instance of an EntityType.
        /// </summary>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value that is used to narrow the scope of the Badge to a subset or specific instance of an EntityType.
        /// </summary>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the given Name of the badge. This value is an alternate key and is required.
        /// </summary>
        public string Name { get; set; }
    }
}
