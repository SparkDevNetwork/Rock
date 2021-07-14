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
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Rock.Rest.Utility
{
    /// <summary>
    /// Custom action value binder that handles special /api/v2 endpoint use
    /// cases.
    /// </summary>
    /// <seealso cref="System.Web.Http.ModelBinding.DefaultActionValueBinder" />
    public class RockActionValueBinder : DefaultActionValueBinder
    {
        /// <inheritdoc/>
        protected override HttpParameterBinding GetParameterBinding( HttpParameterDescriptor parameter )
        {
            var binder = base.GetParameterBinding( parameter );

            // Override the Formatter binder (which read POST body content) with
            // our own version that handles /api/v2 endpoints properly.
            if ( binder is FormatterParameterBinding )
            {
                return new RockFormatterParameterBinding( parameter, parameter.Configuration.Formatters, parameter.Configuration.Services.GetBodyModelValidator() );
            }

            return binder;
        }
    }
}
