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

namespace Rock.Enums.Event
{
    /// <summary>
    /// Determines how campus usage behaves for the interactive experience.
    /// </summary>
    public enum InteractiveExperienceCampusBehavior
    {
        /// <summary>
        /// The individual's geolocation is used to determine which campus
        /// geofence they are within. This will be used to filter the
        /// schedules and determine which campus to write to the Interaction.
        /// If a schedule has a campus without a geofence defined then it
        /// will always be excluded from the list of occurrences.
        /// </summary>
        FilterSchedulesByCampusGeofences,

        /// <summary>
        /// The individual's geolocation is only used to determine the campus
        /// to be written on the Interaction. If no geolocation is available or
        /// it does not match any campus geofence then null (or the default
        /// experience campus) will be written on the Interaction.
        /// </summary>
        DetermineCampusFromGeofence,

        /// <summary>
        /// The individual's campus associated with their record will be used
        /// only for determining which campus to write to the Interaction. If
        /// no campus is associated with the Person record then the default
        /// experience campus will be used.
        /// </summary>
        UseIndividualsCampus
    }
}
