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

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Bag containing the response information after sending a communication via the Communication Entry Wizard block.
    /// </summary>
    public class CommunicationEntryWizardSendResponseBag
    {
        /// <summary>
        /// Gets a value indicating whether the page has a detail block for viewing the communication details.
        /// </summary>
        /// <value>
        ///   <see langword="true"/> if the page has a detail block for viewing the communication details; otherwise, <see langword="false"/>.
        /// </value>
        public bool HasDetailBlockOnCurrentPage { get; set; }
    }
}
