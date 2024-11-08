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
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Integration.Modules.Reporting.DataFilter
{
    /// <summary>
    /// Provides base functionality for a set of DataFilter tests.
    /// </summary>
    public abstract class DataFilterTestBase : DatabaseTestsBase
    {
        /// <summary>
        /// Create a query that returns entities of a specific type, filtered by a predicate expression.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataContext"></param>
        /// <param name="whereExpression"></param>
        /// <param name="paramExpression"></param>
        /// <returns></returns>
        protected IQueryable<T> GetFilteredEntityQuery<T>( RockContext dataContext, Expression whereExpression, ParameterExpression paramExpression )
            where T : IEntity
        {
            var serviceInstance = Rock.Reflection.GetServiceForEntityType( typeof( T ), dataContext );

            var getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ) } );

            if ( getMethod != null )
            {
                var getResult = getMethod.Invoke( serviceInstance, new object[] { paramExpression, whereExpression } );

                var qry = getResult as IQueryable<T>;

                return qry;
            }

            return null;
        }
    }
}
