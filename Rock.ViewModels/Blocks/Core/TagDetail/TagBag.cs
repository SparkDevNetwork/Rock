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

namespace Rock.ViewModels.Blocks.Core.TagDetail
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class TagBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the background color of each tag
        /// </summary>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.EntityType of the Entities that this Tag can be applied to.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the name of the column/property that contains the value that can narrow the scope of entities that can receive this Tag. Entities where this
        /// column contains the Rock.Model.Tag.EntityTypeQualifierValue will be eligible to have this Tag. This property must be used in conjunction with the Rock.Model.Tag.EntityTypeQualifierValue
        /// property. If all entities of the specified Rock.Model.EntityType are eligible to use this Tag, this property will be null.
        /// </summary>
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the value in the Rock.Model.Tag.EntityTypeQualifierColumn that narrows the scope of entities that can receive this Tag. Entities that contain this value
        /// in the Rock.Model.Tag.EntityTypeQualifierColumn are eligible to use this Tag. This property must be used in conjunction with the Rock.Model.Tag.EntityTypeQualifierColumn property.
        /// </summary>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Tag. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the owner person alias.
        /// </summary>
        public ListItemBag OwnerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user has Administrate Authorization.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }
    }
}
