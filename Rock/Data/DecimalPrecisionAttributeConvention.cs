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

using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace Rock.Data
{
    /// <summary>
    /// Decimal Precision Attribute Convention
    /// </summary>
    public class DecimalPrecisionAttributeConvention : PrimitivePropertyAttributeConfigurationConvention<DecimalPrecisionAttribute>
    {
        /// <summary>
        /// Applies this convention to a property that has an attribute of type TAttribute applied.
        /// </summary>
        /// <param name="configuration">The configuration for the property that has the attribute.</param>
        /// <param name="attribute">The attribute.</param>
        public override void Apply( ConventionPrimitivePropertyConfiguration configuration, DecimalPrecisionAttribute attribute )
        {
            configuration.HasPrecision( attribute.Precision, attribute.Scale );
        }
    }
}
