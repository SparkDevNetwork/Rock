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

namespace Rock.ViewModels.Blocks.Communication.SmsPipelineDetail
{
    /// <summary>
    /// The initialization details for the SMS Pipeline Detail block.
    /// </summary>
    public class SmsPipelineDetailInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the pipeline details, including its actions.
        /// </summary>
        public SmsPipelineBag Pipeline { get; set; }

        /// <summary>
        /// Gets or sets the SMS action component types available in the action editor,
        /// each with its per-instance attribute schema preloaded.
        /// </summary>
        public List<SmsActionComponentBag> AvailableComponents { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person may edit,
        /// save, or delete the pipeline and its actions.
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the testing drawer is available
        /// to the current person.
        /// </summary>
        public bool IsTestingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the attribute category name used to partition each component's
        /// per-instance attributes between the Filters section and the main Attributes
        /// section in the action editor.
        /// </summary>
        public string FilterCategoryName { get; set; }

        /// <summary>
        /// Gets or sets the attribute keys that represent scalar properties on the
        /// SmsAction entity and must be excluded from the rendered per-instance
        /// attribute editor.
        /// </summary>
        public List<string> SystemAttributeKeys { get; set; }
    }
}
