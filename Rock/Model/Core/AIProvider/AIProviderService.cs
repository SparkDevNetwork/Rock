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
    /// 
    /// </summary>
    public partial class AIProviderService
    {
        /// <summary>
        /// Gets the active no tracking.
        /// </summary>
        /// <returns></returns>
        public IQueryable<AIProvider> GetActiveNoTracking()
        {
            return Queryable()
                .AsNoTracking()
                .Where( a => a.IsActive == true );
        }

        /// <summary>
        /// Gets all no tracking.
        /// </summary>
        /// <returns></returns>
        public IQueryable<AIProvider> GetAllNoTracking()
        {
            return Queryable().AsNoTracking();
        }

        /// <summary>
        /// Returns the active component
        /// </summary>
        /// <returns></returns>
        public AIProvider GetActiveProvider()
        {
            var activeProvider = this.Queryable().FirstOrDefault( p => p.IsActive );
            return activeProvider;
        }
    }
}
