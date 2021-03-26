using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Media Account
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "MediaAccount" )]
    [DataContract]
    public partial class MediaAccount : Model<MediaAccount>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Name of the MediaAccount. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the MediaAccount.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the last refresh date time.
        /// </summary>
        /// <value>
        /// The last refresh date time.
        /// </value>
        [DataMember]
        public DateTime? LastRefreshDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the Id of the achievement component <see cref="EntityType"/>
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int ComponentEntityTypeId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.MediaFolder">Media Folders</see> that belong to this Account.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.MediaFolder">Media Folders</see> that belong to this Account.
        /// </value>
        [DataMember]
        public virtual ICollection<MediaFolder> MediaFolders { get; set; }

        /// <summary>
        /// Gets or sets the type of the component entity.
        /// </summary>
        /// <value>
        /// The type of the component entity.
        /// </value>
        [DataMember]
        public virtual EntityType ComponentEntityType { get; set; }

        /// <summary>
        /// Gets or sets the Defined Type that this DefinedValue belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedType"/> that this DefinedValue belongs to.
        /// </value>
        [LavaInclude]
        public virtual DefinedType DefinedType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// MediaAccount Configuration class.
    /// </summary>
    public partial class MediaAccountConfiguration : EntityTypeConfiguration<MediaAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaAccountConfiguration"/> class.
        /// </summary>
        public MediaAccountConfiguration()
        {
            this.HasRequired( b => b.ComponentEntityType ).WithMany().HasForeignKey( b => b.ComponentEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}