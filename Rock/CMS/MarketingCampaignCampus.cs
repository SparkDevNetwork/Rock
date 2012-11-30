//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Rock.Crm;
using Rock.Data;

namespace Rock.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "MarketingCampaignCampus")]
    public partial class MarketingCampaignCampus : Model<MarketingCampaignCampus>
    {
        /// <summary>
        /// Gets or sets the marketing campaign id.
        /// </summary>
        /// <value>
        /// The marketing campaign id.
        /// </value>
        public int MarketingCampaignId { get; set; }

        /// <summary>
        /// Gets or sets the campus id.
        /// </summary>
        /// <value>
        /// The campus id.
        /// </value>
        public int CampusId { get; set; }

        /// <summary>
        /// Gets or sets the marketing campaign.
        /// </summary>
        /// <value>
        /// The marketing campaign.
        /// </value>
        public virtual MarketingCampaign MarketingCampaign { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Reads the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static MarketingCampaignCampus Read( int id )
        {
            return Read<MarketingCampaignCampus>( id );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarketingCampaignCampusConfiguration : EntityTypeConfiguration<MarketingCampaignCampus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarketingCampaignCampusConfiguration" /> class.
        /// </summary>
        public MarketingCampaignCampusConfiguration()
        {
            this.HasRequired( p => p.MarketingCampaign ).WithMany(p => p.MarketingCampaignCampuses ).HasForeignKey( p => p.MarketingCampaignId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Campus ).WithMany().HasForeignKey( p => p.CampusId ).WillCascadeOnDelete( true );
        }
    }
}