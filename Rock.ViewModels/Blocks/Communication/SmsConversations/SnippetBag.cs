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

namespace Rock.ViewModels.Blocks.Communication.SmsConversations
{
    /// <summary>
    /// The Snippet Bag for SMS Conversations
    /// </summary>
    public class SnippetBag
    {
        /// <summary>
        /// Gets or sets the snippet data.
        /// </summary>
        public ListItemBag Snippet { get; set; }

        /// <summary>
        /// Gets or sets the list of category GUIDs associated with the snippet.
        /// </summary>
        public List<Guid> Categories { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the snippet (e.g., "Personal" or "Shared").
        /// </summary>
        public string SnippetVisibility { get; set; }

    }
}
