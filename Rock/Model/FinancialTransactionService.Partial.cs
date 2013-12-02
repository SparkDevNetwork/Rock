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
    /// Service/Data access class for <see cref="Rock.Model.FinancialTransaction"/> entity objects.
    /// </summary>
    public partial class FinancialTransactionService 
    {
        /// <summary>
        /// Gets a transaction by it's transaction code.
        /// </summary>
        /// <param name="transactionCode">A <see cref="System.String"/> representing the transaction code for the transaction</param>
        /// <returns>The <see cref="Rock.Model.FinancialTransaction"/> that matches the transaction code, this value will be null if a match is not found.</returns>
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