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
using System.Globalization;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using System.Web.Http.ValueProviders.Providers;

namespace Rock.Rest.Utility.ValueProviders
{
    /// <summary>
    /// Custom <see cref="QueryStringValueProvider"/> that handles special API v2
    /// considerations.
    /// </summary>
    /// <seealso cref="System.Web.Http.ValueProviders.Providers.QueryStringValueProvider" />
    public class RockQueryStringValueProvider : QueryStringValueProvider
    {
        private readonly HttpActionContext _actionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RockQueryStringValueProvider"/> class.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        public RockQueryStringValueProvider( HttpActionContext actionContext )
            : base( actionContext, CultureInfo.InvariantCulture )
        {
            _actionContext = actionContext;
        }

        /// <inheritdoc/>
        public override ValueProviderResult GetValue( string key )
        {
            var valueResult = base.GetValue( key );

            if ( valueResult == null )
            {
                return null;
            }

            // If running on API v2 then wrap in our own result that will
            // handle special conversions.
            if ( _actionContext.Request.RequestUri.AbsolutePath.StartsWith( "/api/v2/", StringComparison.OrdinalIgnoreCase ) )
            {
                valueResult = new RockApiV2ValueProviderResult( valueResult.RawValue, valueResult.AttemptedValue, valueResult.Culture );
            }

            return valueResult;
        }
    }
}
