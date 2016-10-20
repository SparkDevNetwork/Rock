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
    public class RetirementFundService : Service<RetirementFund>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetirementFundService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public RetirementFundService( RockContext context ) : base( context ) { }
    }

    public static partial class RetirementFundExtensionMethods
    {
        public static RetirementFund Clone( this RetirementFund source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as RetirementFund;
            }
            else
            {
                var target = new RetirementFund();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        public static void CopyPropertiesFrom( this RetirementFund target, RetirementFund source )
        {
            target.Id = source.Id;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.FundValueId = source.FundValueId;
            target.IsFixedAmount = source.IsFixedAmount;
            target.EmployeeAmount = source.EmployeeAmount;
            target.EmployerAmount = source.EmployerAmount;
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

