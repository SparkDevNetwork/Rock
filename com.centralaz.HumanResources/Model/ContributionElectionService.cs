using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace com.centralaz.HumanResources.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class ContributionElectionService : Service<ContributionElection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContributionElectionService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public ContributionElectionService( RockContext context ) : base( context ) { }
    }

    public static partial class ContributionElectionExtensionMethods
    {
        public static ContributionElection Clone( this ContributionElection source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as ContributionElection;
            }
            else
            {
                var target = new ContributionElection();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        public static void CopyPropertiesFrom( this ContributionElection target, ContributionElection source )
        {
            target.Id = source.Id;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.FinancialAccountId = source.FinancialAccountId;
            target.IsFixedAmount = source.IsFixedAmount;
            target.Amount = source.Amount;
            target.ActiveDate = source.ActiveDate;
            target.InactiveDate = source.InactiveDate;
            target.IsActive = source.IsActive;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
        }
    }
}

