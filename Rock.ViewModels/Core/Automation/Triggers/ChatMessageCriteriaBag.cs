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

namespace Rock.ViewModels.Core.Automation.Triggers
{
    /// <summary>
    /// The criteria used to determine if a chat message should trigger.
    /// </summary>
    public class ChatMessageCriteriaBag
    {
        /// <summary>
        /// Gets or sets whether all rules must match or if any one matching rule is sufficient to trigger the events.
        /// </summary>
        public bool AreAllRulesRequired { get; set; }

        /// <summary>
        /// Gets or sets the list of rules that will be used to determine if the chat message matches this criteria.
        /// </summary>
        public List<ChatMessageCriteriaRuleBag> Rules { get; set; }
    }
}
