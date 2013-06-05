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
    /// Refund Transaction POCO class.
    /// </summary>
    [Table( "FinancialTransactionRefund" )]
    [DataContract]
    public partial class FinancialTransactionRefund : Model<FinancialTransactionRefund>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the refund reason value id.
        /// </summary>
        /// <value>
        /// The refund reason value id.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_REFUND_REASON )]
        public int? RefundReasonValueId { get; set; }

        /// <summary>
        /// Gets or sets the refund reason summary.
        /// </summary>
        /// <value>
        /// The refund reason summary.
        /// </value>
        [DataMember]
        public string RefundReasonSummary { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the refund reason value.
        /// </summary>
        /// <value>
        /// The refund reason value.
        /// </value>
        [DataMember]
        public virtual DefinedValue RefundReasonValue { get; set; }

        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// Transaction Configuration class.
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