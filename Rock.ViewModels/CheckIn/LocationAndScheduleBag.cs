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
    /// Defines a location and schedule pair. Used to indicate which locations
    /// are valid for a group since a location might only be valid during one
    /// schedule.
    /// </summary>
    public class LocationAndScheduleBag
    {
        /// <summary>
        /// The identifier of the location.
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// The identifier of the schedule.
        /// </summary>
        public string ScheduleId { get; set; }
    }
}
