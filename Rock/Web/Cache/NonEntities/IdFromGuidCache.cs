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
        /// <returns></returns>
        public static int? GetId( Guid guid )
        {
            return GetOrAddExisting( guid.ToString(), () => null )?.Id;
        }

        #endregion
    }
}