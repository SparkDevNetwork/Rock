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

namespace Rock.ViewModels.Blocks.Security.VerifySecurity
{
    /// <summary>
    /// The selection criteria describing which entity and person to verify
    /// security for in the Verify Security block.
    /// </summary>
    public class VerifySecuritySelectionBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the person alias for the
        /// person whose security is being checked. When null, the current
        /// person's security is checked instead.
        /// </summary>
        public Guid? PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the entity type that the
        /// security check applies to.
        /// </summary>
        public Guid? EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the entity whose security is being
        /// checked. Accepts an integer Id, a Guid, or an IdKey.
        /// </summary>
        public string EntityIdentifier { get; set; }
    }
}
