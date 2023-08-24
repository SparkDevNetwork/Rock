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
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.LocationDetail
{
    /// <summary>
    /// AddressStandardizationResultBag
    /// </summary>
    public sealed class AddressStandardizationResultBag
    {
        /// <summary>
        /// Gets or sets the address fields.
        /// </summary>
        /// <value>The address fields.</value>
        public AddressControlBag AddressFields { get; set; }

        /// <summary>
        /// Gets or sets the standardize attempted result.
        /// </summary>
        /// <value>The standardize attempted result.</value>
        public string StandardizeAttemptedResult { get; set; }

        /// <summary>
        /// Gets or sets the geocode attempted result.
        /// </summary>
        /// <value>The geocode attempted result.</value>
        public string GeocodeAttemptedResult { get; set; }
    }                 
}
