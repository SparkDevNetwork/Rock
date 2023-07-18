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

using Rock.Enums.Blocks.Security.ForgotUserName;

namespace Rock.ViewModels.Blocks.Security.ForgotUserName
{
    /// <summary>
    /// A bag containing the results of sending instructions.
    /// </summary>
    public class ForgotUserNameSendInstructionsResultBag
    {
        /// <summary>
        /// The result type.
        /// </summary>
        public SendInstructionsResultType ResultType { get; set; }

        /// <summary>
        /// The bag containing the required information to display a "Change Password Not Supported" message.
        /// <para>Only present if <see cref="ResultType"/> == <see cref="SendInstructionsResultType.ChangePasswordNotSupported" />.</para>
        /// </summary>
        public ChangePasswordNotSupportedResultBag ChangePasswordNotSupportedResult { get; set; }
    }
}
