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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder
{
    /// <summary>
    /// Identifies which member of the resolved primary person's family will
    /// receive a form confirmation e-mail. Sits orthogonally to the existing
    /// recipient-attribute lookup that finds the primary person; this enum
    /// decides whether the e-mail also fans out to the spouse.
    /// </summary>
    public enum FormConfirmationEmailRecipientType
    {
        /// <summary>
        /// Send only to the resolved primary person. This preserves today's
        /// default behavior.
        /// </summary>
        Person = 0,

        /// <summary>
        /// Send only to the resolved primary person's spouse. If no spouse is
        /// present, the runtime warns and skips.
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
