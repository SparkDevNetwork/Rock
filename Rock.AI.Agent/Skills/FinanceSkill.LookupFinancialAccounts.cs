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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Classes.Entity;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal partial class FinanceSkill
{
    #region Tool(s)

    [Description( "Lists all active financial accounts configured in the system." )]
    [AgentToolGuid( "4DBAE64C-A7B9-4826-90C0-8DE4AA598FFF" )]
    public AgentToolResult LookupFinancialAccounts()
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
