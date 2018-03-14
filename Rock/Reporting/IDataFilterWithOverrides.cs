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
using Rock.Data;
using Rock.Model;

namespace Rock.Reporting
{
    /// <summary>
    ///  A DataFilter that supports DataViewFilterOverrides
    /// </summary>
    public interface IDataFilterWithOverrides
    {
        /// <summary>
        /// Gets the expression with overrides.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="dataViewFilterOverrides">The data view filter overrides.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        Expression GetExpressionWithOverrides( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, DataViewFilterOverrides dataViewFilterOverrides, string selection );
    }
}
