//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Core
{
    /// <summary>
    /// MetricValue POCO Entity.
    /// </summary>
    [Table( "coreMetricValue" )]
    public partial class MetricValue : Model<MetricValue>, IOrdered
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        [Required]
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the MetricId.
        /// </summary>
        /// <value>
        /// MetricId.
        /// </value>
        [Required]
        [DataMember]
        public int MetricId { get; set; }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        /// <value>
        /// Value.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
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
        [DataMember]
        public string xValue { get; set; }
                
        /// <summary>
        /// Gets or sets the isDateBased flag.
        /// </summary>
        /// <value>
        /// isDateBased.
        /// </value>
        [Required]
        [DataMember]
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
        [DataMember]
        public int Order { get; set; }
        
        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static MetricValue Read( int id )
        {
            return Read<MetricValue>( id );
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return new Security.GenericEntity( "Global" ); }
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
        }
    }
}
