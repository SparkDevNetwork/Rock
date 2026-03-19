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

using Rock.Enums.Connection;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents an activity type available for logging against connection requests, including its person note creation behavior.
    /// </summary>
    public class ConnectionActivityTypeBag
    {
        /// <summary>
        /// Gets or sets the activity type as a list item containing its value and display text.
        /// </summary>
        public ListItemBag ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the behavior that controls whether a person note is automatically created when this activity type is logged.
        /// </summary>
        public PersonNoteCreationBehavior? PersonNoteCreationBehavior { get; set; }
    }
}
