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

        public DraftResult() { }

        public string GetVerificationText( Rock.Model.Person currentPerson, Rock.Model.Person recipient )
        {
            string recipientAddr; // Either email or SMS
            if ( Type == AgentCommunicationType.Email )
            {
                recipientAddr = string.IsNullOrWhiteSpace( recipient.Email ) ? "" : " (" + recipient.Email + ")";
            }
            else if ( Type == AgentCommunicationType.Sms )
            {
                recipientAddr = "+1 555 123 1234";
            }
            else
            {
                recipientAddr = "";
            }

            return "Recipient:" + recipient.FullName + recipientAddr + "\r\n\r\n"
                 + "From:" + currentPerson.FullName + " (" + currentPerson.Email + ")\r\n\r\n"
                 + "Subject:" + "[subject]" + "\r\n\r\n"
                 + "Body:\r\n\r\n" + "[body]";
        }
    }
}
