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

namespace Rock.AI.Agent
{
    internal class ProxyFunction
    {
        private readonly AgentRequestContext _requestContext;

        public ProxyFunction( AgentRequestContext requestContext )
        {
            _requestContext = requestContext;
        }

        public string RunLavaFromJson( string promptAsJson, string template ) // template would be some kind of key or id in the future.
        {
            var promptAsDictionary = promptAsJson.FromJsonOrNull<Dictionary<string, object>>();
            var mergeFields = new Dictionary<string, object>
            {
                ["AgentContext"] = _requestContext
            };

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

            return template.ResolveMergeFields( mergeFields );
        }
    }
}
