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

using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Blocks.Prayer.PrayerSession
{
    /// <summary>
    /// The details of a single prayer request displayed during a prayer session.
    /// </summary>
    public class PrayerSessionRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier of the prayer request.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the resolved HTML describing the person who made the request.
        /// </summary>
        public string PersonHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved HTML describing the prayer request details.
        /// </summary>
        public string PrayerHtml { get; set; }

        /// <summary>
        /// Gets or sets the campus name to display, or <c>null</c> when there is a
        /// single active campus and the campus label should be hidden.
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets the category name to display.
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the request is marked urgent.
        /// </summary>
        public bool IsUrgent { get; set; }

        /// <summary>
        /// Gets or sets the display text for the total number of team prayers.
        /// </summary>
        public string PrayerCountText { get; set; }

        /// <summary>
        /// Gets or sets the disclaimer to show when the request text may have been
        /// modified by an AI automation, or <c>null</c> when it should be hidden.
        /// </summary>
        public string AiDisclaimer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether comments are allowed on the request.
        /// </summary>
        public bool AllowComments { get; set; }

        /// <summary>
        /// Gets or sets the comments currently associated with the request.
        /// </summary>
        public List<NoteBag> Notes { get; set; }

        /// <summary>
        /// Gets or sets the note types available for commenting.
        /// </summary>
        public List<NoteTypeBag> NoteTypes { get; set; }
    }
}
