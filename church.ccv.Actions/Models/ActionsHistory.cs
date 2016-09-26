using Rock.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.Actions.Models
{
    [Table( "_church_ccv_Actions_History" )]
        [DataContract]
        public class ActionsHistory : Model<ActionsHistory>, IRockEntity
        {
        [DataMember]
            public int Baptized { get; set; }
	    
            [DataMember]
            public int Giving { get; set; }

            [DataMember]
            public int Worshipping { get; set; }

            [DataMember]
            public int Serving { get; set; }

            // CCV Considers this "Connected". But if / when they change it,
            // the concept will remain the same. Learning from peers.
            [DataMember]
            public int PeerLearning { get; set; }

            // CCV Considers this "Coaching". But if / when they change it,
            // the concept will remaing the same. They're teaching people. (not necessarily peers)
            [DataMember]
            public int Teaching { get; set; }

            // Young Adult Groups
            [DataMember]
            public int YAMember { get; set; }

            [DataMember]
            public int YACoach { get; set; }

            [DataMember]
            public int YACoCoach { get; set; }

            [DataMember]
            public int YAHost { get; set; }
            //
              
            // Neighborhood Groups  
            [DataMember]
            public int NHMember { get; set; }

            [DataMember]
            public int NHCoach { get; set; }

            [DataMember]
            public int NHCoCoach { get; set; }

            [DataMember]
            public int NHHost { get; set; }
            //
            
            // Next Gen Groups
            [DataMember]
            public int NGMember { get; set; }

            [DataMember]
            public int NGCoach { get; set; }

            [DataMember]
            public int NGCoCoach { get; set; }

            [DataMember]
            public int NGHost { get; set; }
            //

            // Next Steps Groups
            [DataMember]
            public int NSMember { get; set; }

            [DataMember]
            public int NSCoach { get; set; }

            [DataMember]
            public int NSCoCoach { get; set; }
            //
        }
}
