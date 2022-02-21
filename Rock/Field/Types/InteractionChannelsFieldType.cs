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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Select multiple Interaction Channels from a checkbox list. Stored as a comma-delimited list of InteractionChannel Guids
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class InteractionChannelsFieldType : SelectFromListFieldType
    {
        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> GetListSource( Dictionary<string, ConfigurationValue> configurationValues )
        {
            return new InteractionChannelService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToDictionary( c => c.Guid.ToString(), c => c.Name );
        }
    }
}