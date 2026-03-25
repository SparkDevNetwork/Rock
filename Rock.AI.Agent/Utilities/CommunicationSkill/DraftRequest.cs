// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

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
