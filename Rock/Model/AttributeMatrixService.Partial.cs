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
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Field.Types;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AttributeMatrixService : Service<AttributeMatrix>
    {
        /// <summary>
        /// Gets the orphaned attribute matrices.
        /// </summary>
        /// <returns></returns>
        [Obsolete("No longer used. It could have returned false positives, especially if Plugins reference this.") ]
        [RockObsolete("12.5")]
        public IEnumerable<AttributeMatrix> GetOrphanedAttributeMatrices()
        {
            /* 07-22-2021 MDP
              
            After researching why there were sometimes performance issues with this query, we found a few things:
            1) It could return false positives, especially if Plugins reference the AttributeMatrix table.
            2) There could be performance issues that make that query timeout, depending on the system.
             
            */

            return new List<AttributeMatrix>().AsQueryable();
        }
    }
}
