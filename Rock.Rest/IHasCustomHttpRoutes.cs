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

namespace Rock.Rest
{
    /// <summary>
    /// Interface for controllers that need to add additional routes beyond the default
    /// api/{controller}/{id} route or to override route attributes in a base class.
    /// </summary>
    public interface IHasCustomHttpRoutes
    {
        /// <summary>
        /// Adds the routes. This is called before Route attributes are evaluated, so any routes
        /// added here will take precedence.
        /// </summary>
        /// <param name="routes">The routes.</param>
        void AddRoutes( HttpRouteCollection routes );
    }
}
