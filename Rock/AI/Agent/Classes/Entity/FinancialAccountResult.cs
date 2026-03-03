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

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Hierarchical result model for a financial account (fund) used by AI tools.
    /// Includes basic descriptive properties plus child accounts for building
    /// a parent-to-child tree suitable for selection or display scenarios.
    /// </summary>
    internal class FinancialAccountResult : EntityResultBase
    {
        /// <summary>
        /// Gets or sets the public (display) name of the account.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public description of the account that can be shown to end users.
        /// </summary>
        public string PublicDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether contributions to this account are tax deductible.
        /// </summary>
        public bool? IsTaxDeductible { get; set; }

        /// <summary>
        /// Gets or sets the child accounts that are direct descendants of this account.
        /// This list is empty when the account has no children. Each child may itself
        /// contain additional children forming a recursive hierarchy.
        /// </summary>
        public List<FinancialAccountResult> Children { get; set; }

        /// <summary>
        /// Gets or sets the IdKey (hashed identifier) of the parent account if one exists;
        /// otherwise <c>null</c> for top-level accounts.
        /// </summary>
        public string ParentAccountIdKey { get; set; }

        /// <summary>
        /// Gets or sets the campus associated with this account when applicable (e.g., campus-specific child accounts).
        /// Will be <c>null</c> if the account is not campus-specific.
        /// </summary>
        public CampusResult Campus { get; set; }
    }
}
