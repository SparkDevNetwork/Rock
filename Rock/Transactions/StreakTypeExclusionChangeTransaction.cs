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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Transaction to process changes that occur to a streak type exclusion
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    [Obsolete( "Use ProcessStreakTypeExclusionChange Task instead." )]
    [RockObsolete( "1.13" )]
    public class StreakTypeExclusionChangeTransaction : ITransaction
    {
        /// <summary>
        /// Gets the streak identifier.
        /// </summary>
        private int StreakTypeId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreakTypeExclusionChangeTransaction"/> class.
        /// </summary>
        /// <param name="entry">The entry.</param>
        public StreakTypeExclusionChangeTransaction( DbEntityEntry entry )
        {
            var isAdded = entry.State == EntityState.Added;
            var mapIsModified = entry.State == EntityState.Modified && entry.Property( "ExclusionMap" )?.IsModified == true;

            if ( !isAdded && !mapIsModified )
            {
                return;
            }

            var streakTypeExclusion = entry.Entity as StreakTypeExclusion;
            StreakTypeId = streakTypeExclusion?.StreakTypeId ?? default;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute()
        {
            if ( StreakTypeId == default )
            {
                return;
            }

            StreakTypeService.HandlePostSaveChanges( StreakTypeId );
        }
    }
}