using System.Text.Json.Serialization;

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes
{
    /// <summary>
    /// Represents the result of a function call.
    /// </summary>
    internal sealed class ToolResultContent
    {
        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string CallId { get; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string PluginName { get; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public string FunctionName { get; }

        [JsonIgnore( Condition = JsonIgnoreCondition.WhenWritingNull )]
        public HistoryContentBag Result { get; }

        [JsonConstructor]
        public ToolResultContent( string functionName = null, string pluginName = null, string callId = null, HistoryContentBag result = null )
        {
            FunctionName = functionName;
            PluginName = pluginName;
            CallId = callId;
            Result = result;
        }
    }
}