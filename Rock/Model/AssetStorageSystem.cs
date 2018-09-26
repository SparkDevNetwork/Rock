using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;
using Rock.Storage.AssetStorage;

namespace Rock.Model
{
    [DataContract]
    public partial class AssetStorageSystem : Model<AssetStorageSystem>, IHasActiveFlag
    {
        #region Entity Properties

        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        [DataMember]
        public int? EntityTypeId { get; set; }

        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public string Description { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion Virtual Properties

        #region Constructors

        public AssetStorageSystem()
        {
            IsActive = true;
        }

        #endregion Constructors

        #region Public Methods
        public virtual AssetStorageComponent GetAssetStorageComponent()
        {
            if ( EntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( EntityTypeId.Value );
                if ( entityType != null )
                {
                    return AssetStorageContainer.GetComponent( entityType.Name );
                }
            }

            return null;
        }

        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods


    }

    #region Entity Configuration

    public partial class AssetStorageServiceConfiguration : EntityTypeConfiguration<AssetStorageSystem>
    {
        public AssetStorageServiceConfiguration()
        {
            this.HasRequired( g => g.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
