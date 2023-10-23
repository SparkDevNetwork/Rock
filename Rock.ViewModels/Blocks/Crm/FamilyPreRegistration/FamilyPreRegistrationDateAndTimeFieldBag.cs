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

namespace Rock.ViewModels.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The bag that contains all the field information for the Family Pre-Registration block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Blocks.Crm.FamilyPreRegistration.FamilyPreRegistrationFieldBag" />
    public class FamilyPreRegistrationDateAndTimeFieldBag : FamilyPreRegistrationFieldBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the date and time are shown.
        /// </summary>
        public bool IsDateAndTimeShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the date shown by itself.
        /// </summary>
        public bool IsDateShown { get; set; }
    }
}
