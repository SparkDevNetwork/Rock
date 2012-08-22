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
    /// Metric POCO Entity.
    /// </summary>
    [Table( "coreMetric" )]
    public partial class Metric : Model<Metric>, IAuditable, IOrdered
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
		/// Gets or sets the Type.
		/// </summary>
		/// <value>
		/// Type.
		/// </value>
		[Required]
		[DataMember]
		public bool Type { get; set; }

		/// <summary>
		/// Gets or sets the Category.
		/// </summary>
		/// <value>
		/// Category.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Category { get; set; }

		/// <summary>
		/// Gets or sets the Title.
		/// </summary>
		/// <value>
		/// Title.
		/// </value>
		[Required]
		[MaxLength( 100 )]
		[DataMember]
		public string Title { get; set; }
		
		/// <summary>
		/// Gets or sets the Subtitle.
		/// </summary>
		/// <value>
		/// Subtitle.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Subtitle { get; set; }
		
		/// <summary>
		/// Gets or sets the Description.
		/// </summary>
		/// <value>
		/// Description.
		/// </value>
		[DataMember]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the MinValue.
		/// </summary>
		/// <value>
		/// MinValue.
		/// </value>
		[Required]
		[DataMember]
		public int MinValue { get; set; }

		/// <summary>
		/// Gets or sets the MaxValue.
		/// </summary>
		/// <value>
		/// MaxValue.
		/// </value>
		[Required]
		[DataMember]
		public int MaxValue { get; set; }

		/// <summary>
		/// Gets or sets the CollectionFrequency.
		/// </summary>
		/// <value>
		/// CollectionFrequency.
		/// </value>
		[Required]
		[DataMember]
		public int CollectionFrequency { get; set; }

		/// <summary>
		/// Gets or sets the LastCollected date.
		/// </summary>
		/// <value>
		/// LastCollected.
		/// </value>
		[Required]
		[DataMember]
		public DateTime LastCollected { get; set; }

		/// <summary>
		/// Gets or sets the Source.
		/// </summary>
		/// <value>
		/// Source.
		/// </value>
		[MaxLength( 100 )]
		[DataMember]
		public string Source { get; set; }

		/// <summary>
		/// Gets or sets the SourceSQL.
		/// </summary>
		/// <value>
		/// SourceSQL.
		/// </value>
		[DataMember]
		public string SourceSQL { get; set; }
		
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
		public override string AuthEntity { get { return "Core.Metric"; } }
        
		/// <summary>
        /// Gets or sets the Metric Values.
        /// </summary>
        /// <value>
        /// Collection of Metric Values.
        /// </value>
		public virtual ICollection<MetricValue> MetricValues { get; set; }
               
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
        /// Initializes a new instance of the <see cref="Metric"/> class.
        /// </summary>
        public Metric()
        {
            
        }
    }

    /// <summary>
    /// Metric Configuration class.
    /// </summary>
    public partial class MetricConfiguration : EntityTypeConfiguration<Metric>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricConfiguration"/> class.
        /// </summary>
        public MetricConfiguration()
        {
			//this.HasOptional( p => p.MetricValues ).WithMany().HasForeignKey( p => MetricId ).WillCascadeOnDelete( true );
			this.HasOptional( p => p.CreatedByPerson ).WithMany().HasForeignKey( p => p.CreatedByPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.ModifiedByPerson ).WithMany().HasForeignKey( p => p.ModifiedByPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class MetricDTO : DTO<Metric>
    {
        /// <summary>
        /// Gets or sets the System.
        /// </summary>
        /// <value>
        /// System.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Type.
        /// </summary>
        /// <value>
        /// Type.
        /// </value>
        public bool Type { get; set; }

        /// <summary>
        /// Gets or sets the Category.
        /// </summary>
        /// <value>
        /// Category.
        /// </value>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        /// <value>
        /// Title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Subtitle.
        /// </summary>
        /// <value>
        /// Subtitle.
        /// </value>
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the MinValue.
        /// </summary>
        /// <value>
        /// MinValue.
        /// </value>
        public int MinValue { get; set; }

        /// <summary>
        /// Gets or sets the MaxValue.
        /// </summary>
        /// <value>
        /// MaxValue.
        /// </value>
        public int MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the CollectionFrequency.
        /// </summary>
        /// <value>
        /// CollectionFrequency.
        /// </value>
        public int CollectionFrequency { get; set; }

        /// <summary>
        /// Gets or sets the LastCollected date.
        /// </summary>
        /// <value>
        /// LastCollected.
        /// </value>
        public DateTime LastCollected { get; set; }

        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        /// <value>
        /// Source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the SourceSQL.
        /// </summary>
        /// <value>
        /// SourceSQL.
        /// </value>
        public string SourceSQL { get; set; }

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
        public MetricDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public MetricDTO( Metric metric )
        {
            CopyFromModel( metric );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="metric"></param>
        public override void CopyFromModel( Metric metric )
        {
            this.Id = metric.Id;
            this.Guid = metric.Guid;
            this.IsSystem = metric.IsSystem;
            this.Type = metric.Type;
            this.Category = metric.Category;
            this.Title = metric.Title;
            this.Subtitle = metric.Subtitle;
            this.Description = metric.Description;
            this.MinValue = metric.MinValue;
            this.MaxValue = metric.MaxValue;
            this.CollectionFrequency = metric.CollectionFrequency;
            this.LastCollected = metric.LastCollected;
            this.Source = metric.Source;
            this.SourceSQL = metric.SourceSQL;
            this.Order = metric.Order;
            this.CreatedDateTime = metric.CreatedDateTime;
            this.ModifiedDateTime = metric.ModifiedDateTime;
            this.CreatedByPersonId = metric.CreatedByPersonId;
            this.ModifiedByPersonId = metric.ModifiedByPersonId;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="metric"></param>
        public override void CopyToModel( Metric metric )
        {
            metric.Id = this.Id;
            metric.Guid = this.Guid;
            metric.IsSystem = this.IsSystem;
            metric.Type = this.Type;
            metric.Category = this.Category;
            metric.Title = this.Title;
            metric.Subtitle = this.Subtitle;
            metric.Description = this.Description;
            metric.MinValue = this.MinValue;
            metric.MaxValue = this.MaxValue;
            metric.CollectionFrequency = this.CollectionFrequency;
            metric.LastCollected = this.LastCollected;
            metric.Source = this.Source;
            metric.SourceSQL = this.SourceSQL;
            metric.Order = this.Order;
            metric.CreatedDateTime = this.CreatedDateTime;
            metric.ModifiedDateTime = this.ModifiedDateTime;
            metric.CreatedByPersonId = this.CreatedByPersonId;
            metric.ModifiedByPersonId = this.ModifiedByPersonId;
        }
    }
}
