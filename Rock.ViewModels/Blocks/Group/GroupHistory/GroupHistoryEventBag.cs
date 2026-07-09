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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Group.GroupHistory
{
    /// <summary>
    /// A single event on the Group History timeline, summarizing one set of
    /// related history records.
    /// </summary>
    public class GroupHistoryEventBag
    {
        /// <summary>
        /// Gets or sets the kind of change this event describes.
        /// </summary>
        public GroupHistoryEventType EventType { get; set; }

        /// <summary>
        /// Gets or sets the date and time the event occurred.
        /// </summary>
        public DateTimeOffset EventDateTime { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who made the change. Null when
        /// the person who made the change is not known.
        /// </summary>
        public string ActorName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person profile page for the person who
        /// made the change. Null when the person is not known.
        /// </summary>
        public string ActorProfileUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person who made the
        /// change is the person currently viewing the timeline.
        /// </summary>
        public bool IsActorCurrentPerson { get; set; }

        /// <summary>
        /// Gets or sets the display text of the thing the event acted on, such
        /// as the group name for a group-created event.
        /// </summary>
        public string TargetText { get; set; }

        /// <summary>
        /// Gets or sets the URL the target text should link to. Null when no
        /// link is available.
        /// </summary>
        public string TargetUrl { get; set; }

        /// <summary>
        /// Gets or sets the caption describing this event. Used when the event
        /// type is Other and no structured phrasing is available.
        /// </summary>
        public string CaptionText { get; set; }

        /// <summary>
        /// Gets or sets the individual value changes that make up this event.
        /// </summary>
        public List<GroupHistoryChangeBag> Changes { get; set; }

        /// <summary>
        /// Gets or sets the people this event acted on, such as the members
        /// that were added or removed.
        /// </summary>
        public List<GroupHistoryPersonBag> Persons { get; set; }
    }
}
