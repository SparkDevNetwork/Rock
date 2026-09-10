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

namespace Rock.Enums.CheckIn
{
    /// <summary>
    /// Determines when the "skip" screen is shown during check-in. The skip
    /// screen is the area selection screen displayed when there is no valid
    /// opportunity for the current attendee (for example, a room exists but is
    /// full, or no room applies at all).
    /// </summary>
    public enum SkipScreenBehavior
    {
        /// <summary>
        /// Show the skip screen only when it could still matter, such as when a
        /// valid room exists for the attendee but is currently unavailable. When
        /// the attendee could never have checked in, the skip screen is bypassed
        /// and the attendee is quietly skipped.
        /// </summary>
        ShowWhenNeeded = 0,

        /// <summary>
        /// Never show the skip screen. Attendees who have no valid opportunity
        /// are quietly skipped without any prompt. This matches the legacy
        /// behavior where a skipped attendee was not discovered until after
        /// other family members had already been checked in.
        /// </summary>
        NeverShow = 1,

        /// <summary>
        /// Always show the skip screen when there is no valid opportunity,
        /// letting the operator confirm the skip for every attendee.
        /// </summary>
        AlwaysShow = 2
    }
}
