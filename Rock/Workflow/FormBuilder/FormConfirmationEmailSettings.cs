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
    /// Contains details about a confirmation e-mail for a Form Builder form.
    /// This specifies if one should be sent, who receives it and the content
    /// it will contain.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public class FormConfirmationEmailSettings
    {
        /// <summary>
        /// Specifies if the confirmation e-mail has been enabled and should be
        /// sent.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Specifies which workflow attribute will be used to determine the
        /// recipient of the confirmation e-mail.
        /// </summary>
        /// <remarks>
        ///     <para>The attribute must be a field type of Person or Email.</para>
        ///     <para>
        ///         This property is a Guid instead of integer so that the
        ///         attribute can be referenced before it has been saved. The form
        ///         must be saved first, including this JSON data, before the
        ///         attribute can be created. So we don't always have access to
        ///         the integer identifier.
        ///     </para>
        /// </remarks>
        public Guid? RecipientAttributeGuid { get; set; }

        /// <summary>
        /// Determines how the content of the e-mail will be generated.
        /// </summary>
        public FormEmailSourceSettings Source { get; set; }
    }
}
