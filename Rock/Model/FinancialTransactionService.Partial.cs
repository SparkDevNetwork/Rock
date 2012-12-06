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
    public partial class FinancialTransactionService : Service<FinancialTransaction, FinancialTransactionDto>
    {
        /// <summary>
        /// Gets the transaction by search.
        /// </summary>
        /// <param name="searchValue">The search value.</param>
        /// <returns></returns>
        public IQueryable<FinancialTransaction> Get(TransactionSearchValue searchValue)
        {
            var transactions = Repository.AsQueryable();

            if (searchValue.AmountRange.From.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.Amount >= searchValue.AmountRange.From); 
            }
            if (searchValue.AmountRange.To.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.Amount <= searchValue.AmountRange.To); 
            }
            if (searchValue.CreditCardTypeValueId.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.CreditCardTypeValue.Id == searchValue.CreditCardTypeValueId.Value);
            }
            if (searchValue.CurrencyTypeValueId.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.CurrencyTypeValue.Id == searchValue.CurrencyTypeValueId.Value);
            }
            if (searchValue.DateRange.From.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.TransactionDateTime >= searchValue.DateRange.From.Value);
            }
            if (searchValue.DateRange.To.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.TransactionDateTime <= searchValue.DateRange.To.Value);
            }
            if (searchValue.FundId.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.TransactionFunds.Any(transactionFund => transactionFund.Fund.Id == searchValue.FundId.Value));
            }
            if (searchValue.SourceTypeValueId.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.SourceTypeValueId == searchValue.SourceTypeValueId.Value);
            }
            if (!String.IsNullOrEmpty(searchValue.TransactionCode))
            {
                transactions = transactions.Where(transaction => transaction.TransactionCode == searchValue.TransactionCode);
            }
            return transactions;
        }
    }
}