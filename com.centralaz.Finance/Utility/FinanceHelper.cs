// <copyright>
// Copyright by Central Christian Church
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Data;

namespace com.centralaz.Finance.Utility
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FinanceHelper
    {
        public static Dictionary<string, object> GetFinancialStatementTransactionsAndAddresses( Dictionary<string, object> parameters )
        {
            List<TransactionSummary> transactionList = new List<TransactionSummary>();
            List<AddressSummary> addressList = new List<AddressSummary>();
            var result = DbService.GetDataSet( "_com_centralaz_Finance_GetGivingGroupTransactions", System.Data.CommandType.StoredProcedure, parameters );

            if ( result.Tables.Count > 1 )
            {
                var transactionDataTable = result.Tables[0];
                foreach ( var row in transactionDataTable.Rows.OfType<DataRow>().ToList() )
                {
                    transactionList.Add( new TransactionSummary
                    {
                        GivingId = row.ItemArray[0] as string,
                        TransactionCode = row.ItemArray[1] as string,
                        TransactionDateTime = row.ItemArray[2] as DateTime?,
                        Account1Amount = row.ItemArray[3] as decimal?,
                        Account2Amount = row.ItemArray[4] as decimal?,
                        Account3Amount = row.ItemArray[5] as decimal?,
                        Account4Amount = row.ItemArray[6] as decimal?,
                        OtherAmount = row.ItemArray[7] as decimal?,
                        TotalTransactionAmount = row.ItemArray[8] as decimal?
                    } );
                }

                var addressDataTable = result.Tables[1];
                foreach ( var row in addressDataTable.Rows.OfType<DataRow>().ToList() )
                {
                    addressList.Add( new AddressSummary
                    {
                        GivingId = row.ItemArray[0] as string,
                        AddressNames = row.ItemArray[1] as string,
                        GreetingNames = row.ItemArray[2] as string,
                        Street1 = row.ItemArray[4] as string,
                        Street2 = row.ItemArray[5] as string,
                        City = row.ItemArray[6] as string,
                        State = row.ItemArray[7] as string,
                        PostalCode = row.ItemArray[8] as string
                    } );
                }
            }

            var givingTransactionList = transactionList.GroupBy( t => t.GivingId ).Select( g => new GroupedTransaction
            {
                Key = g.Key,
                Account1Total = g.Sum( t => t.Account1Amount ),
                Account2Total = g.Sum( t => t.Account2Amount ),
                Account3Total = g.Sum( t => t.Account3Amount ),
                Account4Total = g.Sum( t => t.Account4Amount ),
                OtherTotal = g.Sum( t => t.OtherAmount ),
                TransactionTotal = g.Sum( t => t.TotalTransactionAmount ),
                Transactions = g.ToList()
            } ).ToList();

            // Dump everything into a lava object
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null );
            mergeFields.Add( "Transactions", givingTransactionList );
            mergeFields.Add( "Addresses", addressList );
            return mergeFields;
        }

        public static Dictionary<string, object> GetSqlParameters(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? account1Id = null,
            int? account2Id = null,
            int? account3Id = null,
            int? account4Id = null,
            int? excludedGroupId = null,
            List<int> campusIds = null,
            int? definedTypeAttributeId = null,
            List<int> definedValueIds = null,
            decimal? minimumContributionAmount = null,
            int? chapterNumber = null,
            int? chapterSize = null,
            bool isBusiness = false,
            bool returnTransactions = true,
            bool returnAddresses = true,
            string givingId = null )
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            if ( startDate.HasValue )
            {
                parameters.Add( "startDate", startDate.Value );
            }
            else
            {
                parameters.Add( "startDate", DateTime.MinValue );
            }

            if ( endDate.HasValue )
            {
                parameters.Add( "endDate", endDate.Value );
            }
            else
            {
                parameters.Add( "endDate", DateTime.MaxValue );
            }

            if ( account1Id != null )
            {
                parameters.Add( "account1Id", account1Id );
            }
            else
            {
                parameters.Add( "account1Id", DBNull.Value );
            }

            if ( account2Id != null )
            {
                parameters.Add( "account2Id", account2Id );
            }
            else
            {
                parameters.Add( "account2Id", DBNull.Value );
            }

            if ( account3Id != null )
            {
                parameters.Add( "account3Id", account3Id );
            }
            else
            {
                parameters.Add( "account3Id", DBNull.Value );
            }

            if ( account4Id != null )
            {
                parameters.Add( "account4Id", account4Id );
            }
            else
            {
                parameters.Add( "account4Id", DBNull.Value );
            }

            if ( excludedGroupId != null )
            {
                parameters.Add( "excludedGroupId", excludedGroupId );
            }
            else
            {
                parameters.Add( "excludedGroupId", DBNull.Value );
            }

            if ( campusIds != null )
            {
                parameters.Add( "campusList", campusIds.AsDelimited( "," ) );
            }
            else
            {
                parameters.Add( "campusList", DBNull.Value );
            }

            if ( definedTypeAttributeId != null )
            {
                parameters.Add( "definedTypeAttributeId", definedTypeAttributeId );
            }
            else
            {
                parameters.Add( "definedTypeAttributeId", DBNull.Value );
            }

            if ( definedValueIds != null )
            {
                parameters.Add( "definedValueList", definedValueIds.AsDelimited( "," ) );
            }
            else
            {
                parameters.Add( "definedValueList", DBNull.Value );
            }

            if ( minimumContributionAmount != null )
            {
                parameters.Add( "minimumAmount", minimumContributionAmount );
            }
            else
            {
                parameters.Add( "minimumAmount", DBNull.Value );
            }

            if ( chapterNumber != null )
            {
                parameters.Add( "chapterNumber", chapterNumber );
            }
            else
            {
                parameters.Add( "chapterNumber", DBNull.Value );
            }

            if ( chapterSize != null )
            {
                parameters.Add( "chapterSize", chapterSize );
            }
            else
            {
                parameters.Add( "chapterSize", DBNull.Value );
            }

            if ( givingId != null )
            {
                parameters.Add( "givingId", givingId );
            }
            else
            {
                parameters.Add( "givingId", DBNull.Value );
            }

            parameters.Add( "isBusiness", isBusiness );
            parameters.Add( "returnTransactions", returnTransactions );
            parameters.Add( "returnAddresses", returnAddresses );

            return parameters;
        }

        #region Helper Classes

        /// <summary>
        /// Grouped Transactions
        /// </summary>
        /// <seealso cref="Rock.Data.Entity{com.centralaz.Finance.Transactions.GenerateContributionStatementTransaction.GroupedTransaction}" />
        public class GroupedTransaction : Entity<GroupedTransaction>
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            [LavaInclude]
            public String Key { get; set; }

            /// <summary>
            /// Gets or sets the account1 total.
            /// </summary>
            /// <value>
            /// The account1 total.
            /// </value>
            [LavaInclude]
            public decimal? Account1Total { get; set; }

            /// <summary>
            /// Gets or sets the account2 total.
            /// </summary>
            /// <value>
            /// The account2 total.
            /// </value>
            [LavaInclude]
            public decimal? Account2Total { get; set; }

            /// <summary>
            /// Gets or sets the account3 total.
            /// </summary>
            /// <value>
            /// The account3 total.
            /// </value>
            [LavaInclude]
            public decimal? Account3Total { get; set; }

            /// <summary>
            /// Gets or sets the account4 total.
            /// </summary>
            /// <value>
            /// The account4 total.
            /// </value>
            [LavaInclude]
            public decimal? Account4Total { get; set; }

            /// <summary>
            /// Gets or sets the other total.
            /// </summary>
            /// <value>
            /// The other total.
            /// </value>
            [LavaInclude]
            public decimal? OtherTotal { get; set; }

            /// <summary>
            /// Gets or sets the transaction total.
            /// </summary>
            /// <value>
            /// The transaction total.
            /// </value>
            [LavaInclude]
            public decimal? TransactionTotal { get; set; }

            /// <summary>
            /// Gets or sets the transactions.
            /// </summary>
            /// <value>
            /// The transactions.
            /// </value>
            [LavaInclude]
            public List<TransactionSummary> Transactions { get; set; }

        }

        /// <summary>
        /// The transaction Summary
        /// </summary>
        [DotLiquid.LiquidType( "GivingId", "TransactionCode", "TransactionDateTime", "Account1Amount", "Account2Amount", "Account3Amount", "Account4Amount", "OtherAmount", "TotalTransactionAmount" )]
        public class TransactionSummary
        {
            /// <summary>
            /// Gets or sets the giving identifier.
            /// </summary>
            /// <value>
            /// The giving identifier.
            /// </value>
            public String GivingId { get; set; }

            /// <summary>
            /// Gets or sets the transaction code.
            /// </summary>
            /// <value>
            /// The transaction code.
            /// </value>
            public String TransactionCode { get; set; }

            /// <summary>
            /// Gets or sets the transaction date time.
            /// </summary>
            /// <value>
            /// The transaction date time.
            /// </value>
            public DateTime? TransactionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the account1 amount.
            /// </summary>
            /// <value>
            /// The account1 amount.
            /// </value>
            public decimal? Account1Amount { get; set; }

            /// <summary>
            /// Gets or sets the account2 amount.
            /// </summary>
            /// <value>
            /// The account2 amount.
            /// </value>
            public decimal? Account2Amount { get; set; }

            /// <summary>
            /// Gets or sets the account3 amount.
            /// </summary>
            /// <value>
            /// The account3 amount.
            /// </value>
            public decimal? Account3Amount { get; set; }

            /// <summary>
            /// Gets or sets the account4 amount.
            /// </summary>
            /// <value>
            /// The account4 amount.
            /// </value>
            public decimal? Account4Amount { get; set; }

            /// <summary>
            /// Gets or sets the other amount.
            /// </summary>
            /// <value>
            /// The other amount.
            /// </value>
            public decimal? OtherAmount { get; set; }

            /// <summary>
            /// Gets or sets the total transaction amount.
            /// </summary>
            /// <value>
            /// The total transaction amount.
            /// </value>
            public decimal? TotalTransactionAmount { get; set; }
        }

        /// <summary>
        /// The address summary for lava
        /// </summary>
        [DotLiquid.LiquidType( "GivingId", "AddressNames", "GreetingNames", "Street1", "Street2", "City", "State", "PostalCode" )]
        public class AddressSummary
        {
            /// <summary>
            /// Gets or sets the giving identifier.
            /// </summary>
            /// <value>
            /// The giving identifier.
            /// </value>
            public String GivingId { get; set; }

            /// <summary>
            /// Gets or sets the address names.
            /// </summary>
            /// <value>
            /// The address names.
            /// </value>
            public String AddressNames { get; set; }

            /// <summary>
            /// Gets or sets the greeting names.
            /// </summary>
            /// <value>
            /// The greeting names.
            /// </value>
            public String GreetingNames { get; set; }

            /// <summary>
            /// Gets or sets the street1.
            /// </summary>
            /// <value>
            /// The street1.
            /// </value>
            public String Street1 { get; set; }

            /// <summary>
            /// Gets or sets the street2.
            /// </summary>
            /// <value>
            /// The street2.
            /// </value>
            public String Street2 { get; set; }

            /// <summary>
            /// Gets or sets the city.
            /// </summary>
            /// <value>
            /// The city.
            /// </value>
            public String City { get; set; }

            /// <summary>
            /// Gets or sets the state.
            /// </summary>
            /// <value>
            /// The state.
            /// </value>
            public String State { get; set; }

            /// <summary>
            /// Gets or sets the postal code.
            /// </summary>
            /// <value>
            /// The postal code.
            /// </value>
            public String PostalCode { get; set; }
        }
        #endregion

    }
}

