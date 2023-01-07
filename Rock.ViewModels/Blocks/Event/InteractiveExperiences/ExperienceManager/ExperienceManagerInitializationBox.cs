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

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.ExperienceManager
{
    /// <summary>
    /// Class ExperienceManagerInitializationBox.
    /// Implements the <see cref="Rock.ViewModels.Blocks.BlockBox" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.BlockBox" />
    public class ExperienceManagerInitializationBox : BlockBox
    {
        /// <summary>
        /// Gets or sets the experience token used to authenticate to the RealTime topic.
        /// </summary>
        /// <value>The experience token used to authenticate to the RealTime topic.</value>
        public string ExperienceToken { get; set; }

        /// <summary>
        /// Gets or sets the occurrence identifier key.
        /// </summary>
        /// <value>The occurrence identifier key.</value>
        public string OccurrenceIdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the experience.
        /// </summary>
        /// <value>The name of the experience.</value>
        public string ExperienceName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the notification
        /// toggle option to the individual.
        /// </summary>
        /// <value><c>true</c> if the notification toggle button should be visible; otherwise, <c>false</c>.</value>
        public bool IsNotificationAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this experience is inactive.
        /// </summary>
        /// <value><c>true</c> if this experience is inactive; otherwise, <c>false</c>.</value>
        public bool IsExperienceInactive { get; set; }

        /// <summary>
        /// Gets or sets the number of active participants in the experience.
        /// </summary>
        /// <value>The number of active participants in the experience.</value>
        public int ParticipantCount { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds between participant count updates.
        /// </summary>
        /// <value>The number of seconds between participant count updates.</value>
        public int ParticipantCountUpdateInterval { get; set; }

        /// <summary>
        /// Gets or sets the actions configured for this experience.
        /// </summary>
        /// <value>The actions configured for this experience.</value>
        public List<ListItemBag> Actions { get; set; }

        /// <summary>
        /// Gets or sets the tabs that should be shown in the manager.
        /// </summary>
        /// <value>The tabs that should be shown in the manager.</value>
        public List<string> TabsToShow { get; set; }
    }
}
