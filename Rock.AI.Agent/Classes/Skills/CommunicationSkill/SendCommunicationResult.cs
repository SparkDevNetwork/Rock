using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Classes.Skills.CommunicationSkill
{
    internal class SendCommunicationResult
    {
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string CommunicationUrl { get; set; }

        public string CommunicationKey { get; set; }
    }
}
