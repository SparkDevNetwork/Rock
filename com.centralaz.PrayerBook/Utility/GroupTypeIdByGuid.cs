// <copyright>
// Copyright by Central Christian Church
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
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;

namespace com.centralaz.Prayerbook.Utility
{
    public class GroupTypeIdByGuid
    {
        /// <summary>
        /// Returns the Id of the GroupType with the guid provided
        /// </summary>
        /// <param name="guid">The guid (as a string) of the GroupType you want the Id for.</param>
        /// <returns>The Id of the GroupType.</returns>
        public static int Get( string guid )
        {
            return new GroupTypeService( new RockContext() ).Get( Guid.Parse( guid ) ).Id;
        }
    }
}
