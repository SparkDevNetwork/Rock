using Rock.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.CCVPurpleWifiSync.Model
{
        [Table( "_church_ccv_PurpleWifiSync_PurpleUser" )]
        [DataContract]
        public class PurpleUser : Model<PurpleUser>, IRockEntity
        {
            [DataMember]
            public int PurpleId { get; set; }

            [DataMember]
            public int PersonAliasId { get; set; }
        }
}
