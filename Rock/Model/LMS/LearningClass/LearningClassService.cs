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
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    public partial class LearningClassService
    {
        /// <summary>
        /// Gets class with it's related <see cref="GroupType"/> for the specified id key.
        /// </summary>
        /// <param name="idKey">The idKey of the <see cref="LearningClass"/> for which to retrieve the default class.</param>
        /// <returns>The first active learning class.</returns>
        public IQueryable<GroupTypeRole> GetClassRoles( string idKey )
        {
            var id = IdHasher.Instance.GetId( idKey ).ToIntSafe();
            return id > 0 ? GetClassRoles( id ) : Array.Empty<GroupTypeRole>().AsQueryable();
        }

        /// <summary>
        /// Gets class with it's related <see cref="GroupType"/> for the specified id.
        /// </summary>
        /// <param name="id">The identifier of the <see cref="LearningClass"/> for which to retrieve the default class.</param>
        /// <returns>The first active learning class.</returns>
        public IQueryable<GroupTypeRole> GetClassRoles( int id )
        {
            var groupTypeId = Queryable().Where( c => c.Id == id ).Select( c => c.GroupTypeId ).FirstOrDefault();

            return
                groupTypeId > 0 ?
                new GroupTypeRoleService( ( RockContext ) Context ).GetByGroupTypeId( groupTypeId ).OrderBy( t => t.Order ) :
                Array.Empty<GroupTypeRole>().AsQueryable();
        }
    }
}