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

using Rock.Attribute;

namespace Rock.Workflow.FormBuilder
{
    /// <summary>
    /// Runtime mirror of <see cref="Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormConnectionRequestsViewModel"/>.
    /// Configures whether and how a Connection Request is opened when a Form
    /// Builder form is submitted. Persisted as part of the
    /// <see cref="FormSettings"/> blob in
    /// <see cref="Rock.Model.WorkflowType.FormBuilderSettingsJson"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.20" )]
    public class FormConnectionRequestsSettings
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
        /// Per-form-field mappings.
        /// </summary>
        public List<FormFieldAttributeMappingSettings> AttributeMappings { get; set; } = new List<FormFieldAttributeMappingSettings>();
    }
}
