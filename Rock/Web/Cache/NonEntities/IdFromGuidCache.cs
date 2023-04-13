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
using System.Runtime.Serialization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Internal cache for looking up Id from Guid
    /// This information will be cached by the engine
    /// </summary>
	[Serializable]
    [DataContract]
    internal class IdFromGuidCache : ItemCache<IdFromGuidCache>
    {
        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate
        /// </summary>
        public IdFromGuidCache( int id )
        {
            Id = id;
        }

        #endregion

        /// <summary>
        /// Gets the id.
        /// </summary>
		[DataMember]
        public int Id { get; private set; }

        #region Static Methods


        /// <summary>
        /// Returns Id associated with the Guid.  If the Item with that Guid hasn't been cached yet, returns null
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <typeparam name="T">The type of the cached object whose identifier is to be looked up.</typeparam>
        /// <returns>The identifier if found in cache or <c>null</c>.</returns>
        public static int? GetId<T>( Guid guid )
        {
            return GetOrAddExisting( $"{typeof(T).FullName}-{guid}", () => null )?.Id;
        }

        /// <summary>
        /// Updates the cache identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="id">The identifier.</param>
        /// <typeparam name="T">The type of the cached object whose identifier is to be updated.</typeparam>
        internal protected static void UpdateCacheId<T>( Guid guid, int id )
        {
            UpdateCacheItem( $"{typeof( T ).FullName}-{guid}", new IdFromGuidCache( id ) );
        }

        /// <summary>
        /// Attempts to get an item from the cache without adding it if it does
        /// not already exist.
        /// </summary>
        /// <param name="key">The key that identifies the item.</param>
        /// <param name="id">On return will contain the identifier.</param>
        /// <typeparam name="T">The type of the cached object whose identifier is to be looked up.</typeparam>
        /// <returns><c>true</c> if the item was found in cache, <c>false</c> otherwise.</returns>
        internal protected static bool TryGetId<T>( string key, out int id )
        {
            var result = TryGet( $"{typeof(T).FullName}-{key}", out var item );

            if ( result )
            {
                id = item.Id;
            }
            else
            {
                id = 0;
            }

            return result;
        }

        #endregion
    }
}