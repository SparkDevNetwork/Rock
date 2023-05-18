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
using Rock.Enums.Blocks.Security.AccountEntry;

namespace Rock.ViewModels.Blocks.Security.AccountEntry
{
    /// <summary>
    /// A bag that contains the register response information.
    /// </summary>
    public class AccountEntryRegisterResponseBox
    {
        /// <summary>
        /// The account entry step.
        /// </summary>
        public AccountEntryStep? Step { get; set; }

        /// <summary>
        /// Duplicate person selection step bag.
        /// </summary>
        /// <remarks>
        /// This will be set when <see cref="Step"/> == <see cref="AccountEntryStep.DuplicatePersonSelection"/>.
        /// </remarks>
        public AccountEntryDuplicatePersonSelectionStepBag DuplicatePersonSelectionStepBag { get; set; }

        /// <summary>
        /// Existing account step bag.
        /// </summary>
        /// <remarks>
        /// This will be set when <see cref="Step"/> == <see cref="AccountEntryStep.ExistingAccount"/>.
        /// </remarks>
        public AccountEntryExistingAccountStepBag ExistingAccountStepBag { get; set; }

        /// <summary>
        /// Confirmation sent step bag.
        /// </summary>
        /// <remarks>
        /// This will be set when <see cref="Step"/> == <see cref="AccountEntryStep.ConfirmationSent"/>.
        /// </remarks>
        public AccountEntryConfirmationSentStepBag ConfirmationSentStepBag { get; set; }

        /// <summary>
        /// Completed step bag.
        /// </summary>
        /// <remarks>
        /// This will be set when <see cref="Step"/> == <see cref="AccountEntryStep.Completed"/>.
        /// </remarks>
        public AccountEntryCompletedStepBag CompletedStepBag { get; set; }

        /// <summary>
        /// Passwordless confirmation sent step bag.
        /// </summary>
        /// <remarks>
        /// This will be set when <see cref="Step"/> == <see cref="AccountEntryStep.PasswordlessConfirmationSent"/>.
        /// </remarks>
        public AccountEntryPasswordlessConfirmationSentStepBag PasswordlessConfirmationSentStepBag { get; set; }
    }
}
