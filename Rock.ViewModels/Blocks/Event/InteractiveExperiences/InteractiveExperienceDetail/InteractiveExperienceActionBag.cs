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

namespace Rock.ViewModels.Blocks.Event.InteractiveExperiences.InteractiveExperienceDetail
{
    /// <summary>
    /// Identifies a single action configured for use with an interactive experience.
    /// </summary>
    public class InteractiveExperienceActionBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of this action instance.
        /// </summary>
        /// <value>The unique identifier of this action instance.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the title of this action.
        /// </summary>
        /// <value>The title of this action.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the type of action.
        /// </summary>
        /// <value>The type of action.</value>
        public ListItemBag ActionType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether moderation is required
        /// for this action.
        /// </summary>
        /// <value><c>true</c> if moderation is required; otherwise, <c>false</c>.</value>
        public bool IsModerationRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multiple submissions are
        /// allowed for this action.
        /// </summary>
        /// <value><c>true</c> if multiple submissions are allowed; otherwise, <c>false</c>.</value>
        public bool IsMultipleSubmissionsAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this action should record
        /// responses anonymously.
        /// </summary>
        /// <value><c>true</c> if this action should record responses anonymously; otherwise, <c>false</c>.</value>
        public bool IsResponseAnonymous { get; set; }

        /// <summary>
        /// Gets or sets the response visualizer used to display responses.
        /// </summary>
        /// <value>The response visualizer used to display responses.</value>
        public ListItemBag ResponseVisualizer { get; set; }

        /// <summary>
        /// Gets or sets the attribute values.
        /// </summary>
        /// <value>
        /// The attribute values.
        /// </value>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
