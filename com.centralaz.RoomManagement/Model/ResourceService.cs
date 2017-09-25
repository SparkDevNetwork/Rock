// <copyright>
// Copyright by the Central Christian Church
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
using Rock.Data;

namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ResourceService : Service<Resource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ResourceService( RockContext context ) : base( context ) { }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can delete the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanDelete( Resource item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<ReservationResource>( Context ).Queryable().Any( a => a.ResourceId == item.Id ) )
            {
                errorMessage = string.Format( "This {0} is used in a {1}.", Resource.FriendlyTypeName, ReservationResource.FriendlyTypeName );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes a specified resource. Returns a boolean flag indicating if the deletion was successful.
        /// </summary>
        /// <param name="item">The <see cref="com.centralaz.RoomManagement.Model.Resource" /> to delete.</param>
        /// <returns>
        /// A <see cref="System.Boolean" /> that indicates if the <see cref="com.centralaz.RoomManagement.Model.Resource" /> was deleted successfully.
        /// </returns>
        public override bool Delete( Resource item )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item );
        }
    }
}
