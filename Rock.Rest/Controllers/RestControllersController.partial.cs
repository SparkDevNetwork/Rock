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

using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RestControllersController 
    {
        /// <summary>
        /// Ensures that rest controllers have been registered to the Rock Database
        /// </summary>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/RestControllers/EnsureRestControllers" )]
        public bool EnsureRestControllers()
        {
            RestControllerService.RegisterControllers();

            return true;
        }
    }
}
