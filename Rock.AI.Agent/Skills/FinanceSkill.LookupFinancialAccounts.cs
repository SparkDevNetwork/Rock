using System.Collections.Generic;
using System.Linq;

using Rock.AI.Agent.Classes.Entity;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    internal partial class FinanceSkill
    {
        #region Tool(s)

        /// <summary>
        /// Returns active financial accounts (funds) that can be used for filtering or selection in other tools.
        /// </summary>
        /// <returns>A <see cref="RockToolResult"/> containing the accounts or <c>NoData</c> if none.</returns>
        [AgentToolGuid( "4DBAE64C-A7B9-4826-90C0-8DE4AA598FFF" )]
        public IAgentToolResult LookupFinancialAccounts()
        {
            // Load all top-level active accounts.
            var topLevelAccounts = FinancialAccountCache
                .All( AgentRequestContext.RockContext )
                .Where( a => a.IsActive && a.ParentAccountId == null );

            // Build hierarchical tree.
            var parentAccountResults = new List<FinancialAccountResult>();

            foreach ( var acct in topLevelAccounts )
            {
                var result = new FinancialAccountResult
                {
                    Id = acct.Id,
                    IsTaxDeductible = acct.IsTaxDeductible,
                    Name = acct.PublicName,
                    PublicDescription = acct.PublicDescription,
                    Campus = acct.CampusId.HasValue ? new CampusResult
                    {
                        Id = acct.CampusId.Value,
                        Name = acct.Campus.Name
                    } : null
                };

                result.Children = new List<FinancialAccountResult>();
                var childAccts = acct.GetDescendentFinancialAccounts()
                    .Where( childAcct => childAcct.IsActive );

                foreach ( var childAcct in childAccts )
                {
                    if ( result.Children.Any( c => c.Id == childAcct.Id ) )
                    {
                        continue;
                    }

                    result.Children.Add( new FinancialAccountResult
                    {
                        Id = childAcct.Id,
                        IsTaxDeductible = childAcct.IsTaxDeductible,
                        Name = childAcct.PublicName,
                        PublicDescription = childAcct.PublicDescription,
                        ParentAccountIdKey = IdHasher.Instance.GetHash( childAcct.ParentAccountId ?? 0 ),
                        Campus = childAcct.CampusId.HasValue ? new CampusResult
                        {
                            Id = childAcct.CampusId.Value,
                            Name = childAcct.Campus.Name
                        } : null
                    } );
                }

                parentAccountResults.Add( result );
            }

            // Flatten the tree for history (a single list of all accounts + children).
            if ( !parentAccountResults.Any() )
            {
                return NoData();
            }

            var trimmedForHistory = new List<object>();

            foreach ( var parent in parentAccountResults )
            {
                trimmedForHistory.Add( new
                {
                    parent.IdKey,
                    parent.Name,
                    parent.IsTaxDeductible,
                    parent.PublicDescription,
                } );

                foreach ( var child in parent.Children )
                {
                    trimmedForHistory.Add( new
                    {
                        child.IdKey,
                        child.Name,
                        child.IsTaxDeductible,
                        child.PublicDescription,
                        child.ParentAccountIdKey
                    } );
                }
            }

            return Success( parentAccountResults )
                .WithHistoryContent( trimmedForHistory, "financial-accounts" );
        }

        #endregion
    }
}
