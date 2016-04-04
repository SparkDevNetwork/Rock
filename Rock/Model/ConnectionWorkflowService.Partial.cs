﻿// <copyright>
// Copyright by the Spark Development Network
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
            return GetOrAddExisting( () => LoadCache() );
        }

        /// <summary>
        /// Gets the or add existing.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        private static List<ConnectionWorkflow> GetOrAddExisting( Func<List<ConnectionWorkflow>> factory )
        {
            RockMemoryCache cache = RockMemoryCache.Default;

            var value = cache.Get( CACHE_KEY ) as List<ConnectionWorkflow>;
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
        public static void FlushCachedTriggers()
        {
            RockMemoryCache cache = RockMemoryCache.Default;
            cache.Remove( CACHE_KEY );
        }

    }
}
