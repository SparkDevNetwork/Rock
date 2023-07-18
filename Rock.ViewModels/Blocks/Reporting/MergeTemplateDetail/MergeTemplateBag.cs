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

using Rock.Enums.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Reporting.MergeTemplateDetail
{
    public class MergeTemplateBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the type of the merge template type entity.
        /// </summary>
        public ListItemBag MergeTemplateTypeEntityType { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        public ListItemBag PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the template binary file.
        /// </summary>
        public ListItemBag TemplateBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the merge template ownership.
        /// </summary>
        public MergeTemplateOwnership MergeTemplateOwnership { get; set; }
    }
}
