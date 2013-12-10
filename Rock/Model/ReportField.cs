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
    /// 
    /// </summary>
    [NotAudited]
    [Table( "ReportField" )]
    [DataContract]
    public partial class ReportField : Model<ReportField>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the report identifier.
        /// </summary>
        /// <value>
        /// The report identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ReportId { get; set; }

        /// <summary>
        /// Gets or sets the type of the report field.
        /// </summary>
        /// <value>
        /// The type of the report field.
        /// </value>
        [Required]
        [DataMember]
        public ReportFieldType ReportFieldType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show in grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in grid]; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool ShowInGrid { get; set; }

        /// <summary>
        /// Gets or sets the data select component entity type identifier.
        /// </summary>
        /// <value>
        /// The data select component entity type identifier.
        /// </value>
        [DataMember]
        public int? DataSelectComponentEntityTypeId { get; set; }

        /// <summary>
        /// Selection is where the FieldType stores specific parameter values 
        /// If ReportFieldType is Column or Attribute, it is the Column or Attribute name
        /// If ReportFieldType is DataSelectComponent, it will be some values of whatever the DataSelectComponent implements for specific parameters
        /// </summary>
        /// <value>
        /// The selection.
        /// </value>
        [DataMember]
        public string Selection { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the report.
        /// </summary>
        /// <value>
        /// The report.
        /// </value>
        [DataMember]
        public virtual Report Report { get; set; }

        /// <summary>
        /// Gets or sets the type of the data select component entity.
        /// </summary>
        /// <value>
        /// The type of the data select component entity.
        /// </value>
        [DataMember]
        public virtual EntityType DataSelectComponentEntityType { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ReportFieldConfiguration : EntityTypeConfiguration<ReportField>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportFieldConfiguration"/> class.
        /// </summary>
        public ReportFieldConfiguration()
        {
            this.HasRequired( p => p.Report ).WithMany( p => p.ReportFields ).HasForeignKey( p => p.ReportId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.DataSelectComponentEntityType ).WithMany().HasForeignKey( p => p.DataSelectComponentEntityTypeId ).WillCascadeOnDelete( true );
        }
    }

    #region Enumerations

    /// <summary>
    /// 
    /// </summary>
    public enum ReportFieldType
    {
        /// <summary>
        /// The field is one of the properties of the entity
        /// </summary>
        Property = 0,

        /// <summary>
        /// The field is one of the attributes of the entity
        /// </summary>
        Attribute = 1,

        /// <summary>
        /// The field(s) that result from a <see cref="Rock.Reporting.DataSelectComponent" />
        /// </summary>
        DataSelectComponent = 2
    }

    #endregion
}
