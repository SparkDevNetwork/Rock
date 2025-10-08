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
//

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