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

using Rock.Enums.CheckIn;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInConfigurationSettings
{
    /// <summary>
    /// The check-in type flow settings for a check-in configuration. Controls how the check-in screens flow
    /// based on whether a family or an individual is checking in; a Family check-in type exposes additional
    /// flow-related settings.
    /// </summary>
    public class CheckInTypeFlowSettingsBag
    {
        /// <summary>
        /// Gets or sets the type of check-in experience to use. Family check-in allows more than one person
        /// in the family to be checked in at a time.
        /// </summary>
        public string CheckInType { get; set; }

        /// <summary>
        /// Gets or sets which prior selections should be pre-selected for returning attendees during Family
        /// check-in (either people only, or people along with their area, group, and location).
        /// </summary>
        public AutoSelectMode? AutoSelectOptions { get; set; }

        /// <summary>
        /// Gets or sets the number of days back to look for a previous check-in for each person in the family
        /// (or related person). If they have previously checked in within this number of days, they will be
        /// automatically selected during Family check-in.
        /// </summary>
        public int? AutoSelectDaysBack { get; set; }

        /// <summary>
        /// Gets or sets whether, when family members are checking into more than one service, the same options
        /// that were selected for the first service should be automatically applied to additional services.
        /// </summary>
        public bool UseSameOptions { get; set; }

        /// <summary>
        /// Gets or sets whether people are prevented from checking into the same service time (schedule) more
        /// than once.
        /// </summary>
        public bool PreventDuplicateCheckin { get; set; }
    }
}
