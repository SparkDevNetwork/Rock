﻿// <copyright>
// Copyright by BEMA Software Services
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
using System.Data.Entity;
using System.Runtime.Caching;

using Rock.Data;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ReservationWorkflowService : Service<ReservationWorkflow>
    {
        /// <summary>
        /// The cache key
        /// </summary>
        private const string CACHE_KEY = "com_bemaservices_roommanagement:ReservationWorkflows";

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationWorkflowService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationWorkflowService( RockContext context ) : base( context ) { }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( ReservationWorkflow item, out string errorMessage )
        {
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets the cached triggers.
        /// </summary>
        /// <returns></returns>
        public static List<ReservationWorkflow> GetCachedTriggers()
        {
            return RockCache.GetOrAddExisting( CACHE_KEY, () => LoadCache() ) as List<ReservationWorkflow>;
        }

        /// <summary>
        /// Loads the cache.
        /// </summary>
        /// <returns></returns>
        private static List<ReservationWorkflow> LoadCache()
        {
            var triggers = new List<ReservationWorkflow>();

            using ( var rockContext = new RockContext() )
            {
                foreach ( var trigger in new ReservationWorkflowService( rockContext )
                    .Queryable().AsNoTracking() )
                {
                    triggers.Add( trigger.Clone( false ) );
                }
            }

            return triggers;
        }

        /// <summary>
        /// Flushes the cached triggers.
        /// </summary>
        [Obsolete( "Use RemoveCachedTriggers() instead." )]
        public static void FlushCachedTriggers()
        {
            RemoveCachedTriggers();
        }

        /// <summary>
        /// Flushes the cached triggers.
        /// </summary>
        public static void RemoveCachedTriggers()
        {
            RockCache.Remove( CACHE_KEY );
        }

    }
    /// <summary>
    /// Extensions methods for the ConnectionRequestWorkflow Service
    /// </summary>
    public static partial class ConnectionRequestWorkflowExtensionMethods
    {

        /// <summary>
        /// Clones a specified deep copy.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> [deep copy].</param>
        /// <returns></returns>
        public static ReservationWorkflow Clone( this ReservationWorkflow source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as ReservationWorkflow;
            }
            else
            {
                var target = new ReservationWorkflow();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from the source to the target.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this ReservationWorkflow target, ReservationWorkflow source )
        {
            target.Id = source.Id;
            target.ReservationId = source.ReservationId;
            target.ReservationWorkflowTriggerId = source.ReservationWorkflowTriggerId;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.TriggerQualifier = source.TriggerQualifier;
            target.TriggerType = source.TriggerType;
            target.WorkflowId = source.WorkflowId;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
        }
    }

}
