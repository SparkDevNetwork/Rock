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

using OpenTelemetry;
using OpenTelemetry.Logs;

namespace Rock.Observability
{
    /// <summary>
    /// Log processor that adds the category name to the logged message.
    /// </summary>
    internal class CategoryLogProcessor : BaseProcessor<LogRecord>
    {
        /// <inheritdoc/>
        public override void OnEnd( LogRecord data )
        {
            List<KeyValuePair<string, object>> attributes;

            if ( data.Attributes != null )
            {
                attributes = new List<KeyValuePair<string, object>>( data.Attributes.Count + 1 );
                attributes.AddRange( data.Attributes );
            }
            else
            {
                attributes = new List<KeyValuePair<string, object>>( 1 );
            }

            attributes.Add( new KeyValuePair<string, object>( "dotnet.ilogger.category", data.CategoryName ) );

            data.Attributes = attributes;

            base.OnEnd( data );
        }
    }

}
