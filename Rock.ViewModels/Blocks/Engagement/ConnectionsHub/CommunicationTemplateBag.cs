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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents a container for communication template data.
    /// </summary>
    public class CommunicationTemplateBag
    {
        /// <summary>
        /// Gets or sets the unique identifier key for the communication template.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets the communication template name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the email address that is used as the sender in outgoing email messages.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets the name that is used as the sender in outgoing messages.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets the subject to use for outgoing email messages.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets the template email message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets the collection of binary files to be included as attachments in the email message.
        /// </summary>
        public List<ListItemBag> EmailAttachmentBinaryFiles { get; set; }
    }
}
