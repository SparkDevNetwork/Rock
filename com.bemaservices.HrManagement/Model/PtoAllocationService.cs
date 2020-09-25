// <copyright>
// Copyright by BEMA Information Technologies
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

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class PtoAllocationService : Service<PtoAllocation>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoAllocationService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PtoAllocationService( RockContext context ) : base( context ) { }

        public bool CanDelete( PtoAllocation ptoAllocation, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( new Service<PtoRequest>( Context ).Queryable().Any( a => a.PtoAllocationId == ptoAllocation.Id ) )
            {
                errorMessage = string.Format( "This {0} is assigned to a {1}.", PtoAllocation.FriendlyTypeName, PtoRequest.FriendlyTypeName );
                return false;
            }

            return true;
        }
    }
}
