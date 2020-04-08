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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionTypesFieldType : SelectFromListFieldType
    {
        /// <summary>
        /// Gets the list source.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        internal override Dictionary<string, string> ListSource
        {
            get
            {
                return new ConnectionTypeService( new RockContext() )
                    .Queryable().AsNoTracking()
                    .OrderBy( o => o.Name )
                    .Select( o => new
                    {
                        o.Guid,
                        o.Name,
                    } )
                    .ToDictionary( c => c.Guid.ToString(), c => c.Name );
            }
        }
    }
}
