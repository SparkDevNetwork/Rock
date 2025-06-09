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

using Rock;

namespace Rock.Model
{
    /// <summary>
    /// LavaEndpoint Service class
    /// </summary>
    public partial class LavaEndpointService
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.LavaEndpoint">LavaEndpoints</see> that belong to a specified <see cref="Rock.Model.LavaApplication"/> retrieved by the LavaEndpoint's LavaApplicationId.
        /// </summary>
        /// <param name="lavaApplicationId">A <see cref="System.Int32"/> representing the LavaApplicationId of the <see cref="Rock.Model.LavaApplication"/> to retrieve <see cref="Rock.Model.LavaEndpoint">LavaEndPoints</see> for.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.LavaEndpoint">LavaEndpoints</see> that belong to the specified <see cref="Rock.Model.LavaApplication"/>. The <see cref="Rock.Model.LavaEndpoint">LavaEndpoints</see> will 
        /// be ordered by the <see cref="LavaEndpoint">LavaEndpoint's</see> LavaApplicationId property.</returns>
        public IOrderedQueryable<LavaEndpoint> GetByLavApplicationId( int lavaApplicationId )
        {
            return Queryable()
                .Where( t => t.LavaApplicationId == lavaApplicationId )
                .OrderBy( t => t.LavaApplicationId );
        }
    }
}
