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
using System.Linq;

namespace Rock.Data
{
    /// <summary>
    /// Represents a model that supports being active or not
    /// </summary>
    public interface IHasActiveFlag
    {
        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        /// <value>
        /// Active.
        /// </value>
        bool IsActive { get; set; }
    }

    /// <summary>
    /// Extensions for classes implementing the interface
    /// </summary>
    public static class HasActiveFlagExtensions
    {
        /// <summary>
        /// Return the subset of items that are active.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static IQueryable<T> WhereIsActive<T>( this IQueryable<T> query ) where T : IHasActiveFlag
        {
            return query.Where( i => i.IsActive );
        }
    }
}
