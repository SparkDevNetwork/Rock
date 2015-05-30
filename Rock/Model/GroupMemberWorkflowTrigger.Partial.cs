// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Runtime.Caching;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.GroupType"/> objects.
    /// </summary>
    public partial class GroupMemberWorkflowTriggerService
    {
        /// <summary>
        /// The cach e_ key
        /// </summary>
        private const string CACHE_KEY = "Rock:GroupMemberWorkflowTriggers";

        /// <summary>
        /// Gets the cached triggers.
        /// </summary>
        /// <returns></returns>
        public static List<GroupMemberWorkflowTrigger> GetCachedTriggers()
        {
            return GetOrAddExisting( () => LoadCache() );
        }

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        private static List<GroupMemberWorkflowTrigger> GetOrAddExisting( Func<List<GroupMemberWorkflowTrigger>> factory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            var newValue = new Lazy<List<GroupMemberWorkflowTrigger>>( factory );
            var oldValue = cache.AddOrGetExisting( CACHE_KEY, newValue, new CacheItemPolicy() ) as Lazy<List<GroupMemberWorkflowTrigger>>;
            try
            {
                return ( oldValue ?? newValue ).Value;
            }
            catch
            {
                cache.Remove( CACHE_KEY );
                throw;
            }
        }

        /// <summary>
        /// Loads the cache.
        /// </summary>
        /// <returns></returns>
        private static List<GroupMemberWorkflowTrigger> LoadCache()
        {
            var triggers = new List<GroupMemberWorkflowTrigger>();

            using ( var rockContext = new RockContext() )
            {
                foreach( var trigger in new GroupMemberWorkflowTriggerService( rockContext )
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
}
