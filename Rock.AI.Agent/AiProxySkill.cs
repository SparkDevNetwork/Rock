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

using System.Linq;

namespace Rock.AI.Agent
{
    internal class AiProxySkill
    {
        private AgentRequestContext _requestContext;

        public AiProxySkill( AgentRequestContext requestContext )
        {
            _requestContext = requestContext;
        }

        public string RunLavaFromJson( string promptAsJson, string template ) // template would be some kind of key or id in the future.
        {
            var test = _requestContext.ChatHistory.FirstOrDefault().Content;

            // Be sure to add the request context as a Lava merge field.

            return $"Lava skill called with template: {template}";
        }
    }
}
