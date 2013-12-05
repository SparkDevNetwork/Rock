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
    /// Represents an image that is associated with a <see cref="Rock.Model.FinancialTransaction"/>. Examples could be 
    /// the front or back side of a check or an offering envelope.
    /// </summary>
    [Table( "FinancialTransactionImage" )]
    [DataContract]
    public partial class FinancialTransactionImage : Model<FinancialTransactionImage>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the TransactionId of the <see cref="Rock.Model.FinancialTransaction"/> that this image belongs to
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the <see cref="Rock.Model.FinancialTransaction"/>that this image belongs to.
        /// </value>
        [DataMember]
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the BinaryFileId of the image's <see cref="Rock.Model.BinaryFile"/> 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing BinaryFileId of the image's <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        [DataMember]
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets DefinedValueId of the transaction image type <see cref="Rock.Model.DefinedValue"/> for this image.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the transaction image type <see cref="Rock.Model.DefinedValue"/>
        /// for this image.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_IMAGE_TYPE )]
        public int? TransactionImageTypeValueId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialTransaction"/> that this image belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialTransaction"/> that this image belongs to.
        /// </value>
        public virtual FinancialTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.BinaryFile"/> of the image.
        /// </summary>
        /// <value>
        /// The image's <see cref="Rock.Model.BinaryFile"/>
        /// </value>
        public virtual BinaryFile BinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the transaction image type <see cref="Rock.Model.DefinedValue"/> of this image.
        /// </summary>
        /// <value>
        /// The transaction image type <see cref="Rock.Model.DefinedValue"/> of this image.
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
            this.HasRequired( i => i.Transaction ).WithMany( t => t.Images ).HasForeignKey( i => i.TransactionId ).WillCascadeOnDelete( false );
            this.HasRequired( i => i.BinaryFile ).WithMany().HasForeignKey( i => i.BinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( i => i.TransactionImageTypeValue ).WithMany().HasForeignKey( i => i.TransactionImageTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}