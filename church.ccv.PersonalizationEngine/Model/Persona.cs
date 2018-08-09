using Rock.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.PersonalizationEngine.Model
{
    [Table( "_church_ccv_PersonalizationEngine_Persona" )]
    [DataContract]
    public class Persona : Model<Persona>, IRockEntity
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string RockSQL { get; set; }
    }
}
