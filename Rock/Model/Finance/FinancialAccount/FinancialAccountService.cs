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
using System.Collections.Generic;
using System.Linq;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.FinancialAccount"/> objects.
    /// </summary>
    public partial class FinancialAccountService
    {
        /// <summary>
        /// Enum AccountHierarchyDirection
        /// </summary>
        public enum AccountHierarchyDirection
        {
            /// <summary>
            /// The current account to parent
            /// </summary>
            CurrentAccountToParent,
            /// <summary>
            /// The parent account to last descendant account
            /// </summary>
            ParentAccountToLastDescendantAccount
        }

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
            var result = FinancialAccountCache.Get( accountId )?.GetAncestorFinancialAccountIds() ?? new int[0];

            // already ordered GetAncestorFinancialAccountIds, so do a dummy order by to get IOrderedEnumerable
            return result.OrderBy( a => 0 );
        }

        /// <summary>
        /// Gets all descendent ids.
        /// </summary>
        /// <param name="parentAccountId">The parent account identifier.</param>
        /// <returns>System.Collections.Generic.IEnumerable&lt;int&gt;.</returns>
        public IEnumerable<int> GetAllDescendentIds( int parentAccountId )
        {
            return FinancialAccountCache.Get( parentAccountId )?.GetDescendentFinancialAccountIds() ?? new int[0];
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
                , [ShowInGivingOverview]
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

        /// <summary>
        /// Returns a Queryable of <see cref="PersonAlias" /> of the Participants for the specified <see cref="FinancialAccount" /> and purpose key.
        /// </summary>
        /// <param name="financialAccountId">The financial account identifier.</param>
        /// <param name="purposeKey">The purpose key.</param>
        /// <returns>IQueryable&lt;Person&gt;.</returns>
        public IQueryable<PersonAlias> GetAccountParticipants( int financialAccountId, string purposeKey )
        {
            var accountParticipantsQuery = this.RelatedEntities.GetRelatedToSourceEntity<PersonAlias>( financialAccountId, purposeKey );
            return accountParticipantsQuery;
        }

        /// <summary>
        /// Gets the account participants and Purpose
        /// </summary>
        /// <param name="financialAccountId">The financial account identifier.</param>
        /// <returns>IQueryable&lt;PersonAlias&gt;.</returns>
        public IQueryable<PersonAliasAndPurposeKey> GetAccountParticipantsAndPurpose( int financialAccountId )
        {
            var purposeKeys = this.RelatedEntities.GetUsedPurposeKeys( financialAccountId ).ToList();

            // get a query of participants where no purpose is specified
            IQueryable<PersonAliasAndPurposeKey> accountParticipantsQuery = this.RelatedEntities.GetRelatedToSourceEntity<PersonAlias>( financialAccountId, null ).Select( s => new PersonAliasAndPurposeKey
            {
                PersonAlias = s,
                PurposeKey = null
            } );

            // union with query of participants with all other purposes
            foreach ( var purposeKey in purposeKeys )
            {
                var accountParticipantsQueryForPurpose = this.RelatedEntities.GetRelatedToSourceEntity<PersonAlias>( financialAccountId, purposeKey ).Select( s => new PersonAliasAndPurposeKey
                {
                    PersonAlias = s,
                    PurposeKey = purposeKey
                } );

                accountParticipantsQuery = accountParticipantsQuery.Union( accountParticipantsQueryForPurpose );
            }

            return accountParticipantsQuery;
        }

        /// <summary>
        /// Gets the accounts by search term.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <param name="includeAll">if set to <c>true</c> [include all].</param>
        /// <returns>IQueryable&lt;FinancialAccount&gt;.</returns>
        public IQueryable<FinancialAccount> GetAccountsBySearchTerm( string searchTerm, bool includeAll = false )
        {
            var qry = Queryable();
            
            if( string.IsNullOrEmpty( searchTerm ) && !includeAll){
                return null;
            }

            qry = qry
                .Where( f =>
                ( f.Name != null && f.Name.Contains( searchTerm ) )
                || ( f.PublicName != null && f.PublicName.Contains( searchTerm ) )
                || ( f.GlCode != null && f.GlCode.Contains( searchTerm ) )
                );

            return qry;
        }

        /// <summary>
        /// Populates the path variable for <see cref="Rock.Web.UI.Controls.AccountTreeViewItem"/>s using the fewest database queries necessary.
        /// </summary>
        /// <param name="accountTreeViewItems">The account TreeView items.</param>
        /// <param name="accountList">The account list.</param>
        /// <returns>List&lt;Rock.Web.UI.Controls.AccountTreeViewItem&gt;.</returns>
        public List<Rock.Web.UI.Controls.AccountTreeViewItem> GetTreeviewPaths( List<Rock.Web.UI.Controls.AccountTreeViewItem> accountTreeViewItems, List<FinancialAccount> accountList )
        {
            var accountPaths = new Dictionary<string, string>();
            foreach ( var accountTreeViewItem in accountTreeViewItems )
            {
                if ( !accountPaths.ContainsKey( accountTreeViewItem.ParentId) )
                {
                    var account = accountList.Where ( a => a.Id.ToString() == accountTreeViewItem.Id ).FirstOrDefault();
                    accountPaths.Add( accountTreeViewItem.ParentId,
                        this.GetDelimitedAccountHierarchy( account, FinancialAccountService.AccountHierarchyDirection.CurrentAccountToParent) );
                }

                accountTreeViewItem.Path = accountPaths[accountTreeViewItem.ParentId];
            }

            return accountTreeViewItems;
        }

        /// <summary>
        /// Gets the account hierarchy path as a '^' delimited string..
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="accountHierarchyDirection">The account hierarchy direction.</param>
        /// <returns>System.String.</returns>
        public string GetDelimitedAccountHierarchy( FinancialAccount account, AccountHierarchyDirection accountHierarchyDirection = AccountHierarchyDirection.ParentAccountToLastDescendantAccount )
        {
            var selectedAccounts = new List<SimpleFinancialAccount>();

            var relatedIds = accountHierarchyDirection == AccountHierarchyDirection.CurrentAccountToParent
                ? GetAllAncestorIds( account.Id )
                : GetAllDescendentIds( account.Id );

            var parentAccounts = GetParents( relatedIds?.ToArray() )?.ToList();

            selectedAccounts = parentAccounts.Select( v => new SimpleFinancialAccount {
                Id = v.Id,
                Name = System.Net.WebUtility.HtmlEncode( v.PublicName.IsNotNullOrWhiteSpace() ? v.PublicName : v.Name ),
                GlCode = v.GlCode,
                IsActive = v.IsActive,
                ParentId = v.ParentAccountId
            } )?.ToList();

            return selectedAccounts?.Select( v => v.Name ).JoinStrings( "^" );
        }

        /// <summary>
        /// Class PersonAliasAndPurposeKey.
        /// </summary>
        public class PersonAliasAndPurposeKey
        {
            /// <summary>
            /// Gets or sets the person alias.
            /// </summary>
            /// <value>The person alias.</value>
            public PersonAlias PersonAlias { get; set; }

            /// <summary>
            /// Gets or sets the purpose key.
            /// </summary>
            /// <value>The purpose key.</value>
            public string PurposeKey { get; set; }
        }

        /// <summary>
        /// Class SimpleFinancialAccount.
        /// </summary>
        public class SimpleFinancialAccount
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>The identifier.</value>
            public int Id { get; set; }
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; set; }
            /// <summary>
            /// Gets or sets the gl code.
            /// </summary>
            /// <value>The gl code.</value>
            public string GlCode { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this instance is active.
            /// </summary>
            /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
            public bool IsActive { get; set; }
            /// <summary>
            /// Gets or sets the parent identifier.
            /// </summary>
            /// <value>The parent identifier.</value>
            public int? ParentId { get; set; }
        }

        /// <summary>
        /// Sets the participants (<see cref="PersonAlias" />) for the specified  <see cref="FinancialAccount" /> and purpose key
        /// </summary>
        /// <param name="financialAccountId">The financial account identifier.</param>
        /// <param name="givingAlertParticipants">The giving alert participants.</param>
        /// <param name="purposeKey">The purpose key.</param>
        public void SetAccountParticipants( int financialAccountId, List<PersonAlias> givingAlertParticipants, string purposeKey )
        {
            this.RelatedEntities.SetRelatedToSourceEntity( financialAccountId, givingAlertParticipants, purposeKey );
        }

        #region Help Methods

        private IQueryable<FinancialAccount> GetParents( IEnumerable<int> parentIds )
        {
            return Queryable()
                       .Where( v => parentIds.Contains(v.Id) );
        }
        #endregion Help Methods
    }
}
