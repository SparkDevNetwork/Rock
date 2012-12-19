//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// MarketingCampaignAudience POCO Entity
    /// </summary>
    [Table( "MarketingCampaignAudience" )]
    [DataContract( IsReference = true )]
    public partial class MarketingCampaignAudience : Model<MarketingCampaignAudience>
    {
        /// <summary>
        /// Gets or sets the marketing campaign id.
        /// </summary>
        /// <value>
        /// The marketing campaign id.
        /// </value>
        [DataMember]
        public int MarketingCampaignId { get; set; }

        /// <summary>
        /// Gets or sets the audience type id.
        /// </summary>
        /// <value>
        /// The audience type id.
        /// </value>
        [DataMember]
        public int AudienceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is primary; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public virtual string Name
        {
            get
            {
                return ( AudienceTypeValue != null ? AudienceTypeValue.Name : string.Empty );
            }
        }

        /// <summary>
        /// Gets or sets the marketing campaign.
        /// </summary>
        /// <value>
        /// The marketing campaign.
        /// </value>
        [DataMember]
        public virtual MarketingCampaign MarketingCampaign { get; set; }

        /// <summary>
        /// Gets or sets the audience type value.
        /// </summary>
        /// <value>
        /// The audience type value.
        /// </value>
        [DataMember]
        public virtual Model.DefinedValue AudienceTypeValue { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarketingCampaignAudienceConfiguration : EntityTypeConfiguration<MarketingCampaignAudience>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarketingCampaignAudienceConfiguration" /> class.
        /// </summary>
        public MarketingCampaignAudienceConfiguration()
        {
            this.HasRequired( p => p.MarketingCampaign ).WithMany( p => p.MarketingCampaignAudiences).HasForeignKey( p => p.MarketingCampaignId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.AudienceTypeValue ).WithMany().HasForeignKey( p => p.AudienceTypeValueId ).WillCascadeOnDelete( true );
        }
    }
}