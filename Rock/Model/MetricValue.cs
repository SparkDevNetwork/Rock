//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// MetricValue POCO Entity.
    /// </summary>
    [Table( "MetricValue" )]
    [DataContract]
    public partial class MetricValue : Model<MetricValue>, IOrdered
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the MetricId.
        /// </summary>
        /// <value>
        /// MetricId.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int MetricId { get; set; }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        /// <value>
        /// Value.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Value { get; set; }
        
        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the xValue.
        /// </summary>
        /// <value>
        /// xValue.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public string xValue { get; set; }
                
        /// <summary>
        /// Gets or sets the isDateBased flag.
        /// </summary>
        /// <value>
        /// isDateBased.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool isDateBased { get; set; }

        /// <summary>
        /// Gets or sets the Label.
        /// </summary>
        /// <value>
        /// Label.
        /// </value>
        [DataMember]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        [DataMember]
        public virtual Metric Metric { get; set; }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return this.Metric; }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricValue"/> class.
        /// </summary>
        public MetricValue()
        {
            
        }
    }
    
    /// <summary>
    /// MetricValue Configuration class.
    /// </summary>
    public partial class MetricValueConfiguration : EntityTypeConfiguration<MetricValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricValueConfiguration"/> class.
        /// </summary>
        public MetricValueConfiguration()
        {
            this.HasRequired( p => p.Metric ).WithMany( p => p.MetricValues ).HasForeignKey( p => p.MetricId ).WillCascadeOnDelete( true );
        }
    }
}
