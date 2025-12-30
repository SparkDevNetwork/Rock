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

using Rock.Net;
using Rock.ViewModels.Rest.Controls;

namespace Rock.Field
{
    /// <summary>
    /// Represents the options used when retrieving items for a universal
    /// item search picker control.
    /// </summary>
    public class UniversalItemSearchPickerGetItemsOptions
    {
        /// <summary>
        /// The options that were sent by the UI picker.
        /// </summary>
        public UniversalItemSearchPickerOptionsBag PickerOptions { get; set; }

        /// <summary>
        /// The private configuration values that describe the configured state
        /// of the field type.
        /// </summary>
        public Dictionary<string, string> PrivateConfigurationValues { get; set; }

        /// <summary>
        /// The request context that describes the current network request.
        /// </summary>
        public RockRequestContext RequestContext { get; set; }
    }
}
