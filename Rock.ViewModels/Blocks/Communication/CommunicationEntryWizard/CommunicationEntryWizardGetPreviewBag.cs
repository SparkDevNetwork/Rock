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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Bag containing information to get a message preview for the Communication Entry Wizard block.
    /// </summary>
    public class CommunicationEntryWizardGetPreviewBag
    {
        /// <summary>
        /// Gets or sets the communication.
        /// </summary>
        public CommunicationEntryWizardCommunicationBag Communication { get ;set; }

        /// <summary>
        /// Gets or sets the message preview.
        /// </summary>
        public string MessagePreview { get; set; }

        /// <summary>
        /// Gets or sets the "preview as" person alias.
        /// </summary>
        public ListItemBag PreviewAsPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the requested person was found.
        /// </summary>
        public bool? WasRequestedPersonFound { get; set; }
    }
}
