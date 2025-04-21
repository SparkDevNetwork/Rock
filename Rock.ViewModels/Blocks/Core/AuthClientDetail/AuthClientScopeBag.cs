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

namespace Rock.ViewModels.Blocks.Core.AuthClientDetail
{
    /// <summary>
    /// Auth Client Scope Bag
    /// </summary>
    public class AuthClientScopeBag
    {
        /// <summary>
        /// Gets or sets the identifier key of this entity.
        /// </summary>
        /// <value>
        /// The identifier key of this entity.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the scope.
        /// </summary>
        /// <value>
        /// The name of the scope.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the name of the scope public.
        /// </summary>
        /// <value>
        /// The name of the scope public.
        /// </value>
        public string PublicName { get; set; }
        /// <summary>
        /// Gets or sets the name of the claim.
        /// </summary>
        /// <value>
        /// The name of the claim.
        /// </value>
        public string ClaimName { get; set; }
        /// <summary>
        /// Gets or sets the name of the claim public.
        /// </summary>
        /// <value>
        /// The name of the claim public.
        /// </value>
        public string PublicClaimName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected { get; set; }
    }
}
