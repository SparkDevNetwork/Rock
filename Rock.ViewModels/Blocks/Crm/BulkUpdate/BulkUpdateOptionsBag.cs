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

namespace Rock.ViewModels.Blocks.Crm.BulkUpdate
{
    /// <summary>
    /// The initialization options for the Bulk Update block.
    /// </summary>
    public class BulkUpdateOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current user can edit the connection status.
        /// </summary>
        public bool CanEditConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can edit the record status.
        /// </summary>
        public bool CanEditRecordStatus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user can edit the record source.
        /// </summary>
        public bool CanEditRecordSource { get; set; }

        /// <summary>
        /// Gets or sets the list of persons to update.
        /// </summary>
        public List<BulkUpdatePersonBag> UpdatePersons { get; set; }

        /// <summary>
        /// Gets or sets the attribute categories.
        /// </summary>
        public List<BulkUpdateAttributeCategoryBag> AttributeCategories { get; set; }

        /// <summary>
        /// Gets or sets the workflow type options.
        /// </summary>
        public List<ListItemBag> WorkflowTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the valid NoteType options.
        /// </summary>
        public List<ListItemBag> NoteTypeOptions { get; set; }

        /// <summary>
        /// Gets or sets the valid Tag options.
        /// </summary>
        public List<ListItemBag> TagOptions { get; set; }

        /// <summary>
        /// Gets or sets the valid Step Program options.
        /// </summary>
        public List<BulkUpdateStepProgramBag> StepProgramOptions { get; set; }

        /// <summary>
        /// Gets or sets an optional configuration error message to display
        /// to the user when block settings are misconfigured (e.g. an attribute
        /// appearing in more than one category).
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
