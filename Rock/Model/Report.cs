//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq.Expressions;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Report (based off of a <see cref="Rock.Model.DataView"/> in RockChMS.
    /// </summary>
    [Table( "Report" )]
    [DataContract]
    public partial class Report : Model<Report>, ICategorized
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Report is part of the RockChMS core system/framework. This property is required.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is <c>true</c> if the Report is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Report. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the Report.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Report's Description.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Report's Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that the Report belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CateogryId of the <see cref="Rock.Model.Category"/> that the report belongs to. If the Report does not belong to
        /// a <see cref="Rock.Model.Category"/> this value will be null.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or the DataViewId of the root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DataViewId of the root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this Report belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/> that this Report belongs to. If the Report does not belong to a <see cref="Rock.Model.Category"/> this value will be null.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> that is being reported on. 
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that is being reported on.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the base/root <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DataView"/> that this Report is based on.
        /// </value>
        [DataMember]
        public virtual DataView DataView { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Campus Configuration class.
    /// </summary>
    public partial class ReportConfiguration : EntityTypeConfiguration<Report>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        public ReportConfiguration()
        {
            this.HasOptional( r => r.Category ).WithMany().HasForeignKey( r => r.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.DataView ).WithMany().HasForeignKey( r => r.DataViewId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
