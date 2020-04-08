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
using System.Collections.Generic;
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegistrationRegistrantsController
    {
        #region Group Placement Related

        /// <summary>
        /// Gets the group placement registrants.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/RegistrationRegistrants/GetGroupPlacementRegistrants" )]
        [HttpPost]
        public IEnumerable<GroupPlacementRegistrant> GetGroupPlacementRegistrants( [FromBody]GetGroupPlacementRegistrantsParameters options )
        {
            var rockContext = new RockContext();
            var registrantService = new RegistrationRegistrantService( rockContext );
            return registrantService.GetGroupPlacementRegistrants( options, this.GetPerson() );
        }

        #endregion Group Placement Related
    }
}
