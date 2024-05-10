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

namespace Rock.ViewModels.Blocks.Security.UserLoginList
{
    /// <summary>
    /// Contains details on the Authentication Component
    /// </summary>
    public class AuthenticationComponentBag
    {
        /// <summary>
        /// Gets a value indicating whether [supports change password].
        /// </summary>
        /// <value>
        /// <c>true</c> if [supports change password]; otherwise, <c>false</c>.
        /// </value>
        public bool SupportsChangePassword { get; set; }

        /// <summary>
        /// Gets a value indicating whether the Rock UI should prompt for a password
        /// </summary>
        /// <value>
        ///   <c>true</c> if [prompt for password]; otherwise, <c>false</c>.
        /// </value>
        public bool PromptForPassword { get; set; }
    }
}
