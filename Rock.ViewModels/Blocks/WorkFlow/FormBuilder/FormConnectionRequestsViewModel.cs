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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Editor settings for the Automations tab's Connection Requests section.
    /// When enabled, the form's submission creates a Connection Request using
    /// the Person Entry primary person as the requestor.
    /// </summary>
    public class FormConnectionRequestsViewModel
    {
        /// <summary>
        /// Whether the Connection Requests section is enabled for this form.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The selected Connection Type.
        /// </summary>
        public Guid? ConnectionTypeGuid { get; set; }

        /// <summary>
        /// The selected Connection Opportunity. Required at runtime when
        /// <see cref="Enabled"/> is true.
        /// </summary>
        public Guid? ConnectionOpportunityGuid { get; set; }

        /// <summary>
        /// Optional explicit Connection Status. <c>null</c> falls back to the
        /// type's default status at runtime.
        /// </summary>
        public Guid? ConnectionStatusGuid { get; set; }

        /// <summary>
        /// Optional Connection Source defined-value. <c>null</c> at runtime
        /// means no source on the new Connection Request.
        /// </summary>
        public Guid? ConnectionSourceValueGuid { get; set; }

        /// <summary>
        /// Per-form-field mappings. Order matches the form's attribute order.
        /// </summary>
        public List<FormFieldAttributeMappingViewModel> AttributeMappings { get; set; } = new List<FormFieldAttributeMappingViewModel>();
    }
}
