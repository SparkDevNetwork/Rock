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

namespace Rock.ClientService.Core.DefinedValue.Options
{
    /// <summary>
    /// Behavioral options for retrieving a list of defined values to be send
    /// to the client.
    /// </summary>
    public class DefinedValueOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether inactive values should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inactive values should be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the description should be
        /// used instead of the value text.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the description should be used; otherwise, <c>false</c>.
        /// </value>
        public bool UseDescription { get; set; }
    }
}
