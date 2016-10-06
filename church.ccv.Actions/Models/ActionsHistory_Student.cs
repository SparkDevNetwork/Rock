using Rock.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.Actions.Models
{
        [Table( "_church_ccv_Actions_History_Student" )]
        [DataContract]
        public class ActionsHistory_Student : Model<ActionsHistory_Student>, IRockEntity
        {
            [DataMember]
            public DateTime Date { get; set; }

            [DataMember]
            public int CampusId { get; set; }

            [DataMember]
            public int RegionId { get; set; }
        
            // The total number of people tested against, within the scope of CampusId and RegionId.
            [DataMember]
            public int TotalPeople { get; set; }

            // --- These are the actions that people can perform ---
            [DataMember]
            public int Baptised { get; set; }

            [DataMember]
            public int ERA { get; set; }
	    
            [DataMember]
            public int Give { get; set; }

            [DataMember]
            public int Member { get; set; }

            [DataMember]
            public int Serving { get; set; }

            // CCV Considers this "Connected". But if / when they change it,
            // the concept will remain the same. Learning from peers.
            [DataMember]
            public int PeerLearning { get; set; }
            
            // CCV Considers this "Coached". This would be someone in a group where
            // someone is helping them grow, like a NextStep Group
            [DataMember]
            public int Mentored { get; set; }

            // CCV Considers this "Coaching". But if / when they change it,
            // the concept will remaing the same. They're teaching people. (not necessarily peers)
            [DataMember]
            public int Teaching { get; set; }
            // ---

            // --- Breakdowns for actions that depend on group types ---
            // It tells us how many people are in each type of group per action.

            // Peer Learning group types
            [DataMember]
            public int PeerLearning_NG { get; set; }

            // Mentored Group Types
            [DataMember]
            public int Mentored_NG { get; set; }

            // Teaching Group Types
            [DataMember]
            public int Teaching_Undefined { get; set; }
            // ---
        }
}
