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
using System.Web.Http.ValueProviders;

namespace Rock.Rest.Utility.ValueProviders
{
    /// <summary>
    /// Custom value provider result that handles conversion of DateTime
    /// into the API v2 format.
    /// </summary>
    /// <seealso cref="System.Web.Http.ValueProviders.ValueProviderResult" />
    public class RockApiV2ValueProviderResult : ValueProviderResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockApiV2ValueProviderResult"/> class.
        /// </summary>
        /// <param name="rawValue">The raw value.</param>
        /// <param name="attemptedValue">The attempted value.</param>
        /// <param name="culture">The culture.</param>
        public RockApiV2ValueProviderResult( object rawValue, string attemptedValue, CultureInfo culture )
            : base( rawValue, attemptedValue, culture )
        {
        }

        /// <inheritdoc/>
        public override object ConvertTo( Type type, CultureInfo culture )
        {
            if ( type == typeof( DateTime ) || type == typeof( DateTime? ) )
            {
                var value = base.ConvertTo( type, culture );

                // If the value is a DateTime then it will be in the local system
                // date and time. Convert it to Rock Organization date time.
                if ( value is DateTime dateTime )
                {
                    return RockDateTime.ConvertLocalDateTimeToRockDateTime( dateTime );
                }
            }

            return base.ConvertTo( type, culture );
        }
    }
}
