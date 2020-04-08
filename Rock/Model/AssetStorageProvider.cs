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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Storage.AssetStorage;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public partial class AssetStorageProvider : Model<AssetStorageProvider>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        /// <value>
        /// Active.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion Virtual Properties

        #region Constructors


        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Gets the asset storage component.
        /// </summary>
        /// <returns></returns>
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

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class AssetStorageServiceConfiguration : EntityTypeConfiguration<AssetStorageProvider>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetStorageServiceConfiguration"/> class.
        /// </summary>
        public AssetStorageServiceConfiguration()
        {
            this.HasRequired( g => g.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
