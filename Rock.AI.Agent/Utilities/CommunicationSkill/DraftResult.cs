using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Utilities.CommunicationSkill
{
    internal sealed class DraftResult
    {
        public AgentCommunicationType Type { get; set; }

        public string CommunicationIdKey { get; set; }
        //public MediumDraftContent Content { get; set; }
        public string CommunicationUrl { get; set; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string Subject { get; set; }

        public string Body { get; set; }

        public string VerificationText { get; set; }

        public DraftResult() { }
    }
}
