using Rock.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.PersonalizationEngine.Model
{
    [Table( "_church_ccv_PersonalizationEngine_Linkage" )]
    [DataContract]
    class Linkage : Model<Linkage>, IRockEntity
    {
        [DataMember]
        public int PersonaId { get; set; }

        [DataMember]
        public int CampaignId { get; set; }
    }
}
