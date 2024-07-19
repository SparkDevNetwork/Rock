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

namespace Rock.ViewModels.Blocks.CheckIn.Config.CheckinTypeDetail
{
    /// <summary>
    /// The additional configuration options for the Group Type Detail block.
    /// </summary>
    public class CheckinTypeDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the achievement type options.
        /// </summary>
        /// <value>
        /// The achievement type options.
        /// </value>
        public List<ListItemBag> AchievementTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the search type options.
        /// </summary>
        /// <value>
        /// The search type options.
        /// </value>
        public List<ListItemBag> SearchTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the display options.
        /// </summary>
        /// <value>
        /// The display options.
        /// </value>
        public List<ListItemBag> DisplayOptions { get; set; }

        /// <summary>
        /// Gets or sets the relationship type options.
        /// </summary>
        /// <value>
        /// The relationship type options.
        /// </value>
        public List<ListItemBag> RelationshipTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the registration attribute options.
        /// </summary>
        /// <value>
        /// The registration attribute options.
        /// </value>
        public List<ListItemBag> PersonAttributeOptions { get; set; }

        /// <summary>
        /// Gets or sets the family attribute options.
        /// </summary>
        /// <value>
        /// The family attribute options.
        /// </value>
        public List<ListItemBag> FamilyAttributeOptions { get; set; }

        /// <summary>
        /// Gets or sets the template display options.
        /// </summary>
        /// <value>
        /// The template display options.
        /// </value>
        public List<ListItemBag> TemplateDisplayOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [hide panel].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [hide panel]; otherwise, <c>false</c>.
        /// </value>
        public bool HidePanel { get; set; }

        /// <summary>
        /// Gets or sets the name search defined value.
        /// </summary>
        /// <value>
        /// The name search.
        /// </value>
        public ListItemBag NameSearch { get; set; }
    }
}
