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
using Rock.Model;

namespace Rock.Data
{
    /// <summary>
    /// Result object for <see cref="DbContext.SaveChanges(SaveChangesArgs)"/>
    /// </summary>
    public sealed class SaveChangesResult
    {
        /// <summary>
        /// The number of state entries written to the underlying database. This can include
        /// state entries for entities and/or relationships. Relationship state entries are
        /// created for many-to-many relationships and relationships where there is no foreign
        /// key property included in the entity class (often referred to as independent associations).
        /// </summary>
        public int RecordsUpdated { get; set; }

        /// <summary>
        /// Achievement attempts that were affected by this SaveChanges call.
        /// </summary>
        public List<AchievementAttempt> AchievementAttempts { get; set; }
    }
}
