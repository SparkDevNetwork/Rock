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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a transaction where a giver/purchaser was refunded a full or partial amount 
    /// on a <see cref="Rock.Model.FinancialTransaction"/>.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialTransactionRefund" )]
    [DataContract]
    public partial class FinancialTransactionRefund : Model<FinancialTransactionRefund>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the original transaction identifier.
        /// </summary>
        /// <value>
        /// The original transaction identifier.
        /// </value>
        [DataMember]
        public int? OriginalTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the return reason <see cref="Rock.Model.DefinedValue"/> indicating
        /// the reason why a refund was issued for the the original transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the refund reason <see cref="Rock.Model.DefinedValue"/> 
        /// indicating the reason for the refund.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_REFUND_REASON )]
        public int? RefundReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets a detailed summary of the circumstances surrounding why a refund was issued.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a summary of why the refund was issued.
        /// </value>
        [DataMember]
        public string RefundReasonSummary { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the financial transaction.
        /// </summary>
        /// <value>
        /// The financial transaction.
        /// </value>
        [DataMember]
        public virtual FinancialTransaction FinancialTransaction { get; set; }

        /// <summary>
        /// Gets or sets the original transaction.
        /// </summary>
        /// <value>
        /// The original transaction.
        /// </value>
        [DataMember]
        public virtual FinancialTransaction OriginalTransaction { get; set; }

        /// <summary>
        /// Gets or sets the refund reason <see cref="Rock.Model.DefinedValue"/> indicating the reason 
        /// for the refund.
        /// </summary>
        /// <value>
        /// The refund reason <see cref="Rock.Model.DefinedValue"/>.
        /// </value>
        [DataMember]
        public virtual DefinedValue RefundReasonValue { get; set; }

        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// Refund Transaction Configuration class.
    /// </summary>
    public partial class FinancialTransactionRefundConfiguration : EntityTypeConfiguration<FinancialTransactionRefund>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionRefundConfiguration"/> class.
        /// </summary>
        public FinancialTransactionRefundConfiguration()
        {
            this.HasOptional( r => r.OriginalTransaction ).WithMany( t => t.Refunds ).HasForeignKey( r => r.OriginalTransactionId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.RefundReasonValue ).WithMany().HasForeignKey( r => r.RefundReasonValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}