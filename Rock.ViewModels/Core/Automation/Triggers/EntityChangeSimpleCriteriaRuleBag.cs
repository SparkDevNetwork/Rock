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
    /// A single rule that is used to determine if an entity change should
    /// trigger the events. This is used when the component is in simple
    /// filtering mode.
    /// </summary>
    public class EntityChangeSimpleCriteriaRuleBag
    {
        /// <summary>
        /// A unique identifier for this rule. This is used to identify the
        /// rule in the UI and not used for any other purpose.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The name of the property that is being monitored for changes.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// The type of change that will trigger the criteria to match.
        /// </summary>
        public EntityChangeSimpleChangeType ChangeType { get; set; }

        /// <summary>
        /// The original value of the property must be equal to in
        /// order to match the rule. This is only used for certain change
        /// types.
        /// </summary>
        public string OriginalValue { get; set; }

        /// <summary>
        /// The current value of the property must be equal to in
        /// order to match the rule. This is only used for certain change
        /// types.
        /// </summary>
        public string UpdatedValue { get; set; }
    }
}
