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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Communication.CommunicationDetail
{
    /// <summary>
    /// The box that contains all the initialization information for the communication detail block.
    /// </summary>
    public class CommunicationDetailInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets whether the communication detail block should be hidden.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets the detail of this communication.
        /// </summary>
        public CommunicationDetailBag CommunicationDetail { get; set; }

        /// <summary>
        /// Gets or sets the message preview for this communication.
        /// </summary>
        public CommunicationMessagePreviewBag MessagePreview { get; set; }

        /// <summary>
        /// Gets or sets the recipient grid definition for this communication.
        /// </summary>
        public GridDefinitionBag RecipientGridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the recipient grid configuration options for this communication.
        /// </summary>
        public CommunicationRecipientGridOptionsBag RecipientGridOptions { get; set; }

        /// <summary>
        /// Gets or sets the permissions for this communication detail block.
        /// </summary>
        public CommunicationDetailPermissionsBag Permissions { get; set; }
    }
}
