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

using System.Collections.Generic;

using Rock.Net;

namespace Rock.AI.Agent
{
    internal class ProxyFunction
    {
        private readonly AgentRequestContext _agentRequestContext;

        private readonly RockRequestContext _rockRequestContext;

        public ProxyFunction( AgentRequestContext requestContext, RockRequestContext rockRequestContext )
        {
            _agentRequestContext = requestContext;
            _rockRequestContext = rockRequestContext;
        }

        public string Run( AgentFunction function, string promptAsJson )
        {
            var promptAsDictionary = promptAsJson.FromJsonOrNull<Dictionary<string, object>>();
            var mergeFields = _rockRequestContext.GetCommonMergeFields();

            mergeFields["AgentContext"] = _agentRequestContext;

            if ( promptAsDictionary != null )
            {
                foreach ( var kvp in promptAsDictionary )
                {
                    if ( kvp.Value is string stringValue )
                    {
                        mergeFields.TryAdd( kvp.Key, stringValue );
                    }
                    else if ( kvp.Value is bool boolValue )
                    {
                        mergeFields.TryAdd( kvp.Key, boolValue );
                    }
                    else if ( kvp.Value is int intValue )
                    {
                        mergeFields.TryAdd( kvp.Key, intValue );
                    }
                    else if ( kvp.Value is double doubleValue )
                    {
                        mergeFields.TryAdd( kvp.Key, doubleValue );
                    }
                    else if ( kvp.Value is null )
                    {
                        mergeFields.TryAdd( kvp.Key, null );
                    }
                    else
                    {
                        mergeFields.TryAdd( kvp.Key, kvp.Value.ToStringSafe() );
                    }
                }
            }

            // Because only administrators (or those granted access by an
            // administrator) can create or edit functions, we can safely
            // just enable all lava commands.
            return function.Prompt.ResolveMergeFields( mergeFields, "All" );
        }
    }
}
