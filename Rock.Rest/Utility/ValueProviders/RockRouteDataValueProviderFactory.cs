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

using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace Rock.Rest.Utility.ValueProviders
{
    /// <summary>
    /// Custom <see cref="RouteDataValueProviderFactory"/> implementation that
    /// uses our custom provider to handle special API v2 considerations.
    /// </summary>
    /// <seealso cref="System.Web.Http.ValueProviders.Providers.QueryStringValueProviderFactory" />
    public class RockRouteDataValueProviderFactory : RouteDataValueProviderFactory
    {
        /// <inheritdoc/>
        public override IValueProvider GetValueProvider( HttpActionContext actionContext )
        {
            return new RockRouteDataValueProvider( actionContext );
        }
    }
}
