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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Used to define a sub-control (child control) configuration of the GroupMemberRequirementCard
    /// </summary>
    public class GroupMemberRequirementCardSubControlConfigBag
    {
        /// <summary>
        /// Whether or not this control is enabled
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// Label text for the control
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// Icon class for the label of the control
        /// </summary>
        public string Icon { get; set; } = "";

        /// <summary>
        /// Used by the front end to indicate the action for this control is currently running
        /// </summary>
        public bool IsLoading { get; set; } = false;
    }
}
