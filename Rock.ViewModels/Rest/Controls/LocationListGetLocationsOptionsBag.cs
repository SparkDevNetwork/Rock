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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the ValidateAddress API action of
    /// the LocationAddressPicker control.
    /// </summary>
    public class LocationListGetLocationsOptionsBag
    {
        /// <summary>
        /// Guid of the location the results should be children of
        /// </summary>
        public Guid ParentLocationGuid { get; set; }

        /// <summary>
        /// GUID of the Location Type that the locations need to be
        /// </summary>
        public Guid LocationTypeValueGuid { get; set; }

        /// <summary>
        /// Whether to include city and state in the name of the location in the list
        /// </summary>
        public bool ShowCityState { get; set; } = false;

        /// <summary>
        /// The security grant token to use when performing authorization checks.
        /// </summary>
        public string SecurityGrantToken { get; set; }
    }
}
