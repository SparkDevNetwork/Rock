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
    /// 
    /// </summary>
    [Table( "MarketingCampaignAdType" )]
    [DataContract]
    public partial class MarketingCampaignAdType : Model<MarketingCampaignAdType>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the date range.
        /// </summary>
        /// <value>
        /// The type of the date range.
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
    /// 
    /// </summary>
    public enum DateRangeTypeEnum : byte
    {
        /// <summary>
        /// 
        /// </summary>
        SingleDate = 1,

        /// <summary>
        /// 
        /// </summary>
        DateRange = 2
    }
}