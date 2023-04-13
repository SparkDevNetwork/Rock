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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Executes the "IndexDocument()" method for the Entity. Use this instead of ProcessEntityTypeIndex since it filters out duplicate index operations.
    /// </summary>
    public class IndexEntityTransaction : ITransaction
    {

        /// <summary>
        /// Keep a list of all the reindex requests that have been enqueued
        /// </summary>
        private static readonly ConcurrentQueue<EntityIndexInfo> EntityIndexInfoQueue = new ConcurrentQueue<EntityIndexInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexEntityTransaction" /> class.
        /// </summary>
        /// <param name="entityIndexInfo">The entity index information.</param>
        public IndexEntityTransaction( EntityIndexInfo entityIndexInfo )
        {
            EntityIndexInfoQueue.Enqueue( entityIndexInfo );
        }

        /// <summary>
        /// Execute method to index the document.
        /// </summary>
        public void Execute()
        {
            // Dequeue any index requests that have been queued and not processed up to this point.
            var entityIndexInfos = new List<EntityIndexInfo>();

            while ( EntityIndexInfoQueue.TryDequeue( out EntityIndexInfo entityIndexInfo ) )
            {
                entityIndexInfos.Add( entityIndexInfo );
            }

            if ( !entityIndexInfos.Any() )
            {
                // If all the interactions have been processed, exit.
                return;
            }

            // Get a distinct list of EntityIndexInfo objects. This is to prevent duplicates of the same reindex request in the same batch.
            // e.g. if multiple group members have their status changed at the same time then this will ensure the group is only reindexed once.
            entityIndexInfos = entityIndexInfos.GroupBy( i => new { i.EntityTypeId, i.EntityId } ).Select( i => i.FirstOrDefault() ).ToList();

            foreach ( var entityIndexInfo in entityIndexInfos )
            {
                IndexDocumentExecute( entityIndexInfo );
            }
        }

        private void IndexDocumentExecute( EntityIndexInfo entityIndexInfo )
        {
            var entityType = EntityTypeCache.Get( entityIndexInfo.EntityTypeId );
            
            if ( entityType != null )
            {
                Type type = entityType.GetEntityType();

                if ( type != null )
                {
                    object classInstance = Activator.CreateInstance( type, null );
                    MethodInfo indexItemMethod = type.GetMethod( "IndexDocument" );

                    if ( classInstance != null && indexItemMethod != null )
                    {
                        object[] parameters = { entityIndexInfo.EntityId };
                        indexItemMethod.Invoke( classInstance, parameters );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Class EntityIndexInfo.
    /// </summary>
    public class EntityIndexInfo
    {
        /// <summary>
        /// Gets or sets the group type identifier.
        /// </summary>
        /// <value>The group type identifier.</value>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>The group identifier.</value>
        public int EntityId { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"Entity: {EntityTypeId} {EntityTypeCache.Get( EntityTypeId ).FriendlyName}, EntityId: {EntityId}";
        }
    }
}