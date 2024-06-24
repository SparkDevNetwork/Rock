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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// Details about how a kiosk was configured. This is saved in the local
    /// storage in the browser and used to retrieve a full KioskConfigurationBag.
    /// </summary>
    public class SavedKioskConfigurationBag
    {
        /// <summary>
        /// The campus unique identifier that was selected when the kiosk was
        /// configured. This may be null.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// The check-in configuration template unique identifier.
        /// </summary>
        public Guid TemplateGuid { get; set; }

        /// <summary>
        /// The device unique identifier the kiosk was configured to use.
        /// </summary>
        public Guid KioskGuid { get; set; }

        /// <summary>
        /// The list of check-in area unique identifiers the kiosk was
        /// configured to use.
        /// </summary>
        public List<Guid> AreaGuids { get; set; }
    }
}
