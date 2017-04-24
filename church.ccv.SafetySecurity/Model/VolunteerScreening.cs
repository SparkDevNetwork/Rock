using Rock.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.SafetySecurity.Model
{
        [Table( "_church_ccv_SafetySecurity_VolunteerScreening" )]
        [DataContract]
        public class VolunteerScreening : Model<VolunteerScreening>, IRockEntity
        {
            public enum Types
            {
                Legacy,
                Normal
            }

            [DataMember]
            public DateTime Date { get; set; }

            [DataMember]
            public int PersonAliasId { get; set; }

            [DataMember]
            public int Type { get; set; }

            [DataMember]
            public int? Application_WorkflowTypeId { get; set; }

            [DataMember]
            public int? Application_WorkflowId { get; set; }

            [DataMember]
            public DateTime? BGCheck_Result_Date { get; set; }

            [DataMember]
            public Guid? BGCheck_Result_DocGuid { get; set; }

            [DataMember]
            public string BGCheck_Result_Value { get; set; }

            // These are legacy values used for scanning in old applications
            [DataMember]
            public int? Legacy_Application_DocFileId { get; set; }
        
            [DataMember]
            public int? Legacy_CharacterReference1_DocFileId { get; set; }

            [DataMember]
            public int? Legacy_CharacterReference2_DocFileId { get; set; }

            [DataMember]
            public int? Legacy_CharacterReference3_DocFileId { get; set; }
            //
        }
}
