// <copyright>
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
    public class ReservationWorkflowTriggerService : Service<ReservationWorkflowTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationWorkflowTriggerService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ReservationWorkflowTriggerService( RockContext context ) : base( context ) { }

        /// <summary>
        /// The cach e_ key
        /// </summary>
        private const string CACHE_KEY = "Rock:ReservationWorkflowTriggers";

        /// <summary>
        /// Gets the cached triggers.
        /// </summary>
        /// <returns></returns>
        public static List<ReservationWorkflowTrigger> GetCachedTriggers()
        {
            return RockCache.GetOrAddExisting( CACHE_KEY, () => LoadCache() ) as List<ReservationWorkflowTrigger>;
        }

        /// <summary>
        /// Loads the cache.
        /// </summary>
        /// <returns></returns>
        private static List<ReservationWorkflowTrigger> LoadCache()
        {
            var triggers = new List<ReservationWorkflowTrigger>();

            using ( var rockContext = new RockContext() )
            {
                foreach ( var trigger in new ReservationWorkflowTriggerService( rockContext )
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
    /// Class for ReservationWorkflowTrigger extension methods.
    /// </summary>
    public static partial class ReservationWorkflowTriggerExtensionMethods
    {
        /// <summary>
        /// Clones a specified deep copy.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> [deep copy].</param>
        /// <returns></returns>
        public static ReservationWorkflowTrigger Clone( this ReservationWorkflowTrigger source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as ReservationWorkflowTrigger;
            }
            else
            {
                var target = new ReservationWorkflowTrigger();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this ReservationWorkflowTrigger target, ReservationWorkflowTrigger source )
        {
            target.Id = source.Id;
            target.ForeignGuid = source.ForeignGuid;
            target.ReservationTypeId = source.ReservationTypeId;
            target.ForeignKey = source.ForeignKey;
            target.QualifierValue = source.QualifierValue;
            target.TriggerType = source.TriggerType;
            target.WorkflowTypeId = source.WorkflowTypeId;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
        }
    }

}
