﻿// <copyright>
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

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Bag containing the save communication template response information in the Communication Entry Wizard block.
    /// </summary>
    public class CommunicationEntryWizardSaveCommunicationTemplateResponseBag
    {
        /// <summary>
        /// Gets or sets the communication template detail information.
        /// </summary>
        public CommunicationEntryWizardCommunicationTemplateDetailBag CommunicationTemplateDetail { get; set; }
        
        /// <summary>
        /// Gets or sets the communication template list item information.
        /// </summary>
        public CommunicationEntryWizardCommunicationTemplateListItemBag CommunicationTemplateListItem { get; set; }
    }
}