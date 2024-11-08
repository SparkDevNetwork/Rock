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

using Rock.ViewModels.Controls;

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// A registration from the check-in kiosk for a family.
    /// </summary>
    public class RegistrationFamilyBag
    {
        /// <summary>
        /// The encrypted identifier of the family. When saving, this value
        /// should be an empty string if this is a new family to be created.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The name of the primary family that the adults will be members of.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// The main address of the family. Currently this is the home address
        /// but in the future that may be updated to allow the configuration of
        /// the address type.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Attribute values on the family to be displayed or edited.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
