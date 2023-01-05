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
using Rock.ViewModels.Controls;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the SaveNewValue API action of
    /// the DefinedValuePicker control for adding a new Defined Value
    /// </summary>
    public class LocationListSaveNewLocationOptionsBag
    {
        /// <summary>
        /// The GUID of the location that will be the new location's parent
        /// </summary>
        public Guid ParentLocationGuid { get; set; }

        /// <summary>
        /// The GUID of the defined type specifying the new location's type
        /// </summary>
        public Guid LocationTypeValueGuid { get; set; }

        /// <summary>
        /// The new location's name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Address data for the new location
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Collection of attribute values for the new location
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }

        /// <summary>
        /// Whether the city and state should be shown in the name of the returned location
        /// </summary>
        public bool ShowCityState { get; set; } = false;

        /// <summary>
        /// The security grant token to use when performing authorization checks.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}
