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

using Rock.ViewModel.NonEntities;

namespace Rock.ViewModel.Blocks.Workflow.FormBuilder.FormTemplateDetail
{
    /// <summary>
    /// Representation of a form template that provides the required information
    /// to display the read-only view on the template detail block.
    /// </summary>
    public class TemplateDetailViewModel
    {
        /// <summary>
        /// Gets or sets the name of the form template.
        /// </summary>
        /// <value>The name of the form template.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the descriptive purpose for the form template.
        /// </summary>
        /// <value>The descriptive purpose for the form template.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of workflow types that use this form template.
        /// </summary>
        /// <value>The list of workflow types that use this form template.</value>
        public List<ListItemViewModel> UsedBy { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if this form template is active and
        /// should show up as a selection item in lists.
        /// </summary>
        /// <value>
        /// <c>true</c> if this form template is active and should show up as a
        /// selection item in lists; otherwise <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the details that describe the audit trail for this view model.
        /// </summary>
        /// <value>The details that describe the audit trail for this view model.</value>
        public AuditDetailViewModel AuditDetails { get; set; }
    }
}
