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

using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Communication.CommunicationFlowDetail
{
    /// <summary>
    /// The initialization details for the Communication Flow Detail block.
    /// </summary>
    public class CommunicationFlowDetailInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the Communication Flow details.
        /// </summary>
        public CommunicationFlowBag Entity { get; set; }

        /// <summary>
        /// Gets or sets the list of SMS From System phone numbers.
        /// </summary>
        public List<ListItemBag> SmsFromSystemPhoneNumbers { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the push medium is configured to use the Rock Mobile Push transport.
        /// </summary>
        public bool IsRockMobilePushTransportConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the visual step indicator at the top of the block should be hidden to simplify the interface.
        /// </summary>
        /// <remarks>
        /// When enabled, this hides the visual step indicator displayed at the top of the Communication Flow editor. This element helps individuals understand their current place in the process of building a communication campaign but is not interactive. Disabling it can create a cleaner, less distracting layout.
        /// </remarks>
        public bool IsStepIndicatorHidden { get; set; }

        /// <summary>
        /// Gets or sets the push notification mobile applications that can be selected.
        /// </summary>
        public List<ListItemBag> PushMobileApplications { get; set; }

        /// <summary>
        /// Gets or sets the list of communication templates that can be selected.
        /// </summary>
        public List<CommunicationFlowDetailCommunicationTemplateBag> CommunicationTemplates { get; set; }

        /// <summary>
        /// Gets or sets the test email address used for sending test email communications.
        /// </summary>
        public string TestEmailAddress { get; set; }

        /// <summary>
        /// Gets or sets the SMS phone number for sending test SMS communications.
        /// </summary>
        public string TestSmsPhoneNumber { get; set; }
    }
}
