using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.AI.Agent.Utilities.CommunicationSkill
{
    internal sealed class DraftRequest
    {
        public AgentCommunicationType Type { get; private set; }
        public string SubjectHint { get; private set; }   // SMS may ignore
        public string DraftGuidance { get; private set; }
        public string RelevantData { get; private set; }
        public string Tone { get; private set; }
        public Rock.Model.Person CurrentPerson { get; private set; }
        public Rock.Model.Person Recipient { get; set; }

        public DraftRequest(
            AgentCommunicationType type,
            string subjectHint,
            string draftGuidance,
            string relevantData,
            string tone,
            Rock.Model.Person currentPerson,
            Rock.Model.Person recipient )
        {
            Type = type;
            SubjectHint = subjectHint;
            DraftGuidance = draftGuidance;
            RelevantData = relevantData;
            Tone = tone;
            CurrentPerson = currentPerson;
            Recipient = recipient;
        }
    }
}
