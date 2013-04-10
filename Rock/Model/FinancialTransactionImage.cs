//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Transaction Image POCO class.
    /// </summary>
    [Table( "FinancialTransactionImage" )]
    [DataContract]
    public partial class FinancialTransactionImage : Model<FinancialTransactionImage>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the transaction id.
        /// </summary>
        /// <value>
        /// The transaction id.
        /// </value>
        [DataMember]
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the binary file id.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        [DataMember]
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the transaction image type value id.
        /// </summary>
        /// <value>
        /// The transaction image type value id.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_IMAGE_TYPE )]
        public int? TransactionImageTypeValueId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the transaction.
        /// </summary>
        /// <value>
        /// The transaction.
        /// </value>
        public virtual FinancialTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the binary file.
        /// </summary>
        /// <value>
        /// The binary file.
        /// </value>
        public virtual BinaryFile BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the transaction image type value.
        /// </summary>
        /// <value>
        /// The transaction image type value.
        /// </value>
        public virtual DefinedValue TransactionImageTypeValue { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// TransactionImage Configuration class
    /// </summary>
    public partial class FinancialTransactionImageConfiguration : EntityTypeConfiguration<FinancialTransactionImage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionImageConfiguration"/> class.
        /// </summary>
        public FinancialTransactionImageConfiguration()
        {
            this.HasOptional( i => i.Transaction ).WithMany( t => t.Images ).HasForeignKey( i => i.TransactionId ).WillCascadeOnDelete( false );
            this.HasOptional( i => i.BinaryFile ).WithMany().HasForeignKey( i => i.BinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( i => i.TransactionImageTypeValue ).WithMany().HasForeignKey( i => i.TransactionImageTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}