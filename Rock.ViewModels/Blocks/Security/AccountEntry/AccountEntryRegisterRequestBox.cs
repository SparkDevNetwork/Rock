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
namespace Rock.ViewModels.Blocks.Security.AccountEntry
{
    /// <summary>
    /// A bag that contains the register request information.
    /// </summary>
    public class AccountEntryRegisterRequestBox
    {
        /// <summary>
        /// Gets or sets the account info.
        /// </summary>
        public AccountEntryAccountInfoBag AccountInfo { get; set; }

        /// <summary>
        /// Gets or sets the person info.
        /// </summary>
        public AccountEntryPersonInfoBag PersonInfo { get; set; }

        /// <summary>
        /// Gets or sets the encrypted state.
        /// </summary>
        /// <remarks>
        /// This was initially implemented for passwordless authentication
        /// when an individual enters an email/phone number that is not
        /// tied to an existing Person. In that case, we need to forward some
        /// passwordless session information to the registration page so that
        /// the passwordless session can be finalized after the individual
        /// completes registration.
        /// </remarks>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the selected Person identifier.
        /// </summary>
        public int? SelectedPersonId { get; set; }

        /// <summary>
        /// Gets or sets the passwordless verification code.
        /// </summary>
        public string Code { get; set; }
    }
}
