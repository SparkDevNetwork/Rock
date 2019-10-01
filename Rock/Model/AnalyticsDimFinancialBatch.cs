// <copyright>
// Copyright by the Spark Development Network
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// AnalyticsDimFinancialBatch is a SQL View off of the FinancialBatch table
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "AnalyticsDimFinancialBatch" )]
    [DataContract]
    [HideFromReporting]
    public class AnalyticsDimFinancialBatch : Rock.Data.Entity<AnalyticsDimFinancialBatch>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the batch identifier.
        /// </summary>
        /// <value>
        /// The batch identifier.
        /// </value>
        public int BatchId { get; set; }

        /// <summary>
        /// Gets or sets the name of the batch.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the batch.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the start posting date and time range of <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that are included in this batch.  
        /// Transactions that post on or after this date and time and before the <see cref="BatchEndDateTime"/> can be included in this batch.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the posting start date for the batch.
        /// </value>
        [DataMember]
        [Column( TypeName = "DateTime" )]
        public DateTime? BatchStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets end of the posting date and time range for <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that are included in this batch.
        /// Transactions that post before or on this date and time and after the <see cref="BatchStartDateTime"/> can be included in this batch.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the posting end date for the batch.
        /// </value>
        [DataMember]
        [Column( TypeName = "DateTime" )]
        public DateTime? BatchEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the batch.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.BatchStatus"/> representing the status of the batch.
        /// When this value is <c>BatchStatus.Pending</c>  it means that transactions are still being added to the batch.
        /// When this value is <c>BatchStatus.Open</c> it means that all transactions have been added and are ready to be matched up.
        /// When this value is <c>BatchStatus.Closed</c> it means that the batch has balanced and has been closed.
        /// </value>
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [DataMember]
        public string Campus { get; set; }

        /// <summary>
        /// Gets or sets an optional transaction code from an accounting system that batch is associated with
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Accounting System transaction code for the batch.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string AccountingSystemCode { get; set; }

        /// <summary>
        /// Gets or sets the control amount. This should match the total value of all <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that are 
        /// included in the batch.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the control amount of the batch.
        /// </value>
        [DataMember]
        public decimal ControlAmount { get; set; }

        #endregion

        #region Entity Properties specific to Analytics

        /// <summary>
        /// Gets or sets the count.
        /// NOTE: this always has a hardcoded value of 1. It is stored in the table because it is supposed to help do certain types of things in analytics
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        [DataMember]
        public int Count { get; set; }

        #endregion
    }
}
