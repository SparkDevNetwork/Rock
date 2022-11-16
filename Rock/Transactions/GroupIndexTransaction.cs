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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.UniversalSearch;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Transaction that will rebuild the UniversalSearch index of GroupMembers for a Group.
    /// This functionality will not be converted to a bus event for now.
    /// Implements the <see cref="Rock.Transactions.ITransaction" />
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    public class GroupIndexTransaction : ITransaction
    {
        /// <summary>
        /// Keep a list of all the reindex requests that have been enqueued
        /// </summary>
        private static readonly ConcurrentQueue<GroupIndexInfo> GroupIndexInfoQueue = new ConcurrentQueue<GroupIndexInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupIndexTransaction"/> class.
        /// </summary>
        /// <param name="groupIndexInfo">The group index information.</param>
        public GroupIndexTransaction( GroupIndexInfo groupIndexInfo )
        {
            GroupIndexInfoQueue.Enqueue( groupIndexInfo );
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            // Dequeue any group index requests that have been queued and not processed up to this point.
            var groupIndexInfos = new List<GroupIndexInfo>();

            while ( GroupIndexInfoQueue.TryDequeue( out GroupIndexInfo interactionTransactionInfo ) )
            {
                groupIndexInfos.Add( interactionTransactionInfo );
                System.Diagnostics.Debug.WriteLine( interactionTransactionInfo.ToString() );
            }

            if ( !groupIndexInfos.Any() )
            {
                // If all the interactions have been processed, exit.
                return;
            }

            // Get a distinct list of the Groups. This is to prevent mutliple reindex requests being processed if multiple group members have their status changed
            groupIndexInfos = groupIndexInfos.GroupBy( i => i.GroupId ).Select( i => i.FirstOrDefault() ).ToList();

            foreach ( var groupIndexInfo in groupIndexInfos )
            {
                var groupType = GroupTypeCache.Get( groupIndexInfo.GroupTypeId );
                var group = new GroupService( new RockContext() ).Get( groupIndexInfo.GroupId );
                if ( groupType != null && group != null && groupType.IsIndexEnabled && group.IsActive )
                {
                    var indexItem = Rock.UniversalSearch.IndexModels.GroupIndex.LoadByModel( group );
                    IndexContainer.IndexDocument( indexItem );
                }
            }
        }
    }

    /// <summary>
    /// Class GroupIndexInfo.
    /// </summary>
    public class GroupIndexInfo
    {
        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>The group type identifier.</value>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>The group identifier.</value>
        public int GroupId { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"GroupTypeId: {GroupTypeId}, GroupId: {GroupId}";
        }
    }
}
