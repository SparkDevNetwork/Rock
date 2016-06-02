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
using Rock.Model;
using Rock.Data;

namespace com.centralaz.Prayerbook.Utility
{
    public class MostRecentBook
    {
        /// <summary>
        /// Gets the most recent book entry in the db.
        /// </summary>
        /// <returns>a Book object.</returns>
        public static Group Get(RockContext context)
        {
            int groupTypeId = GroupTypeIdByGuid.Get( com.centralaz.Prayerbook.SystemGuid.GroupType.BOOKS_GROUPTYPE );
            
            Group book = new GroupService(context).Queryable()
                .Where( t => t.GroupTypeId == groupTypeId )
                .OrderByDescending( t => t.CreatedDateTime )
                .FirstOrDefault();

            if ( book == null )
            {
                book = new Group();
                book.GroupTypeId = groupTypeId;
            }
            return book;
        }
    }
}
