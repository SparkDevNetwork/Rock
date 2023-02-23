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

using Rock.Attribute;

namespace Rock.Workflow.FormBuilder
{
    /// <summary>
    /// The possible destination options for a <see cref="FormConfirmationEmailSettings"/>.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.13.2" )]
    public enum FormConfirmationEmailDestination
    {
        /// <summary>
        /// A custom attribute will be used as the destination of the confirmation
        /// email. It will be contained in <see cref="FormConfirmationEmailSettings.RecipientAttributeGuid"/>.
        /// </summary>
        Custom = 0,

        /// <summary>
        /// The standard Person attribute will be used. It can be found on the workflow
        /// by searching for the attribute whose key value is <see cref="Rock.SystemKey.WorkflowFormBuilderKey.Person"/>.
        /// </summary>
        Person = 1,

        /// <summary>
        /// The standard Person attribute will be used. It can be found on the workflow
        /// by searching for the attribute whose key value is <see cref="Rock.SystemKey.WorkflowFormBuilderKey.Person"/>.
        /// </summary>
        Spouse = 2
    }
}
