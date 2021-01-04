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

namespace Rock.Data
{
    /// <summary>
    /// Decimal Precision Attribute
    /// </summary>
    public class DecimalPrecisionAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        public byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        public byte Scale { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalPrecisionAttribute"/> class.
        /// </summary>
        /// <param name="precision">The precision.</param>
        /// <param name="scale">The scale.</param>
        public DecimalPrecisionAttribute( byte precision, byte scale )
        {
            Precision = precision;
            Scale = scale;
        }
    }
}