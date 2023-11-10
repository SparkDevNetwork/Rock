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

using System.Diagnostics;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Bulk Deletes the Interactions
    /// </summary>
    public sealed class DeleteInteractions : BusStartedTask<DeleteInteractions.Message>
    {
        private int commandTimeout = 180;
        private int batchAmount = 5000;

        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            var rockContext = new RockContext();
#if REVIEW_NET5_0_OR_GREATER
            rockContext.Database.SetCommandTimeout( commandTimeout );
#else
            rockContext.Database.CommandTimeout = commandTimeout;
#endif
            var interactionComponentService = new InteractionComponentService( rockContext );
            var interactionService = new InteractionService( rockContext );

            /*
             SK - 05/26/2022
             Directly pass the pageId as well as siteId as Page and PageCache most likely will return null as it may have deleted by the time this background process gets executed.
             */
            var componentQuery = interactionComponentService.QueryByPage( message.SiteId, message.PageId );
            foreach ( var item in componentQuery )
            {
                var interactionQry = interactionService.Queryable().Where( a => a.InteractionComponentId == item.Id );
                BulkDeleteInteractionInChunks( interactionQry );
            }

            interactionComponentService.DeleteRange( componentQuery );
            rockContext.SaveChanges();

        }

        /// <summary>
        /// Does a <see cref="M:RockContext.BulkDelete"></see> on the records listed in the query, but does it in chunks to help prevent timeouts
        /// </summary>
        /// <param name="recordsToDeleteQuery">The records to delete query.</param>
        /// <returns>
        /// The number of records deleted
        /// </returns>
        private int BulkDeleteInteractionInChunks( IQueryable<Interaction> recordsToDeleteQuery )
        {
            int totalRowsDeleted = 0;

            // Event though BulkDelete has a batch amount, that could exceed our command time out since that'll just be one command for the whole thing, so let's break it up into multiple commands
            // Also, this helps prevent new record inserts waiting the batch operation (if Snapshot Isolation is disabled)
            var chunkQuery = recordsToDeleteQuery.Take( batchAmount );

            using ( var bulkDeleteContext = new RockContext() )
            {
#if REVIEW_NET5_0_OR_GREATER
                bulkDeleteContext.Database.SetCommandTimeout( commandTimeout );
#else
                bulkDeleteContext.Database.CommandTimeout = commandTimeout;
#endif
                var keepDeleting = true;
                while ( keepDeleting )
                {
                    var rowsDeleted = bulkDeleteContext.BulkDelete( chunkQuery );
                    keepDeleting = rowsDeleted > 0;
                    totalRowsDeleted += rowsDeleted;
                }
            }

            return totalRowsDeleted;
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the page identifiers to delete.
            /// </summary>
            /// <value>
            /// The page to delete.
            /// </value>
            public int PageId { get; set; }

            /// <summary>
            /// Gets or sets the related site identifiers to delete.
            /// </summary>
            /// <value>
            /// The related site to delete.
            /// </value>
            public int SiteId { get; set; }
        }
    }
}