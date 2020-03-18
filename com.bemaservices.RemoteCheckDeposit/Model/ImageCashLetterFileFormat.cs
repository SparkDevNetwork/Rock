using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace com.bemaservices.RemoteCheckDeposit.Model
{
    /// <summary>
    /// Defines an instance of a Image Cash Letter component in the database.
    /// </summary>
    [Table( "_com_bemaservices_RemoteCheckDeposit_FileFormat" )]
    [DataContract]
    public class ImageCashLetterFileFormat : Model<ImageCashLetterFileFormat>, IRockEntity
    {
        /// <summary>
        /// Gets or sets the name of this file format instance.
        /// </summary>
        [MaxLength( 100 )]
        [Required( ErrorMessage = "Name is required" )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the file format.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier. This is the component that backs this
        /// instance and provides the logic.
        /// </summary>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the file name template.
        /// </summary>
        [DataMember]
        public string FileNameTemplate { get; set; }

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the component entity type of this file format.
        /// </summary>
        [LavaInclude]
        public virtual EntityType EntityType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class ImageCashLetterFileFormatConfiguration : EntityTypeConfiguration<ImageCashLetterFileFormat>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCashLetterFileFormat"/> class.
        /// </summary>
        public ImageCashLetterFileFormatConfiguration()
        {
            this.HasRequired( f => f.EntityType )
                .WithMany()
                .HasForeignKey( f => f.EntityTypeId )
                .WillCascadeOnDelete( false );

            this.HasEntitySetName( "ImageCashLetterFileFormat" );
        }
    }

    #endregion
}
