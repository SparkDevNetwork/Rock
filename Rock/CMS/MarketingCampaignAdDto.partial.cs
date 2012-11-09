using Rock.Data;

namespace Rock.Cms
{
    public partial class MarketingCampaignAdDto : IDto
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                if ( _name == null )
                {
                    var nameEntity = MarketingCampaignAdType.Read( this.MarketingCampaignAdTypeId );
                    if ( nameEntity != null )
                    {
                        _name = nameEntity.Name;
                    }
                }
                return _name ?? string.Empty;
            }
        }
        private string _name;
    }
}