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

using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInKiosk
{
    /// <summary>
    /// The options to provide to the SaveFamily block action.
    /// </summary>
    public class SaveFamilyOptionsBag
    {
        /// <summary>
        /// The encrypted identifier of the check-in template configuration.
        /// </summary>
        public string TemplateId { get; set; }

        /// <summary>
        /// The encrypted identifier of the kiosk performing the operation.
        /// </summary>
        public string KioskId { get; set; }

        /// <summary>
        /// The details of the family that should be updated.
        /// </summary>
        public ValidPropertiesBox<RegistrationFamilyBag> Family { get; set; }

        /// <summary>
        /// The details of the various people in the family that should be
        /// updated. This will contain both family members and those associated
        /// by a relationship.
        /// </summary>
        public List<ValidPropertiesBox<RegistrationPersonBag>> People { get; set; }

        /// <summary>
        /// The encrypted identifiers of the people to remove from the family.
        /// </summary>
        public List<string> RemovedPersonIds { get; set; }
    }
}
