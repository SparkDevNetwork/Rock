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

using Rock.Model;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Provides the send email response.
    /// </summary>
    public class EmailSendResponse
    {
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public CommunicationRecipientStatus Status { get; set; }
        /// <summary>
        /// Gets or sets the status note.
        /// </summary>
        /// <value>
        /// The status note.
        /// </value>
        public string StatusNote { get; set; }
    }
}
