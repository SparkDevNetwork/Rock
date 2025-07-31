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

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// A bag that contains information about the outcome of a request to perform an action on a communication within
    /// the communication detail block.
    /// </summary>
    public class CommunicationActionResponseBag
    {
        /// <summary>
        /// Gets or sets a message to display to the individual about the outcome of the action.
        /// </summary>
        public string OutcomeMessage { get; set; }

        /// <summary>
        /// Gets or sets the optional URL to which the client should be redirected.
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}
