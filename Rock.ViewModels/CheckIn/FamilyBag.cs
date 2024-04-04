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

namespace Rock.ViewModels.CheckIn
{
    /// <summary>
    /// A search result item representing a single family in check-in
    /// family search.
    /// </summary>
    public class FamilyBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the family Group.
        /// </summary>
        /// <value>The unique identifier of the family Group.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the family.
        /// </summary>
        /// <value>The name of the family.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the family campus unique identifier.
        /// </summary>
        /// <value>The family campus unique identifier.</value>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the family members.
        /// </summary>
        /// <value>The family members.</value>
        public List<FamilyMemberBag> Members { get; set; }
    }
}
