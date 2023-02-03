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
    /// A bag that contains the required information to render an account entry block's passwordless confirmation sent step.
    /// </summary>
    public class AccountEntryPasswordlessConfirmationSentStepBag
    {
        /// <summary>
        /// Gets or sets the duplicate person selection step caption.
        /// </summary>
        public string Caption { get; set; }

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
    }
}
