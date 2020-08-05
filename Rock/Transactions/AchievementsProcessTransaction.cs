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
using System.Linq;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Transaction to process achievements for updated source entities
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    public class AchievementsProcessingTransaction : ITransaction
    {
        /// <summary>
        /// The entities that need to be processed
        /// </summary>
        private IEnumerable<IEntity> SourceEntities { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementAttemptChangeTransaction"/> class.
        /// </summary>
        /// <param name="sourceEntities">The source entities.</param>
        public AchievementsProcessingTransaction( IEnumerable<IEntity> sourceEntities )
        {
            if ( sourceEntities == null || !sourceEntities.Any() )
            {
                return;
            }

            SourceEntities = sourceEntities;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Execute()
        {
            if ( SourceEntities == null || !SourceEntities.Any() )
            {
                return;
            }

            foreach ( var sourceEntity in SourceEntities )
            {
                AchievementTypeCache.ProcessAchievements( sourceEntity );
            }
        }
    }
}