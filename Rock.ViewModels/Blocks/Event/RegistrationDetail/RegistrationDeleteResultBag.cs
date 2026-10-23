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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The result of deleting a registration.
    /// </summary>
    public class RegistrationDeleteResultBag
    {
        /// <summary>
        /// Gets or sets the URL to navigate to now that the registration is deleted.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets a warning raised while deactivating the payment plan, or null
        /// when there was none.
        /// </summary>
        public string WarningMessage { get; set; }
    }
}
