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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// NcoaHistory Service class
    /// </summary>
    public partial class NcoaHistoryService
    {
        /// <summary>
        /// Gets the count of NCOA addresses.
        /// </summary>
        /// <returns>Count of NCOA addresses</returns>
        public int Count()
        {
            return this.Queryable().AsNoTracking().Count();
        }

        /// <summary>
        /// Get the count of addresses that are marked as moved.
        /// </summary>
        /// <returns>Count of addresses that are marked as moved.</returns>
        public int MovedCount()
        {
            return this.Queryable().AsNoTracking().Where( n => n.MoveType != MoveType.None ).Count();
        }
    }
}
