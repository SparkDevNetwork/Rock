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
    /// Service class for Batch objects.
    /// </summary>  
    public partial class FinancialBatchService 
    {
        /// <summary>
        /// Gets the Batch by search.
        /// </summary>
        /// <param name="searchValue">The search value.</param>
        /// <returns></returns>
        public IQueryable<FinancialBatch> Get(BatchSearchValue searchValue)
        {
            var batches = Repository.AsQueryable();
           if (searchValue.DateRange.From.HasValue)
            {
                batches = batches.Where(batch => batch.BatchDate >= searchValue.DateRange.From.Value);
            }
            if (searchValue.DateRange.To.HasValue)
            {
                batches = batches.Where(batch => batch.BatchDate <= searchValue.DateRange.To.Value);
            }
            if (searchValue.BatchTypeValueId.HasValue)
            {
                //need this
              //  batches = batches.Where(batch => batch.CreditCardTypeValue.Id == searchValue.batchTypeValueId.Value);
            }
            
            if (searchValue.IsClosed.HasValue)
            {
                batches = batches.Where(Batch => Batch.IsClosed == searchValue.IsClosed.Value);
            }

            if ( !string.IsNullOrEmpty(searchValue.Title) )
            {
                batches = batches.Where(Batch => Batch.Name.Any());
            }
            
            return batches;
        }
    }
}