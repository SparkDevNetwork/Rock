using System.Collections.Generic;

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
        public List<Rock.Model.Person> Recipients { get; set; }

        public string EmailClosingPhrase { get; private set; }  // Email may use

        public DraftRequest(
            AgentCommunicationType type,
            string subjectHint,
            string draftGuidance,
            string relevantData,
            string tone,
            Rock.Model.Person currentPerson,
            List<Rock.Model.Person> recipients,
            string emailSignature )
        {
            Type = type;
            SubjectHint = subjectHint;
            DraftGuidance = draftGuidance;
            RelevantData = relevantData;
            Tone = tone;
            CurrentPerson = currentPerson;
            Recipients = recipients;
            EmailClosingPhrase = emailSignature;
        }
    }
}
