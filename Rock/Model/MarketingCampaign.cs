//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// MarketingCampaign POCO Entity
    /// </summary>
    [Table( "MarketingCampaign" )]
    [DataContract]
    public partial class MarketingCampaign : Model<MarketingCampaign>
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [MergeField]
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
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
        [MergeField]
        [MaxLength( 254 )]
        [DataMember]
        public string ContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the contact phone number.
        /// </summary>
        /// <value>
        /// The contact phone number.
        /// </value>
        [MergeField]
        [MaxLength( 20 )]
        [DataMember]
        public string ContactPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the full name of the contact.
        /// </summary>
        /// <value>
        /// The full name of the contact.
        /// </value>
        [MergeField]
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
        /// Gets or sets the contact person.
        /// </summary>
        /// <value>
        /// The contact person.
        /// </value>
        [MergeField]
        [DataMember]
        public virtual Person ContactPerson { get; set; }

        /// <summary>
        /// Gets or sets the event group.
        /// </summary>
        /// <value>
        /// The event group.
        /// </value>
        [MergeField]
        [DataMember]
        public virtual Group EventGroup { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign ads.
        /// </summary>
        /// <value>
        /// The marketing campaign ads.
        /// </value>
        [DataMember]
        public virtual ICollection<MarketingCampaignAd> MarketingCampaignAds { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign audiences.
        /// </summary>
        /// <value>
        /// The marketing campaign audiences.
        /// </value>
        [DataMember]
        public virtual ICollection<MarketingCampaignAudience> MarketingCampaignAudiences { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign campuses.
        /// </summary>
        /// <value>
        /// The marketing campaign campuses.
        /// </value>
        [DataMember]
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