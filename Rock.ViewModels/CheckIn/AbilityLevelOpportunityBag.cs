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
namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// Defines a single ability level that can be used during check-in.
    /// </summary>
    public class AbilityLevelOpportunityBag : CheckInItemBag
    {
        /// <summary>
        /// Determines if this ability level is unavailable for selection during
        /// a self-serve check-in. This may be overridden when assisted by a
        /// staff member. If every ability level is disabled then the entire
        /// screen should be skipped.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Determines if this ability level should be considered of lower
        /// priority than the others. This is used to indicate that the selected
        /// ability level is currently higher than this item and it should be
        /// displayed in a way to make it clear that this wouldn't normally be
        /// selected.
        /// </summary>
        public bool IsDeprioritized { get; set; }
    }
}
