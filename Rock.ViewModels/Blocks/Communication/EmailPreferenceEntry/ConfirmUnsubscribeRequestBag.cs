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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Communication.EmailPreferenceEntry
{
    /// <summary>
    /// A bag that contains information about a request to confirm a pending unsubscribe for the Email Preference
    /// Entry block.
    /// </summary>
    public class ConfirmUnsubscribeRequestBag
    {
        /// <summary>
        /// Gets or sets the level at which the person was offered to be unsubscribed. This must match the level the
        /// server determines for the person, or the request will be rejected.
        /// </summary>
        public UnsubscribeLevel UnsubscribeLevel { get; set; }
    }
}
