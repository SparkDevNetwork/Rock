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
using Rock.Crm;
using Rock.Data;

namespace Rock.Cms
{
    /// <summary>
    /// MarketingCampaign POCO Entity
    /// </summary>
    [Table( "cmsMarketingCampaign" )]
    public partial class MarketingCampaign : Model<MarketingCampaign>, IExportable
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the contact person id.
        /// </summary>
        /// <value>
        /// The contact person id.
        /// </value>
        [DataMember]
        public int? ContactPersonId { get; set; }

        /// <summary>
        /// Gets or sets the contact email.
        /// </summary>
        /// <value>
        /// The contact email.
        /// </value>
        [MaxLength( 254 )]
        [DataMember]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the contact phone number.
        /// </summary>
        /// <value>
        /// The contact phone number.
        /// </value>
        [MaxLength( 20 )]
        [DataMember]
        public string ContactPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the full name of the contact.
        /// </summary>
        /// <value>
        /// The full name of the contact.
        /// </value>
        [MaxLength( 152 )]
        [DataMember]
        public string ContactFullName { get; set; }

        /// <summary>
        /// Gets or sets the event group id.
        /// </summary>
        /// <value>
        /// The event group id.
        /// </value>
        [DataMember]
        public int? EventGroupId { get; set; }

        /// <summary>
        /// Reads the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static MarketingCampaign Read( int id )
        {
            return Read<MarketingCampaign>( id );
        }

        /// <summary>
        /// Gets or sets the contact person.
        /// </summary>
        /// <value>
        /// The contact person.
        /// </value>
        public virtual Person ContactPerson { get; set; }

        /// <summary>
        /// Gets or sets the event group.
        /// </summary>
        /// <value>
        /// The event group.
        /// </value>
        public virtual Group EventGroup { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign audiences.
        /// </summary>
        /// <value>
        /// The marketing campaign audiences.
        /// </value>
        public virtual ICollection<MarketingCampaignAudience> MarketingCampaignAudiences { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign campuses.
        /// </summary>
        /// <value>
        /// The marketing campaign campuses.
        /// </value>
        public virtual ICollection<MarketingCampaignCampus> MarketingCampaignCampuses { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        /// <summary>
        /// Exports the object as JSON.
        /// </summary>
        /// <returns></returns>
        public string ExportJson()
        {
            return ExportObject().ToJSON();
        }

        /// <summary>
        /// Exports the object.
        /// </summary>
        /// <returns></returns>
        public object ExportObject()
        {
            return this.ToDynamic();
        }

        /// <summary>
        /// Imports the object from JSON.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ImportJson( string data )
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarketingCampaignConfiguration : EntityTypeConfiguration<MarketingCampaign>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarketingCampaignConfiguration" /> class.
        /// </summary>
        public MarketingCampaignConfiguration()
        {
            this.HasOptional( p => p.ContactPerson ).WithMany().HasForeignKey( p => p.ContactPersonId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.EventGroup ).WithMany().HasForeignKey( p => p.EventGroupId).WillCascadeOnDelete( false );
        }
    }
}