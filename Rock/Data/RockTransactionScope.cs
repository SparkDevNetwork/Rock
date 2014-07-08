// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
            TransactionOptions transactionOptions = new TransactionOptions();
            transactionOptions.IsolationLevel = IsolationLevel.ReadCommitted;

            using ( var t = new TransactionScope( TransactionScopeOption.Required, transactionOptions ) )
            {
                action.Invoke();
                t.Complete();
            }
        }

    }
}