﻿//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Rock.Financial
{
    /// <summary>
    /// Service class for Transaction objects.
    /// </summary>
    public partial class TransactionService : Service<Transaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        public TransactionService() : base()
        {
        }

        public IEnumerable<Transaction> GetAllTransactions()
        {
           return Repository.GetAll();
        }

        public IEnumerable<Transaction> GetTransactionBySearch(TransactionSearchValue searchValue)
        {
            var transactions = Repository.GetAll();
            if (searchValue.AmountRange.From.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.Amount >= searchValue.AmountRange.From); 
            }
            if (searchValue.AmountRange.To.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.Amount <= searchValue.AmountRange.To); 
            }
            if (searchValue.CreditCardType != null)
            {
                transactions = transactions.Where(transaction => transaction.CreditCardType.Id == searchValue.CreditCardType.Id);
            }
            if (searchValue.CurrencyType != null)
            {
                transactions = transactions.Where(transaction => transaction.CurrencyType.Id == searchValue.CurrencyType.Id);
            }
            if (searchValue.DateRange.From.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.TransactionDate >= searchValue.DateRange.From.Value);
            }
            if (searchValue.DateRange.To.HasValue)
            {
                transactions = transactions.Where(transaction => transaction.TransactionDate <= searchValue.DateRange.To.Value);
            }
            if (searchValue.Fund != null)
            {
                transactions = transactions.Where(transaction => transaction.TransactionFunds.Any(transactionFund => transactionFund.Fund.Id == searchValue.Fund.Id));
            }
            if (searchValue.SourceType != null)
            {
                transactions = transactions.Where(transaction => transaction.SourceTypeId == searchValue.SourceType.Id);
            }
            if (!String.IsNullOrEmpty(searchValue.TransactionCode))
            {
                transactions = transactions.Where(transaction => transaction.TransactionCode == searchValue.TransactionCode);
            }
            return transactions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public TransactionService(IRepository<Transaction> repository) : base(repository)
        {
        }
    }
}