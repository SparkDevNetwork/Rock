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

namespace Rock.Cache
{
    /// <summary>
    /// Internal cache for looking up Id from Guid
    /// This information will be cached by the engine
    /// </summary>
    internal class CacheIdFromGuid : ItemCache<CacheIdFromGuid>
    {
        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate
        /// </summary>
        public CacheIdFromGuid( int id )
        {
            Id = id;
        }

        #endregion

        /// <summary>
        /// Gets the id.
        /// </summary>
        public int Id { get; private set; }

        #region Static Methods

        /// <summary>
        /// Returns Id associated with the Guid.  If the Item with that Guid hasn't been cached yet, returns null
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static int? GetId( Guid guid )
        {
            return GetOrAddExisting( guid.ToString(), () => null )?.Id;
        }

        #endregion
    }
}