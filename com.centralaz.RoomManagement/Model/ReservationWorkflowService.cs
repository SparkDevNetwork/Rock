// <copyright>
// Copyright by the Central Christian Church
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
using com.centralaz.RoomManagement.Model;
using Rock.Data;
using Rock.Web.Cache;

namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ReservationWorkflowService : Service<ReservationWorkflow>
    {
        /// <summary>
        /// The cache key
        /// </summary>
        private const string CACHE_KEY = "com_centralaz_roommanagement:ReservationWorkflows";

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
            return GetOrAddExisting( () => LoadCache() );
        }

        private static List<ReservationWorkflow> GetOrAddExisting( Func<List<ReservationWorkflow>> factory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            var value = cache.Get( CACHE_KEY ) as List<ReservationWorkflow>;
            if ( value != null )
            {
                return value;
            }

            value = factory();
            if ( value != null )
            {
                cache.Set( CACHE_KEY, value, new CacheItemPolicy() );
            }
            return value;
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
        public static void FlushCachedTriggers()
        {
            RockMemoryCache cache = RockMemoryCache.Default;
            cache.Remove( CACHE_KEY );
        }


    }
    public static partial class ConnectionRequestWorkflowExtensionMethods
    {

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
