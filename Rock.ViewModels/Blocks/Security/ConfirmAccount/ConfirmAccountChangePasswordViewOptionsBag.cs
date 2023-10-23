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
    /// A bag containing the required information to display the Confirm Account Block's change password view.
    /// </summary>
    public class ConfirmAccountChangePasswordViewOptionsBag
    {
        /// <summary>
        /// The encrypted confirmation code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The error caption.
        /// </summary>
        public string ErrorCaption { get; set; }

        /// <summary>
        /// The change password view caption.
        /// </summary>
        public string ViewCaption { get; set; }
    }
}
