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
using System;

namespace Rock.Rest
{
    /// <summary>
    /// Interface for controllers that need to add additional routes beyond the default
    /// api/{controller}/{id} route.
    /// </summary>
    [RockObsolete( "1.9" )]
    [Obsolete( "Use IHasCustomHttpRoutes instead.", false )]
    public interface IHasCustomRoutes
    {
        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        void AddRoutes( System.Web.Routing.RouteCollection routes );
    }
}
