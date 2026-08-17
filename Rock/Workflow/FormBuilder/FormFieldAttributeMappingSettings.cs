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

using Rock.Attribute;

namespace Rock.Workflow.FormBuilder
{
    /// <summary>
    /// Runtime mirror of <see cref="Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormFieldAttributeMappingViewModel"/>.
    /// Persisted as part of the Form Builder settings blob, so it does not
    /// require its own database column.
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
    public class FormFieldAttributeMappingSettings
    {
        /// <summary>
        /// The unique identifier of the form attribute (workflow form field)
        /// being mapped.
        /// </summary>
        public Guid FormAttributeGuid { get; set; }

        /// <summary>
        /// The target attribute on the Connection Opportunity. <c>null</c>
        /// means the form field's value is appended to the Connection
        /// Request's Comment field as <c>"{Label}: {Value}"</c>.
        /// </summary>
        public Guid? TargetAttributeGuid { get; set; }
    }
}
