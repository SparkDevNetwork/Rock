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
    /// Runtime mirror of <see cref="Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormConfirmationEmailRecipientType"/>.
    /// Identifies whether a Form Builder confirmation e-mail goes to the resolved
    /// primary person, the primary person's spouse, or both.
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
    public enum FormConfirmationEmailRecipientType
    {
        /// <summary>
        /// Send only to the resolved primary person (today's default).
        /// </summary>
        Person = 0,

        /// <summary>
        /// Send only to the resolved primary person's spouse. Skip-and-warn
        /// if no spouse is on the family.
        /// </summary>
        Spouse = 1,

        /// <summary>
        /// Send to the resolved primary person AND to the primary person's
        /// spouse (one delivery each). If no spouse is present, only the
        /// primary person is sent to.
        /// </summary>
        Both = 2
    }
}
