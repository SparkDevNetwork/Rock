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
    public partial class MetricValue : Model<MetricValue>, IAuditable, IOrdered
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
		public int xValue { get; set; }
				
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
		/// Gets or sets the Created Date Time.
		/// </summary>
		/// <value>
		/// Created Date Time.
		/// </value>
		[DataMember]
		public DateTime? CreatedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified Date Time.
		/// </summary>
		/// <value>
		/// Modified Date Time.
		/// </value>
		[DataMember]
		public DateTime? ModifiedDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Created By Person Id.
		/// </summary>
		/// <value>
		/// Created By Person Id.
		/// </value>
		[DataMember]
		public int? CreatedByPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Modified By Person Id.
		/// </summary>
		/// <value>
		/// Modified By Person Id.
		/// </value>
		[DataMember]
		public int? ModifiedByPersonId { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "Core.MetricValue"; } }
        
		/// <summary>
        /// Gets or sets the Created By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person CreatedByPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Modified By Person.
        /// </summary>
        /// <value>
        /// A <see cref="CRM.Person"/> object.
        /// </value>
		public virtual CRM.Person ModifiedByPerson { get; set; }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get { return new Security.GenericEntity( "Global" ); }
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
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class MetricValueDTO : DTO<MetricValue>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the MetricIc.
        /// </summary>
        /// <value>
        /// MetricIc.
        /// </value>
        public int MetricId { get; set; }

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        /// <value>
        /// Value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the xValue.
        /// </summary>
        /// <value>
        /// xValue.
        /// </value>
        public int xValue { get; set; }

        /// <summary>
        /// Gets or sets the isDateBased flag.
        /// </summary>
        /// <value>
        /// isDateBased.
        /// </value>
        public bool isDateBased { get; set; }

        /// <summary>
        /// Gets or sets the Label.
        /// </summary>
        /// <value>
        /// Label.
        /// </value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public MetricValueDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public MetricValueDTO( MetricValue metricValue )
        {
            CopyFromModel( metricValue );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="metricValue"></param>
        public override void CopyFromModel( MetricValue metricValue )
        {
            this.Id = metricValue.Id;
            this.Guid = metricValue.Guid;
            this.IsSystem = metricValue.IsSystem;
            this.MetricId = metricValue.MetricId;
            this.Value = metricValue.Value;
            this.Description = metricValue.Description;
            this.xValue = metricValue.xValue;
            this.isDateBased = metricValue.isDateBased;
            this.Label = metricValue.Label;
            this.Order = metricValue.Order;
            this.CreatedDateTime = metricValue.CreatedDateTime;
            this.ModifiedDateTime = metricValue.ModifiedDateTime;
            this.CreatedByPersonId = metricValue.CreatedByPersonId;
            this.ModifiedByPersonId = metricValue.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="metricValue"></param>
        public override void CopyToModel( MetricValue metricValue )
        {
            metricValue.Id = this.Id;
            metricValue.Guid = this.Guid;
            metricValue.IsSystem = this.IsSystem;
            metricValue.MetricId = this.MetricId;
            metricValue.Value = this.Value;
            metricValue.Description = this.Description;
            metricValue.xValue = this.xValue;
            metricValue.isDateBased = this.isDateBased;
            metricValue.Label = this.Label;
            metricValue.Order = this.Order;
            metricValue.CreatedDateTime = this.CreatedDateTime;
            metricValue.ModifiedDateTime = this.ModifiedDateTime;
            metricValue.CreatedByPersonId = this.CreatedByPersonId;
            metricValue.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
