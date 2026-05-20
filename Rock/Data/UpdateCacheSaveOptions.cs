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
using System.Data.Entity;

using Rock.Web.Cache;

namespace Rock.Data;

/// <summary>
/// Contains options used by <see cref="DbContext.SaveChanges()"/> to determine
/// how or if the cache should be updated after the save operation is completed.
/// </summary>
internal class UpdateCacheSaveOptions
{
    /// <summary>
    /// Determines if the automatic <see cref="ICacheable.UpdateCache(EntityState, DbContext)"/>
    /// call is made or not.
    /// </summary>
    public bool IsUpdateCacheDisabled { get; set; }
}
