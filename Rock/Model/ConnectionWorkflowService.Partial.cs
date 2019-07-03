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
using System.Data.Entity;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{

    public partial class ConnectionWorkflowService
    {
        /// <summary>
        /// The cach e_ key
        /// </summary>
        private const string CACHE_KEY = "Rock:ConnectionWorkflows";

        /// <summary>
        /// Gets the cached triggers.
        /// </summary>
        /// <returns></returns>
        public static List<ConnectionWorkflow> GetCachedTriggers()
        {
            return RockCache.GetOrAddExisting( CACHE_KEY, () => LoadCache() ) as List<ConnectionWorkflow>;
        }

        /// <summary>
        /// Loads the cache.
        /// </summary>
        /// <returns></returns>
        private static List<ConnectionWorkflow> LoadCache()
        {
            var triggers = new List<ConnectionWorkflow>();

            using ( var rockContext = new RockContext() )
            {
                foreach( var trigger in new ConnectionWorkflowService( rockContext )
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
        [RockObsolete( "1.8" )]
        [Obsolete( "Use RemoveCachedTriggers() instead.")]
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
}
