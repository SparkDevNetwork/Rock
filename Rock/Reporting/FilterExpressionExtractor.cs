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
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Rock.Model;

namespace Rock.Reporting
{
    /// <summary>
    /// Helper class than can extract the "Where" clause Expression from an IQueryable
    /// </summary>
    public static class FilterExpressionExtractor
    {
        /// <summary>
        /// Extracts the "Where" clause Expression from an IQueryable
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="parameterExpression">The original parameter expression.</param>
        /// <param name="parameterName">Name of the parameter (forexample: 'p') from the qry to replace with the parameterExpression.</param>
        /// <returns></returns>
        public static Expression Extract<T>( IQueryable qry, ParameterExpression parameterExpression, string parameterName )
        {
            /* 05/25/2022 MDP

            This will exact the WHERE clause of a queryable. This is used by DataFilters to get the expression that will get appended with all
            other datafilters for a DataView.

            FilterExpressionExtractor is especially handy for DataFilters so that they can be written using a normal Queryable, instead of manually creating
             linq expressions.

            This is how we had to do this *before* FilterExpressionExtractor!
            see https://github.com/SparkDevNetwork/Rock/blob/7410d20dd1d8a982ffa4695b9656f2147d8cb1ce/Rock/Reporting/DataSelect/Person/FirstLastContributionSelect.cs#L208.

            This works by finding the Arguments[1] of the datafilter's queryable. That ends up being the WHERE clause that the DataFilter
            wants to return.

            Example:

            Let's say a develop writes a GroupMember Count datafilter. They would write it like this:

            int memberCount = configuration.MemberCount;
            var groupQuery = new GroupService( (RockContext)serviceInstance.Context ).Queryable();
            var memberCountQuery = groupQuery.Where( a => a.Members.Count() > memberCount );

            Linq breaks this down to an expression where the args are like this:

               groupQuery // Arguments[0]

               Where(a => a.Members.Count() > memberCount) // Arguments[1]

            Special Note: In the above example, GroupService.Queryable has special code in it, but we don't want that to be involved
            in the DataFilter Expression. The DataView engine will be taking care of stitching that part in later.

            */

            MethodCallExpression methodCallExpression = qry.Expression as MethodCallExpression;

            var whereClause = methodCallExpression.Arguments[1];
            Expression<Func<LambdaExpression>> executionLambda = Expression.Lambda<Func<LambdaExpression>>( whereClause );
            Expression extractedExpression = ( executionLambda.Compile().Invoke() as Expression<Func<T, bool>> ).Body;

            return extractedExpression.ReplaceParameter( parameterName, parameterExpression );
        }

        /// <summary>
        /// Alters the type of the comparison.
        /// </summary>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <param name="compareEqualExpression">The compare equal expression.</param>
        /// <param name="blankValue">the value to compare equal to for IsBlank/IsNotBlank.</param>
        /// <returns></returns>
        public static BinaryExpression AlterComparisonType( ComparisonType comparisonType, BinaryExpression compareEqualExpression, object blankValue = null )
        {
            BinaryExpression result = compareEqualExpression;

            switch ( comparisonType )
            {
                case ComparisonType.EqualTo:
                    result = Expression.MakeBinary( ExpressionType.Equal, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.GreaterThan:
                    result = Expression.MakeBinary( ExpressionType.GreaterThan, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.GreaterThanOrEqualTo:
                    result = Expression.MakeBinary( ExpressionType.GreaterThanOrEqual, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.IsBlank:
                    result = Expression.MakeBinary( ExpressionType.Equal, compareEqualExpression.Left, Expression.Constant( blankValue, compareEqualExpression.Right.Type ) );
                    break;
                case ComparisonType.IsNotBlank:
                    result = Expression.MakeBinary( ExpressionType.NotEqual, compareEqualExpression.Left, Expression.Constant( blankValue, compareEqualExpression.Right.Type ) );
                    break;
                case ComparisonType.LessThan:
                    result = Expression.MakeBinary( ExpressionType.LessThan, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.LessThanOrEqualTo:
                    result = Expression.MakeBinary( ExpressionType.LessThanOrEqual, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
                case ComparisonType.NotEqualTo:
                    result = Expression.MakeBinary( ExpressionType.NotEqual, compareEqualExpression.Left, compareEqualExpression.Right );
                    break;
            }

            return result;
        }
    }
}
