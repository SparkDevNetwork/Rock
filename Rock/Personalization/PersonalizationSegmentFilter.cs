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
using System.Linq.Expressions;

using Rock.Model;

namespace Rock.Personalization
{
    /// <summary>
    /// Filter to determine if a person alias meets criteria based on activity.
    /// </summary>
    public abstract class PersonalizationSegmentFilter
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets the description based on how the filter is configured.
        /// </summary>
        /// <returns>System.String.</returns>
        public abstract string GetDescription();

        /// <summary>
        /// Gets Expression that will be used as one of the WHERE clauses for the PersonAlias query.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public abstract Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression );
    }
}
