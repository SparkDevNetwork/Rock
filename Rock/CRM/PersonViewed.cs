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
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.CRM
{
    /// <summary>
    /// Person Viewed POCO Entity.
    /// </summary>
    [Table( "crmPersonViewed" )]
    public partial class PersonViewed : ModelWithAttributes<PersonViewed>
    {
		/// <summary>
		/// Gets or sets the Viewer Person Id.
		/// </summary>
		/// <value>
		/// Viewer Person Id.
		/// </value>
		[DataMember]
		public int? ViewerPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the Target Person Id.
		/// </summary>
		/// <value>
		/// Target Person Id.
		/// </value>
		[DataMember]
		public int? TargetPersonId { get; set; }
		
		/// <summary>
		/// Gets or sets the View Date Time.
		/// </summary>
		/// <value>
		/// View Date Time.
		/// </value>
		[DataMember]
		public DateTime? ViewDateTime { get; set; }
		
		/// <summary>
		/// Gets or sets the Ip Address.
		/// </summary>
		/// <value>
		/// Ip Address.
		/// </value>
		[MaxLength( 25 )]
		[DataMember]
		public string IpAddress { get; set; }
		
		/// <summary>
		/// Gets or sets the Source.
		/// </summary>
		/// <value>
		/// Source.
		/// </value>
		[MaxLength( 50 )]
		[DataMember]
		public string Source { get; set; }
		
        /// <summary>
        /// Gets the auth entity.
        /// </summary>
		[NotMapped]
		public override string AuthEntity { get { return "CRM.PersonViewed"; } }
        
		/// <summary>
        /// Gets or sets the Viewer Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person ViewerPerson { get; set; }
        
		/// <summary>
        /// Gets or sets the Target Person.
        /// </summary>
        /// <value>
        /// A <see cref="Person"/> object.
        /// </value>
		public virtual Person TargetPerson { get; set; }

    }

    /// <summary>
    /// Person Viewed Configuration class.
    /// </summary>
    public partial class PersonViewedConfiguration : EntityTypeConfiguration<PersonViewed>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonViewedConfiguration"/> class.
        /// </summary>
        public PersonViewedConfiguration()
        {
			this.HasOptional( p => p.ViewerPerson ).WithMany().HasForeignKey( p => p.ViewerPersonId ).WillCascadeOnDelete(false);
			this.HasOptional( p => p.TargetPerson ).WithMany().HasForeignKey( p => p.TargetPersonId ).WillCascadeOnDelete(false);
		}
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class PersonViewedDTO : DTO<PersonViewed>
    {
        /// <summary>
        /// Gets or sets the Viewer Person Id.
        /// </summary>
        /// <value>
        /// Viewer Person Id.
        /// </value>
        public int? ViewerPersonId { get; set; }

        /// <summary>
        /// Gets or sets the Target Person Id.
        /// </summary>
        /// <value>
        /// Target Person Id.
        /// </value>
        public int? TargetPersonId { get; set; }

        /// <summary>
        /// Gets or sets the View Date Time.
        /// </summary>
        /// <value>
        /// View Date Time.
        /// </value>
        public DateTime? ViewDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Ip Address.
        /// </summary>
        /// <value>
        /// Ip Address.
        /// </value>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the Source.
        /// </summary>
        /// <value>
        /// Source.
        /// </value>
        public string Source { get; set; }

        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public PersonViewedDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public PersonViewedDTO( PersonViewed personViewed )
        {
            CopyFromModel( personViewed );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="personViewed"></param>
        public override void CopyFromModel( PersonViewed personViewed )
        {
            this.Id = personViewed.Id;
            this.Guid = personViewed.Guid;
            this.ViewerPersonId = personViewed.ViewerPersonId;
            this.TargetPersonId = personViewed.TargetPersonId;
            this.ViewDateTime = personViewed.ViewDateTime;
            this.IpAddress = personViewed.IpAddress;
            this.Source = personViewed.Source;
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="personViewed"></param>
        public override void CopyToModel( PersonViewed personViewed )
        {
            personViewed.Id = this.Id;
            personViewed.Guid = this.Guid;
            personViewed.ViewerPersonId = this.ViewerPersonId;
            personViewed.TargetPersonId = this.TargetPersonId;
            personViewed.ViewDateTime = this.ViewDateTime;
            personViewed.IpAddress = this.IpAddress;
            personViewed.Source = this.Source;
        }
    }
}
