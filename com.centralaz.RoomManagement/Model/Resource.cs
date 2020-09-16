// <copyright>
// Copyright by the Central Christian Church
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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// A Resource
    /// </summary>
    [Table( "_com_centralaz_RoomManagement_Resource" )]
    [DataContract]
    public class Resource : Rock.Data.Model<Resource>, Rock.Data.IRockEntity, Rock.Data.ICategorized
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [Required]
        [MaxLength( 50 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the approval group identifier.
        /// </summary>
        /// <value>
        /// The approval group identifier.
        /// </value>
        [DataMember]
        public int? ApprovalGroupId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        [DataMember]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        [MaxLength( 2000 )]
        public string Note { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the approval group.
        /// </summary>
        /// <value>
        /// The approval group.
        /// </value>
        [LavaInclude]
        public virtual Group ApprovalGroup { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the Name (and Id) that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the Name (and Id) that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} ({1})", Name, Id );
        }

        #endregion
    }

    #region Entity Configuration


    /// <summary>
    /// The EF configuration for the Resource model
    /// </summary>
    public partial class ResourceConfiguration : EntityTypeConfiguration<Resource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceConfiguration"/> class.
        /// </summary>
        public ResourceConfiguration()
        {
            this.HasRequired( r => r.Category ).WithMany().HasForeignKey( r => r.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Campus ).WithMany().HasForeignKey( r => r.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Location ).WithMany().HasForeignKey( r => r.LocationId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.ApprovalGroup ).WithMany().HasForeignKey( r => r.ApprovalGroupId ).WillCascadeOnDelete( false );

            // IMPORTANT!!
            this.HasEntitySetName( "Resource" );
        }
    }

    #endregion

}
