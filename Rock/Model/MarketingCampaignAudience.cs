//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an audience that a <see cref="Rock.Model.MarketingCampaign"/> is targeted towards. A <see cref="Rock.Model.MarketingCampaign"/> can be promoted/targeted towards one or more
    /// primary or secondary audiences.
    /// </summary>
    [Table( "MarketingCampaignAudience" )]
    [DataContract]
    public partial class MarketingCampaignAudience : Model<MarketingCampaignAudience>
    {
        /// <summary>
        /// Gets or sets the MarketingCampaignId of the <see cref="Rock.Model.MarketingCampaign"/> that is being promoted to the Audience.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the MarketingCampaignId of the <see cref="Rock.Model.MarketingCampaign"/>.
        /// </value>
        [DataMember]
        public int MarketingCampaignId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of an AudienceType <see cref="Rock.Model.DefinedValue"/> that that this <see cref="Rock.Model.MarketingCampaign"/> is promoted/targeted to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the AudienceType that is being promoted to.
        /// </value>
        [DataMember]
        public int AudienceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this audience is a primary audience for the <see cref="Rock.Model.MarketingCampaign"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Audience is a primary audience for the <see cref="Rock.Model.MarketingCampaign"/>; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Gets the Name of this MarketingCampaignAudience.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the MarketingCampaignAudience.
        /// </value>
        [DataMember]
        public virtual string Name
        {
            get
            {
                return ( AudienceTypeValueId > 0 ? DefinedValueCache.Read(AudienceTypeValueId).Name : string.Empty );
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.MarketingCampaign"/> that this being promoted to this Audience.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.MarketingCampaign"/> that is being promoted to this Audience.
        /// </value>
        [DataMember]
        public virtual MarketingCampaign MarketingCampaign { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the AudienceType.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedValue"/> representing the AudienceType.
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