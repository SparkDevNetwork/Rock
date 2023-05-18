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
namespace Rock.Enums.Blocks.Security.AccountEntry
{
    /// <summary>
    /// The step in the Account Entry block.
    /// </summary>
    public enum AccountEntryStep
    {
        /// <summary>
        /// The registration step.
        /// </summary>
        Registration = 0,

        /// <summary>
        /// The duplicate person selection step.
        /// </summary>
        DuplicatePersonSelection = 1,

        /// <summary>
        /// The existing account step.
        /// </summary>
        ExistingAccount = 2,

        /// <summary>
        /// The confirmation sent step.
        /// </summary>
        ConfirmationSent = 3,

        /// <summary>
        /// The completed step.
        /// </summary>
        Completed = 4,

        /// <summary>
        /// The passwordless confirmation sent step.
        /// </summary>
        /// <remarks>
        /// This occurs when a second confirmation is required to register a new account for an existing person
        /// whose email/phone wasn't used in the initial passwordless login request.
        /// </remarks>
        PasswordlessConfirmationSent = 5
    }
}
