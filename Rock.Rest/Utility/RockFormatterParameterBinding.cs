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
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Validation;

namespace Rock.Rest.Utility
{
    /// <summary>
    /// Parameter Binding that handles differences in v1 and v2 API endpoints.
    /// </summary>
    /// <seealso cref="System.Web.Http.ModelBinding.FormatterParameterBinding" />
    public class RockFormatterParameterBinding : FormatterParameterBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockFormatterParameterBinding"/> class.
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="formatters">The formatter.</param>
        /// <param name="bodyModelValidator">The body model validator.</param>
        public RockFormatterParameterBinding( HttpParameterDescriptor descriptor, IEnumerable<MediaTypeFormatter> formatters, IBodyModelValidator bodyModelValidator )
            : base( descriptor, formatters, bodyModelValidator )
        {
        }

        /// <inheritdoc/>
        public override Task<object> ReadContentAsync( HttpRequestMessage request, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger, CancellationToken cancellationToken )
        {
            var apiVersionFormatters = formatters.ToList();

            // Replace the ApiPickerJsonMediaTypeFormatter with the appropriate
            // API formatter. This handles the fact that the correct formatter
            // will be used on _output_ to the client, but not parsing POST data.
            for ( int i = 0; i < apiVersionFormatters.Count; i++ )
            {
                if ( apiVersionFormatters[i] is ApiPickerJsonMediaTypeFormatter apiPicker )
                {
                    if ( request.RequestUri.AbsolutePath.StartsWith( "/api/v2/", StringComparison.OrdinalIgnoreCase ) )
                    {
                        apiVersionFormatters[i] = apiPicker.ApiV2Formatter;
                    }
                    else
                    {
                        apiVersionFormatters[i] = apiPicker.ApiV1Formatter;
                    }
                }
            }

            return base.ReadContentAsync( request, type, apiVersionFormatters, formatterLogger, cancellationToken );
        }
    }
}
