using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace church.ccv.Pastoral.Model
{
    /// <summary>
    /// Represents a Care Request document.
    /// </summary>
    [Table( "_church_ccv_Pastoral_CareDocument" )]
    [DataContract]
    public partial class CareDocument : Model<CareDocument>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the Care Request Id.
        /// </summary>
        /// <value>
        /// The Care Request Id.
        /// </value>
        [Required]
        [DataMember]
        public int CareRequestId { get; set; }

        /// <summary>
        /// Gets or sets the binary file id.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        [Required]
        [DataMember]
        public int BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int? Order { get; set; }
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the package.
        /// </summary>
        /// <value>
        /// The package.
        /// </value>
        public virtual CareRequest CareRequest { get; set; }

        /// <summary>
        /// Gets or sets the binary file.
        /// </summary>
        /// <value>
        /// The binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile BinaryFile { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// CareDocument Configuration class.
    /// </summary>
    public partial class CareDocumentConfiguration : EntityTypeConfiguration<CareDocument>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CareDocumentConfiguration" /> class.
        /// </summary>
        public CareDocumentConfiguration()
        {
            this.HasRequired( p => p.CareRequest ).WithMany( p => p.Documents ).HasForeignKey( p => p.CareRequestId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.BinaryFile ).WithMany().HasForeignKey( p => p.BinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}