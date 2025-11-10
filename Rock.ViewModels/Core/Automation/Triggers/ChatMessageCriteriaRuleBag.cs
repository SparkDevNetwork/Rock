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

using Rock.Enums.Core.Automation.Triggers;

namespace Rock.ViewModels.Core.Automation.Triggers
{
    /// <summary>
    /// A single rule that is used to determine if a chat message should trigger the events.
    /// </summary>
    public class ChatMessageCriteriaRuleBag
    {
        /// <summary>
        /// Gets or sets a unique identifier for this rule. This is used to identify the rule in the UI and not used for
        /// any other purpose.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the type of rule to use to determine if a chat message should trigger the events.
        /// </summary>
        public ChatMessageCriteriaRuleType RuleType { get; set; }

        /// <summary>
        /// Gets or sets the criteria value to use to determine if a chat message should trigger the events.
        /// </summary>
        public string CriteriaValue { get; set; }

        /// <summary>
        /// Gets or sets the friendly criteria value to display to the individual when administering this rule in the UI.
        /// </summary>
        public string FriendlyCriteriaValue { get; set; }
    }
}
