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

using System;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Bag containing the communication template list item information needed for the Communication Entry Wizard block.
    /// </summary>
    public class CommunicationEntryWizardCommunicationTemplateListItemBag
    {
        /// <summary>
        /// Gets or sets this template's unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this template supports Email communications.
        /// </summary>
        public bool? IsEmailSupported { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this template supports SMS communications.
        /// </summary>
        public bool? IsSmsSupported { get; set; }

        /// <summary>
        /// Gets or sets the category unique identifier for this template.
        /// </summary>
        public Guid? CategoryGuid { get; set; }
        
        /// <summary>
        /// Gets or sets this template's name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets this template's description.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets this template's image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a starter template.
        /// </summary>
        public bool IsStarter { get; set; }

        /// <summary>
        /// The count of communications in which this communication template has been used.
        /// </summary>
        public int CommunicationCount { get; set; }
    }
}