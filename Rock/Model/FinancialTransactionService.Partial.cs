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

        /// <summary>
        /// Gets the transaction by search.
        /// </summary>
        /// <param name="searchValue">The search value.</param>
        /// <returns></returns>
        public IQueryable<FinancialTransaction> Get(TransactionSearchValue searchValue)
        {
            var transactions = Repository.AsQueryable();

            if ( searchValue.AmountRange.From.HasValue )
            {
                transactions = transactions.Where(t => t.Amount >= searchValue.AmountRange.From); 
            }
            if (searchValue.AmountRange.To.HasValue)
            {
                transactions = transactions.Where(t => t.Amount <= searchValue.AmountRange.To); 
            }
            if ( searchValue.DateRange.From.HasValue )
            {
                transactions = transactions.Where( t => t.TransactionDateTime >= searchValue.DateRange.From.Value );
            }
            if ( searchValue.DateRange.To.HasValue )
            {
                transactions = transactions.Where( t => t.TransactionDateTime <= searchValue.DateRange.To.Value );
            }
            if ( searchValue.TransactionTypeValueId.HasValue )
            {
                transactions = transactions.Where( t => t.TransactionTypeValueId == searchValue.TransactionTypeValueId.Value );
            }
            if ( searchValue.CurrencyTypeValueId.HasValue )
            {
                transactions = transactions.Where( t => t.CurrencyTypeValueId.HasValue && t.CurrencyTypeValueId == searchValue.CurrencyTypeValueId.Value );
            }
            if ( searchValue.CreditCardTypeValueId.HasValue )
            {
                transactions = transactions.Where(t => t.CreditCardTypeValueId.HasValue && t.CurrencyTypeValueId == searchValue.CreditCardTypeValueId.Value);
            }
            if (searchValue.SourceTypeValueId.HasValue)
            {
                transactions = transactions.Where(t => t.SourceTypeValueId.HasValue && t.SourceTypeValueId == searchValue.SourceTypeValueId.Value);
            }
            if (!String.IsNullOrEmpty(searchValue.TransactionCode))
            {
                transactions = transactions.Where(t => t.TransactionCode == searchValue.TransactionCode);
            }
            if ( searchValue.AccountId.HasValue )
            {
                transactions = transactions.Where( t => t.TransactionDetails.Any( d => d.AccountId == searchValue.AccountId.Value ) );
            }
            return transactions;
        }
    }
}