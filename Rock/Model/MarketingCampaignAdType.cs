//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Type of <see cref="Rock.Model.MarketingCampaignAd"/>.
    /// </summary>
    [Table( "MarketingCampaignAdType" )]
    [DataContract]
    public partial class MarketingCampaignAdType : Model<MarketingCampaignAdType>
    {
        /// <summary>
        /// Gets or sets a flag indicating if this MarketingCampaignAdType is part of the RockChMS core system/framework. 
        /// </summary>
        /// <value>
        ///   A <see cref="System.Boolean"/> flag that is <c>true</c> if this MarketingCAmpaignAdType is part of the RockChMS core system/framework; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name of the MarketingCampaignAdType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the name of the MarketingCampaignAdType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="DateRangeTypeEnum"/> enumeration that represents the type of date range that this DateRangeTypeEnum supports.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DateRangeTypeEnum"/> that represents the type of DateRangeTypeEnum is supported. When <c>DateRangeTypeEnum.SingleDate</c> a single date 
        /// will be supported; when <c>DateRangeTypeEnum.DateRange</c> a date range will be supported.
        /// </value>
        [DataMember]
        public DateRangeTypeEnum DateRangeType { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    public partial class MarketingCampaignAdTypeConfiguration : EntityTypeConfiguration<MarketingCampaignAdType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarketingCampaignAdTypeConfiguration" /> class.
        /// </summary>
        public MarketingCampaignAdTypeConfiguration()
        {
        }
    }

    /// <summary>
    /// Represents the type of DateRange that is supported.
    /// </summary>
    public enum DateRangeTypeEnum : byte
    {
        /// <summary>
        /// Allows a single date.
        /// </summary>
        SingleDate = 1,

        /// <summary>
        /// Allows a date range (start - end date)
        /// </summary>
        DateRange = 2
    }
}