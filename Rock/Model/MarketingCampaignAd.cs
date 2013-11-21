//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "MarketingCampaignAd")]
    [DataContract]
    public partial class MarketingCampaignAd : Model<MarketingCampaignAd>
    {
        /// <summary>
        /// Gets or sets the MarketingCampaignId of the <see cref="Rock.Model.MarketingCampaign"/> that this MarketingCampaignAd belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the MarketignCampaignId of the <see cref="Rock.Model.MarketingCampaign"/> that this MarketingCampaignAd belongs to.
        /// </value>
        [DataMember]
        public int MarketingCampaignId { get; set; }


        /// <summary>
        /// Gets or sets the MarketingCampaignAdTypeId of the <see cref="Rock.Model.MarketingCampaignAdType"/> of this MarketingCampaignAd.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the MarketingTypeAdTypeId of the <see cref="Rock.Model.MarketingCampaignAdType"/> of this MarketingCampaignAd.
        /// </value>
        [DataMember]
        public int MarketingCampaignAdTypeId { get; set; }

        /// <summary>
        /// Gets or sets the priority of this MarketingCampaignAd. The lower the number, the higher the priority.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the priority of this MarketingCampaignAd. The lower the number, the higher the priority of the Ad.
        /// </value>
        [MergeField]
        [DataMember]
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.MarketingCampaignAdStatus"/> (status) of this MarketingCampaignAd.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.MarketingCampaignAdStatus"/> enumeration value that represents the status of this MarketingCampaignAd. When <c>MarketingCampaignAdStatus.PendingApproval</c> the ad is 
        /// awaiting approval; when <c>MarketingCampaignAdStatus.Approved</c> the ad has been approved by the approver, when <c>MarketingCampaignAdStatus.Denied</c> the ad has been denied by the approver.
        /// </value>
        [MergeField]
        [DataMember]
        public MarketingCampaignAdStatus MarketingCampaignAdStatus { get; set; }

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who either approved or declined the MarketingCampaignAd. If no approval action has been performed on this Ad, this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of hte <see cref="Rock.Model.Person"/> who either approved or declined the MarketingCampaignAd. This value will be null if no approval action has been
        /// performed on this add.
        /// </value>
        [DataMember]
        public int? MarketingCampaignStatusPersonId { get; set; }


        /// <summary>
        /// Gets or sets the StartDate that the MarketingCampaignAd will begin running.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the StartDate of the MarketingCampaignAd will run.
        /// </value>
        [MergeField]
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the EndDate that the MarketingCampaignAd will stop running.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime" /> representing the EndDate of when the MarketingCampaignAd will run.
        /// </value>
        [MergeField]
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the URL that this MarketingCampaignAd should direct people to.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the URL that this MarketingCampaingnAdd should direct people to.
        /// </value>
        [MergeField]
        [MaxLength( 2000 )]
        [DataMember]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.MarketingCampaign"/> that this ad belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.MarketingCampaign"/> that this ad belongs to.
        /// </value>
        [MergeField]
        [DataMember]
        public virtual MarketingCampaign MarketingCampaign { get; set; }

        /// <summary>
        /// Gets or sets the the <see cref="Rock.Model.MarketingCampaignAdType"/> of this ad.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.MarketingCampaignAdType"/> of this Ad.
        /// </value>
        [MergeField]
        [DataMember]
        public virtual MarketingCampaignAdType MarketingCampaignAdType { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarketingCampaignAdConfiguration : EntityTypeConfiguration<MarketingCampaignAd>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarketingCampaignAdConfiguration" /> class.
        /// </summary>
        public MarketingCampaignAdConfiguration()
        {
            this.HasRequired( p => p.MarketingCampaign ).WithMany( a => a.MarketingCampaignAds ).HasForeignKey( p => p.MarketingCampaignId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.MarketingCampaignAdType ).WithMany().HasForeignKey( p => p.MarketingCampaignAdTypeId ).WillCascadeOnDelete( false );
        }
    }

    /// <summary>
    /// Represents the status of a Marketing Campaign Card
    /// </summary>
    public enum MarketingCampaignAdStatus : byte
    {
        /// <summary>
        /// The <see cref="MarketingCampaignAd"/> is pending approval.
        /// </summary>
        PendingApproval = 1,

        /// <summary>
        /// The <see cref="MarketingCampaignAd"/> has been approved.
        /// </summary>
        Approved = 2,

        /// <summary>
        /// The <see cref="MarketingCampaignAd"/> was denied.
        /// </summary>
        Denied = 3
    }
}