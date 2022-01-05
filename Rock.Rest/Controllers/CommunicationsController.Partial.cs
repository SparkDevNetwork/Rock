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
using System.Threading.Tasks;
using System.Web.Http;
using Rock.Data;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Communications REST API
    /// </summary>
    [RockGuid( "1df46cc7-326a-48e6-b0a9-9330badd0512" )]
    public partial class CommunicationsController
    {
        /// <summary>
        /// Sends a communication.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/Communications/Send/{id}" )]
        [RockGuid( "272c25fc-c608-4673-99d5-7fb1377d8a61" )]
        public virtual Task Send( int id )
        {
            var communication = GetById( id );
            return Model.Communication.SendAsync( communication );
        }
    }
}
