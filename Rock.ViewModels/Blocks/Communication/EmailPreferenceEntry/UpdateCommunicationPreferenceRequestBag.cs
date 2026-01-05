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

using Rock.Enums.Blocks.Communication.EmailPreferenceEntry;
using Rock.Enums.Communication;

namespace Rock.ViewModels.Blocks.Communication.EmailPreferenceEntry
{
    /// <summary>
    /// A bag that contains information about a request to update a person's communication preference for a communication
    /// channel for the Email Preference Entry block.
    /// </summary>
    public class UpdateCommunicationPreferenceRequestBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier key for this communication channel.
        /// </summary>
        /// <remarks>
        /// The type of entity can be inferred from the <see cref="CommunicationChannelType"/> property value.
        /// </remarks>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the type of communication channel subscription the person is managing.
        /// </summary>
        public CommunicationChannelType CommunicationChannelType { get; set; }

        /// <summary>
        /// Gets or sets the person's communication preference for this communication channel.
        /// </summary>
        public CommunicationType CommunicationPreference { get; set; }
    }
}
