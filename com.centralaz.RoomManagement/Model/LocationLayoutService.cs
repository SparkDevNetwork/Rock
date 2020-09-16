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
using Rock.Data;

namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class LocationLayoutService : Service<LocationLayout>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LocationLayoutService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public LocationLayoutService( RockContext context ) : base( context ) { }
    }
}
