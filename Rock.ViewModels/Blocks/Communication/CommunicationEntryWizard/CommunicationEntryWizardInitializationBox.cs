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
using System.Collections.Generic;

using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard
{
    /// <summary>
    /// Box containing initialization information for the Communication Entry Wizard block.
    /// </summary>
    public class CommunicationEntryWizardInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the communication details being created/edited.
        /// </summary>
        /// <value>
        /// The communication details being created/edited.
        /// </value>
        public CommunicationEntryWizardCommunicationBag Communication { get; set; }

        /// <summary>
        /// Gets or sets the communication templates.
        /// </summary>
        public List<CommunicationEntryWizardCommunicationTemplateListItemBag> Templates { get; set; }

        /// <summary>
        /// Gets or sets whether the communication wizard block should be hidden.
        /// </summary>
        /// <value>
        /// <see langword="true"/> when the logged in person is not authorized or if the communication is not editable. 
        /// </value>
        public bool IsHidden { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ListItemBag> CommunicationListGroups { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<ListItemBag> SmsFromNumbers { get; set; }

        /// <summary>
        /// Gets or sets the communication mediums.
        /// </summary>
        /// <value>
        /// This only includes active mediums with active transports authorized for use by the current person.
        /// It is further limited by the "Communication Types" block setting.
        /// </value>
        public List<ListItemBag> Mediums { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDuplicatePreventionOptionShown { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> MergeFields { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<CommunicationEntryWizardRecipientBag> Recipients { get; set; }

        /// <summary>
        /// Gets or sets the file type to use for communication attachments.
        /// </summary>
        public Guid AttachmentBinaryFileTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the push medium is configured to use the Rock Mobile Push transport.
        /// </summary>
        public bool IsUsingRockMobilePushTransport { get; set; }

        /// <summary>
        /// Gets or sets the applications that support push notifications.
        /// </summary>
        public List<ListItemBag> PushApplications { get; set; }

        /// <summary>
        /// Gets or sets the initial communication template detail.
        /// </summary>
        public CommunicationEntryWizardCommunicationTemplateDetailBag CommunicationTemplateDetail { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether adding individuals to recipient lists is disabled.
        /// </summary>
        /// <value>
        /// When <see langword="true"/>, the person picker will be hidden so that additional individuals cannot be added to the recipient list.
        /// </value>
        public bool IsAddingIndividualsToRecipientListsDisabled { get; set; }

        /// <summary>
        /// Gets or sets the binary file type unique identifier to use for images that are added to the email using the image component.
        /// </summary>
        public Guid ImageComponentBinaryFileTypeGuid { get; set; }

        /// <summary>
        /// The maximum width (in pixels) of an image attached to a mobile communication.
        /// If its width is over the max, Rock will automatically resize image to the max width.
        /// </summary>
        public int MaxSmsImageWidth { get; set; }

        /// <summary>
        /// The recipient count threshold that, when exceeded, will automatically mark a new communication as bulk.
        /// </summary>
        public int? BulkEmailThreshold { get; set; }
        
        /// <summary>
        /// Gets or sets the SMS supported MIME types.
        /// </summary>
        public List<string> SmsSupportedMimeTypes { get; set; }
        
        /// <summary>
        /// Gets or sets the SMS accepted MIME types.
        /// </summary>
        public List<string> SmsAcceptedMimeTypes { get; set; }

        /// <summary>
        /// Gets or sets the SMS media size limit in bytes.
        /// </summary>
        public int SmsMediaSizeLimitBytes { get; set; }

        /// <summary>
        /// Gets or sets the video provider names.
        /// </summary>
        public List<string> VideoProviderNames { get; set; }

        /// <summary>
        /// Gets or sets a value determining whether this page has a communication detail block.
        /// </summary>
        public bool HasDetailBlockOnCurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the communication topic values.
        /// </summary>
        public List<ListItemBag> CommunicationTopicValues { get; set; }

        /// <summary>
        /// Gets or sets the personalization segments.
        /// </summary>
        public List<ListItemBag> PersonalizationSegments { get; set; }
    }
}
