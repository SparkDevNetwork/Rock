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
    public class SalaryService : Service<Salary>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SalaryService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public SalaryService( RockContext context ) : base( context ) { }
    }

    public static partial class SalaryExtensionMethods
    {
        public static Salary Clone( this Salary source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as Salary;
            }
            else
            {
                var target = new Salary();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        public static void CopyPropertiesFrom( this Salary target, Salary source )
        {
            target.Id = source.Id;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
            target.Amount = source.Amount;
            target.IsSalariedEmployee = source.IsSalariedEmployee;
            target.HousingAllowance = source.HousingAllowance;
            target.FuelAllowance = source.FuelAllowance;
            target.PhoneAllowance = source.PhoneAllowance;
            target.EffectiveDate = source.EffectiveDate;
            target.ReviewedDate = source.ReviewedDate;
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

