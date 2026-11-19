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

#if NET
namespace Rock.Enums.Reporting
#else
namespace Rock.Model
#endif
{
    /// <summary>
    /// The unit type for a metric
    /// </summary>
#if !NET
    [Enums.EnumDomain( "Reporting" )]
#endif
    public enum UnitType
    {
        /// <summary>
        /// Numeric
        /// </summary>
        Numeric = 0,

        /// <summary>
        /// Currency
        /// </summary>
        Currency = 1,

        /// <summary>
        /// Percentage
        /// </summary>
        Percentage = 2
    }
}
