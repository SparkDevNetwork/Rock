using System.Text.Json.Serialization;

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes
{
    /// <summary>
    /// Represents the result of a tool call.
    /// </summary>
    internal sealed class ToolResultContent
    {
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string CallId { get; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string PluginName { get; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string ToolName { get; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public HistoryContentBag Result { get; }

        [JsonConstructor]
        public ToolResultContent( string toolName = null, string pluginName = null, string callId = null, HistoryContentBag result = null )
        {
            ToolName = toolName;
            PluginName = pluginName;
            CallId = callId;
            Result = result;
        }
    }
}