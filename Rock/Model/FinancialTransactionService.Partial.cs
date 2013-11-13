//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service class for Transaction objects.
    /// </summary>
    public partial class FinancialTransactionService 
    {
        /// <summary>
        /// Gets the transaction by its TransactionCode.
        /// </summary>
        /// <param name="transactionCode">The transaction code.</param>
        /// <returns></returns>
        public FinancialTransaction GetByTransactionCode( string transactionCode )
        {
            if ( !string.IsNullOrWhiteSpace( transactionCode ) )
            {
                return Repository.AsQueryable()
                    .Where( t => t.TransactionCode.Equals( transactionCode.Trim(), StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();
            }
            return null;
        }
    }
}