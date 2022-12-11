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

namespace Rock.Model
{
    /// <summary>
    /// The type of Metric Value that a Metric Value represents
    /// </summary>
    [Enums.EnumDomain( "Reporting" )]
    public enum MetricValueType
    {
        /// <summary>
        /// Metric Value represents something that was measured (for example: Fundraising Total)
        /// </summary>
        Measure = 0,

        /// <summary>
        /// Metric Value represents a goal (for example: Fundraising Goal)
        /// </summary>
        Goal = 1
    }
}
