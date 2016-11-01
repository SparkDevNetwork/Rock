using Rock.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace church.ccv.Actions.Models
{
        [Table( "_church_ccv_Actions_History_Adult_Person" )]
        [DataContract]
        public class ActionsHistory_Adult_Person : Model<ActionsHistory_Adult_Person>, IRockEntity
        {
            [DataMember]
            public DateTime Date { get; set; }

            [DataMember]
            public int PersonAliasId { get; set; }
        
            // --- These are the actions that people can perform ---
            [DataMember]
            public DateTime? Baptised { get; set; }

            [DataMember]
            public bool ERA { get; set; }
	    
            [DataMember]
            public bool Give { get; set; }

            [DataMember]
            public DateTime? Member { get; set; }

            [DataMember]
            public bool Serving { get; set; }

            // CCV Considers this "Connected". But if / when they change it,
            // the concept will remain the same. Learning from peers.
            [DataMember]
            public bool PeerLearning { get; set; }
            
            // CCV Considers this "Coached". This would be someone in a group where
            // someone is helping them grow, like a NextStep Group
            [DataMember]
            public bool Mentored { get; set; }

            // CCV Considers this "Coaching". But if / when they change it,
            // the concept will remaing the same. They're teaching people. (not necessarily peers)
            [DataMember]
            public bool Teaching { get; set; }
            // ---

            // --- Breakdowns for actions that depend on group types ---
            // It tells us how many people are in each type of group per action.

            // Peer Learning group types
            [DataMember]
            public bool PeerLearning_NH { get; set; }

            [DataMember]
            public bool PeerLearning_YA { get; set; }

            // Mentored Group Types
            [DataMember]
            public bool Mentored_NH { get; set; }

            [DataMember]
            public bool Mentored_YA { get; set; }

            [DataMember]
            public bool Mentored_NS { get; set; }

            // Teaching Group Types
            [DataMember]
            public bool Teaching_NH_SubSection { get; set; }

            [DataMember]
            public bool Teaching_NH { get; set; }

            [DataMember]
            public bool Teaching_YA_Section { get; set; }

            [DataMember]
            public bool Teaching_YA { get; set; }

            [DataMember]
            public bool Teaching_NS_SubSection { get; set; }

            [DataMember]
            public bool Teaching_NS { get; set; }

            [DataMember]
            public bool Teaching_NG_Section { get; set; }

            [DataMember]
            public bool Teaching_NG { get; set; }
            // ---
        }
}
