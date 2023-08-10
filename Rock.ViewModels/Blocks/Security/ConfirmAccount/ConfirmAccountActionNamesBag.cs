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

namespace Rock.ViewModels.Blocks.Security.ConfirmAccount
{
    /// <summary>
    /// A bag containing the available action names for the Confirm Account block.
    /// </summary>
    public class ConfirmAccountActionNamesBag
    {
        /// <summary>
        /// The name of the action that changes an account password.
        /// </summary>
        public string ChangePassword { get; set; }

        /// <summary>
        /// The name of the action that confirms an account.
        /// </summary>
        public string ConfirmAccount { get; set; }

        /// <summary>
        /// The name of the action that deletes an account.
        /// </summary>
        public string DeleteAccount { get; set; }

        /// <summary>
        /// The name of the action that shows the change password view.
        /// </summary>
        public string ShowChangePasswordView { get; set; }

        /// <summary>
        /// The name of the action that shows the delete confirmation view.
        /// </summary>
        public string ShowDeleteConfirmationView { get; set; }
    }
}