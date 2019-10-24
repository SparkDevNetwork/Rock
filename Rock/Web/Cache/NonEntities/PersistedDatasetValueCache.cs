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
    /// Internal cache for the object value of <see cref="PersistedDatasetCache.ResultData"/> />
    /// This information will be cached by the engine
    /// </summary>
    internal class PersistedDatasetValueCache : ItemCache<PersistedDatasetValueCache>
    {
        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate
        /// </summary>
        public PersistedDatasetValueCache( object resultDataObjectValue )
        {
            ResultDataObjectValue = resultDataObjectValue;
        }

        #endregion

        /// <summary>
        /// Gets the id.
        /// </summary>
        public object ResultDataObjectValue { get; private set; }
    }
}