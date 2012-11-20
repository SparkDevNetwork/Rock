//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace Rock.Data
{
    /// <summary>
    /// Use to wrap a block in a StartTransaction/Commit/Rollback
    /// </summary>
    public static class RockTransactionScope
    {
        /// <summary>
        /// Executes a block of code within a Start/Commit/Rollback
        /// </summary>
        /// <example>  
        /// <code> 
        ///  RockTransactionScope.WrapTransaction( () =>
        ///  {
        ///      marketingCampaignAdTypeService.Save( marketingCampaignAdType, CurrentPersonId );
        ///      foreach (var attribute in attributes)
        ///      {
        ///        attributeService.Save( attribute, CurrentPersonId );
        ///      };  
        ///  });    
        /// </code> 
        /// </example>
        /// <param name="action">A.</param>
        public static void WrapTransaction( Action action )
        {
            // By default, the System.Transactions infrastructure creates Serializable transactions
            // use ReadCommitted instead (which is the default for normal SQL Server operations)
            // NOTE:  It might be a good idea to turn on Read Committed Snapshot in SQL Server. 
            TransactionScopeOption transactionScopeOption = new TransactionScopeOption();
            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;

            using ( var t = new TransactionScope(transactionScopeOption, transactionOptions))
            {
                action.Invoke();
                t.Complete();
            }
        }

    }
}