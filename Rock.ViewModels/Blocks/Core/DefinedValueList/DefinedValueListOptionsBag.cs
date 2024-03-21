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

namespace Rock.ViewModels.Blocks.Core.DefinedValueList
{
    /// <summary>
    /// The additional configuration options for the Defined Value List block.
    /// </summary>
    public class DefinedValueListOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block should be visible to the user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a valid DefinedType exists; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the defined type has CategorizedValuesEnabled set to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the define type has categorized values enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsCategorizedValuesEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the defined type has EnableSecurityOnValues set to true.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the defined type has EnableSecurityOnValues set to true; otherwise, <c>false</c>.
        /// </value>
        public bool IsSecurityOnValuesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the defined type.
        /// </summary>
        /// <value>
        /// The name of the defined type.
        /// </value>
        public string DefinedTypeName { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string EntityTypeQualifierValue { get; set; }
    }
}
