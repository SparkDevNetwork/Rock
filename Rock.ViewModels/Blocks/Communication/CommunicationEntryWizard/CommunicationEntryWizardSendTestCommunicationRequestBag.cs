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
using System;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Bag containing the information to send a test communication in the Communication Entry Wizard block.
    /// </summary>
    public class CommunicationEntryWizardSendTestCommunicationRequestBag
    {
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the medium entity type unique identifier.
        /// </summary>
        /// <value>
        /// The medium entity type unique identifier.
        /// </value>
        public Guid MediumEntityTypeGuid { get; set; }
    }
}
