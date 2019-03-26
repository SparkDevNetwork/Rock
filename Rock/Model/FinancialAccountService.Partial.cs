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
using System.Linq;

namespace Rock.Model
{

    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.FinancialAccount"/> objects.
    /// </summary>
    public partial class FinancialAccountService
    {

        /// <summary>
        /// Gets immediate children of a account (id) or a rootGroupId. Specify 0 for both Id and rootGroupId to get top level accounts limited
        /// </summary>
        /// <param name="id">The ID of the account to get the children of (or 0 to use rootAccountId)</param>
        /// <param name="includeInactiveAccounts">if set to <c>true</c> [include inactive Accounts].</param>
        /// <returns></returns>
        public IQueryable<FinancialAccount> GetChildren( int id, bool includeInactiveAccounts )
        {
            var qry = Queryable();

            if ( id == 0 )
            {
                qry = qry.Where( a => a.ParentAccountId == null );
            }
            else
            {
                qry = qry.Where( a => a.ParentAccountId == id );
            }

            if ( !includeInactiveAccounts )
            {
                qry = qry.Where( a => a.IsActive );
            }

            return qry;
        }

        /// <summary>
        /// Returns an enumerable collection of the <see cref="Rock.Model.FinancialAccount" /> Ids that are ancestors of a specified accountId sorted starting with the most immediate parent
        /// </summary>
        /// <param name="accountId">The account identifier.</param>
        /// <returns></returns>
        public IOrderedEnumerable<int> GetAllAncestorIds( int accountId )
        {
            var result = this.Context.Database.SqlQuery<int>(
                @"
                with CTE as (
                select *, 0 as [Level] from [FinancialAccount] where [Id]={0}
                union all
                select [a].*, [Level] + 1 as [Level] from [FinancialAccount] [a]
                inner join CTE pcte on pcte.ParentAccountId = [a].[Id]
                )
                select Id from CTE where Id != {0} order by Level
                ", accountId );

            // already ordered within the sql, so do a dummy order by to get IOrderedEnumerable
            return result.OrderBy( a => 0 );
        }

        /// <summary>
        /// Gets the entire FinancialAccount tree ordered by parent to child recursively
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<FinancialAccount> GetTree()
        {
            // The Path field creates a dot seperated list of parent to child Ids, starting with top level and ending with the row's ID.
            // This allows the rows to be ordered by the nested hierarchy.
            string qry = @"WITH cte AS (
                SELECT parent.*
		            , 0 AS [Level]
		            , RIGHT(REPLICATE('0', 5) + CAST(ROW_NUMBER() OVER (ORDER BY parent.[Order],parent.[Name]) AS varchar(max)), 5 ) AS [Path]
                FROM [FinancialAccount] parent
                WHERE parent.[ParentAccountId] IS NULL

                UNION ALL

                SELECT child.*
		            , [Level] + 1
		            , [Path] + '.' +  RIGHT(REPLICATE('0', 5) + CAST(ROW_NUMBER() OVER (ORDER BY child.[Order],child.[Name]) AS varchar(max)), 5)  AS [Path]
                FROM [FinancialAccount] child
                INNER JOIN cte ON cte.[Id] = child.[ParentAccountId]
            )

            SELECT [Id]
                , [ParentAccountId]
                , [CampusId]
                , [Name]
                , [PublicName]
                , [Description]
                , [IsTaxDeductible]
                , [GlCode]
                , [Order]
                , [IsActive]
                , [StartDate]
                , [EndDate]
                , [AccountTypeValueId]
                , [Guid]
                , [CreatedDateTime]
                , [ModifiedDateTime]
                , [CreatedByPersonAliasId]
                , [ModifiedByPersonAliasId]
                , [ForeignKey]
                , [ImageBinaryFileId]
                , [Url]
                , [PublicDescription]
                , [IsPublic]
                , [ForeignGuid]
                , [ForeignId]
            FROM cte ORDER BY [Path]";

            // already ordered within the sql, so do a dummy order by to get IOrderedEnumerable
            return Context.Database.SqlQuery<FinancialAccount>( qry ).OrderBy( a => 0 );

        }
    }
}
