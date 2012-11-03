//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Rock.Data;

namespace Rock.Cms
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "cmsMarketingCampaignAdType" )]
    public partial class MarketingCampaignAdType : Model<MarketingCampaignAdType>, IExportable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the date range.
        /// </summary>
        /// <value>
        /// The type of the date range.
        /// </value>
        public DateRangeTypeEnum DateRangeType {get; set;}

        /// <summary>
        /// Reads the specified id.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static MarketingCampaignAdType Read( int id )
        {
            return Read<MarketingCampaignAdType>( id );
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
            throw new System.NotImplementedException();
        }
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