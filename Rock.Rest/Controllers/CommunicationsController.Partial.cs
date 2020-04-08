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
using System.Web.Http;

using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Communications REST API
    /// </summary>
    public partial class CommunicationsController
    {
        /// <summary>
        /// Sends a communication.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Communications/Send/{id}" )]
        public virtual void Send( int id )
        {
            var communication = GetById( id );
            Rock.Model.Communication.Send( communication );
        }
    }
}
