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

namespace Rock.ViewModels.Blocks.Security.AuthClaims
{
    /// <summary>
    /// The request bag for saving an auth claim in the AuthClaims block.
    /// </summary>
    public class AuthClaimRequestBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the claim to update, or empty/null for a new claim.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the claim name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public name.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the claim value (Lava template).
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
