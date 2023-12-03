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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.DefinedTypeDetail
{
    /// <summary>
    /// The item details for the Defined Type Detail block.
    /// </summary>
    public class DefinedTypeBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a flag indicating if the Defined Values associated with this Defined Type can be grouped into categories.
        /// </summary>
        public bool? CategorizedValuesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the DefinedType.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the DefinedValues for this DefinedType should allow security settings.
        /// </summary>
        public bool EnableSecurityOnValues { get; set; }

        /// <summary>
        /// Gets or sets the help text for the defined type.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this Defined Type is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this DefinedType is part of the Rock core system/framework. This property is required.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the DefinedType.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the defined type attributes.
        /// </summary>
        /// <value>
        /// The defined type attributes.
        /// </value>
        public List<PublicEditableAttributeBag> DefinedTypeAttributes { get; set; }

        /// <summary>
        /// Gets or sets the report detail page URL.
        /// </summary>
        /// <value>
        /// The report detail page URL.
        /// </value>
        public string ReportDetailPageUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is being used on a stand alone page ( i.e. not navigated to through defined type list )
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is stand alone; otherwise, <c>false</c>.
        /// </value>
        public bool IsStandAlone { get; set; }
    }
}
