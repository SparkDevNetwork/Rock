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

using Rock.Model;

namespace Rock.Reporting
{
    /// <summary>
    /// Describes the value of a comparison operation, including the type of
    /// comparison to perform.
    /// </summary>
    public class ComparisonValue
    {
        /// <summary>
        /// Gets or sets the comparison type that will be performed with the value.
        /// </summary>
        /// <value>The comparison type that will be performed with the value.</value>
        public ComparisonType? ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the current raw filter value.
        /// </summary>
        /// <value>The current raw filter value.</value>
        public string Value { get; set; }
    }
}
