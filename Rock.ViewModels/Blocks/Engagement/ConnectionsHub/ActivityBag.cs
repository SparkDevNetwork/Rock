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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionsHub
{
    /// <summary>
    /// Represents the data required to log an activity against one or more connection requests.
    /// </summary>
    public class ActivityBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier keys of the connection requests to log the activity against.
        /// </summary>
        public List<string> ConnectionRequestIdKeys { get; set; }

        //public string ConnectionTypeIdKey { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the activity type to log.
        /// </summary>
        public string ActivityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the GUID of the person alias to assign as the connector for this activity.
        /// </summary>
        public string ConnectorPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the note text to associate with this activity.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a person note should also be created when saving this activity.
        /// </summary>
        public bool AddPersonNote { get; set; }

        /// <summary>
        /// If set, the activity with this ID key will be updated instead of creating a new one.
        /// </summary>
        public string ActivityIdKey { get; set; }
    }
}
