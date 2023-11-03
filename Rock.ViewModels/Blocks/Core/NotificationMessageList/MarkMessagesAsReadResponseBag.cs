﻿// <copyright>
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

namespace Rock.ViewModels.Blocks.Core.NotificationMessageList
{
    /// <summary>
    /// Describes the response data sent back from the MarkMessagesAsRead
    /// block action.
    /// </summary>
    public class MarkMessagesAsReadResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the message specified by
        /// the key was deleted.
        /// </summary>
        /// <value>A dictionary of IdKey keys and boolean values to indicate if the message was deleted.</value>
        public Dictionary<string, bool> IsDeleted { get; set; }
    }
}
