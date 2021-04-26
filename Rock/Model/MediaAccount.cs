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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Media;
using Rock.Web.Cache;

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the media component.
        /// </summary>
        /// <returns></returns>
        public virtual MediaAccountComponent GetMediaAccountComponent()
        {
            var entityType = EntityTypeCache.Get( ComponentEntityTypeId );

            if ( entityType != null )
            {
                return MediaAccountContainer.GetComponent( entityType.Name );
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