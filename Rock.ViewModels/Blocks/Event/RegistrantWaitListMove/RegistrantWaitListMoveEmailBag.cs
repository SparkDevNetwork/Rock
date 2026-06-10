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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.ViewModels.Blocks.Event.RegistrantWaitListMove
{
    /// <summary>
    /// Contains the email template fields used when sending wait list move
    /// confirmation emails. Values are pre-populated from the registration
    /// template and resolved against the first registration's Lava merge fields.
    /// </summary>
    public class RegistrantWaitListMoveEmailBag
    {
        /// <summary>
        /// The display name shown in the From field of the outgoing email.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// The email address shown in the From field of the outgoing email.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// The subject line of the outgoing email.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// The raw (unresolved) Lava email body. Sent to the server as-is
        /// when the user submits; merge fields are resolved per-recipient at send time.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// The email body resolved against the first registration's merge fields,
        /// used to render the preview iframe on initial load.
        /// </summary>
        public string PreviewHtml { get; set; }

        /// <summary>
        /// The ids of the registrations whose registrars should receive the email.
        /// Populated from the recipient selection made by the user on the client.
        /// </summary>
        public List<int> RegistrationIds { get; set; }
    }
}
