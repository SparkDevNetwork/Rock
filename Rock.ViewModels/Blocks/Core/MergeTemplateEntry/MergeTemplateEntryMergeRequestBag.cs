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

namespace Rock.ViewModels.Blocks.Core.MergeTemplateEntry
{
    /// <summary>
    /// The information sent when generating a merge document or requesting the
    /// Lava merge fields help for the selected template.
    /// </summary>
    public class MergeTemplateEntryMergeRequestBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the selected merge template.
        /// </summary>
        public Guid? MergeTemplateGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether family members should be combined
        /// into a single row (for example, "Ted &amp; Cindy Decker").
        /// </summary>
        public bool IsCombineFamilyMembers { get; set; }
    }
}
