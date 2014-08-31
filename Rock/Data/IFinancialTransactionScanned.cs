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

namespace Rock.Data
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFinancialTransactionScanned
    {
        /// <summary>
        /// Gets or sets the batch identifier.
        /// </summary>
        /// <value>
        /// The batch identifier.
        /// </value>
        int? BatchId { get; set; }
        
        /// <summary>
        /// Gets or sets the credit card type value identifier.
        /// </summary>
        /// <value>
        /// The credit card type value identifier.
        /// </value>
        int? CreditCardTypeValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the currency type value identifier.
        /// </summary>
        /// <value>
        /// The currency type value identifier.
        /// </value>
        int? CurrencyTypeValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the source type value identifier.
        /// </summary>
        /// <value>
        /// The source type value identifier.
        /// </value>
        int? SourceTypeValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        DateTime? TransactionDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction type value identifier.
        /// </summary>
        /// <value>
        /// The transaction type value identifier.
        /// </value>
        int TransactionTypeValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        string Summary { get; set; }
        
        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        string TransactionCode { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int Id { get; set; }
        
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        Guid Guid { get; set; }
        
        /// <summary>
        /// Gets or sets the authorized person identifier.
        /// </summary>
        /// <value>
        /// The authorized person identifier.
        /// </value>
        int? AuthorizedPersonId { get; set; }
        
        /// <summary>
        /// Gets or sets the check micr encrypted.
        /// </summary>
        /// <value>
        /// The check micr encrypted.
        /// </value>
        string CheckMicrEncrypted { get; set; }
    }
}
