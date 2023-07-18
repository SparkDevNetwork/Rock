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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Security.ForgotUserName
{
    /// <summary>
    /// A bag containing the required information to display a "Change Password Not Supported" message.
    /// </summary>
    public class ChangePasswordNotSupportedResultBag
    {
        /// <summary>
        /// All the account types associated with the email.
        /// </summary>
        public List<string> AccountTypes { get; set; }

        /// <summary>
        /// The URL to redirect to to create a new account.
        /// </summary>
        public string NewAccountUrl { get; set; }
    }
}
