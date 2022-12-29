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

namespace Rock.Event.InteractiveExperiences
{
    /// <summary>
    /// A token that identifies information about an experience occurrence that
    /// a user is going to join. This must be encrypted before being sent to
    /// the client.
    /// </summary>
    internal class ExperienceToken
    {
        /// <summary>
        /// Gets or sets the occurrence identifier the client is allowed to join.
        /// </summary>
        /// <value>The occurrence identifier the lcient is allowed to join.</value>
        public string OccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the interaction unique identifier to use when recording
        /// participation and online status. If this is null then participation
        /// will not be recorded.
        /// </summary>
        /// <value>The interaction unique identifier.</value>
        public Guid? InteractionGuid { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier that this token will be associated
        /// with. This is used to imply the campus the individual is currently at
        /// as opposed to what campus they normally attend.
        /// </summary>
        /// <value>The identifier of the campus physically present at.</value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the personal device identifier. This is used to know which
        /// push token to use to notify the devices.
        /// </summary>
        /// <value>The personal device identifier.</value>
        public int? PersonalDeviceId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this client is a moderator.
        /// </summary>
        /// <value><c>true</c> if this client is a moderator; otherwise, <c>false</c>.</value>
        public bool IsModerator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this client is a visualizer.
        /// </summary>
        /// <value><c>true</c> if this client is a visualizer; otherwise, <c>false</c>.</value>
        public bool IsVisualizer { get; set; }
    }
}
