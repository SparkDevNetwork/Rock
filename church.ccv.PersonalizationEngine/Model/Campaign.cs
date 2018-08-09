using Rock.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.PersonalizationEngine.Model
{
    [Table( "_church_ccv_PersonalizationEngine_Campaign" )]
    [DataContract]
    public class Campaign : Model<Campaign>, IRockEntity
    {
        public enum CampaignType
        {
            DashboardCard,
            MobileAppCard,
            Website
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string ContentJson { get; set; }
    }
}
