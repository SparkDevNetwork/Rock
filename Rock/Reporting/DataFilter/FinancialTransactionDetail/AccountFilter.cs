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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Model;

namespace Rock.Reporting.DataFilter.FinancialTransactionDetail
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Transaction Detail by Account" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Account Filter" )]
    public class AccountFilter : BaseAccountFilter<Rock.Model.FinancialTransactionDetail>
    {
        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression GetExpression( Type entityType, Data.IService serviceInstance, System.Linq.Expressions.ParameterExpression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 1 )
            {
                var accountGuids = selectionValues[0].Split( ',' ).Select( a => a.AsGuid() ).ToList();
                var accountIds = new FinancialAccountService( (RockContext)serviceInstance.Context ).GetByGuids( accountGuids ).Select( a => a.Id ).ToList();

                var qry = new FinancialTransactionDetailService( (RockContext)serviceInstance.Context ).Queryable()
                    .Where( p => accountIds.Contains( p.AccountId ) );

                Expression extractedFilterExpression = FilterExpressionExtractor.Extract<Rock.Model.FinancialTransactionDetail>( qry, parameterExpression, "p" );

                return extractedFilterExpression;
            }

            return null;
        }
    }
}
