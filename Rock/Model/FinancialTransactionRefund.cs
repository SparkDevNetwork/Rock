//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
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
    [Table( "FinancialTransactionRefund" )]
    [DataContract]
    public partial class FinancialTransactionRefund : Model<FinancialTransactionRefund>
    {

        #region Entity Properties

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
            this.HasOptional( t => t.RefundReasonValue ).WithMany().HasForeignKey( t => t.RefundReasonValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}