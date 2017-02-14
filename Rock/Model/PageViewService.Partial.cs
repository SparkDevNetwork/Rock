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

namespace Rock.Model
{
    /// <summary>
    /// Data Access/service class for <see cref="Rock.Model.PageView"/> entities.
    /// </summary>
    public partial class PageViewService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.PageView" /> entities by the Id of the <see cref="Rock.Model.Page" />
        /// </summary>
        /// <param name="pageId">The page identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.PageView" /> entities where the Id of the <see cref="Rock.Model.Page" /> matches the provided value.
        /// </returns>
        public IEnumerable<PageView> GetByPageId( int? pageId )
        {
            return Queryable().Where( t => ( t.PageId != null && t.PageId == pageId ) || ( pageId == null && t.PageId == null ) );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.PageView" /> entities by the Id of the Viewer <see cref="Rock.Model.Person" />.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.PageView" /> entities where the Id of the viewer <see cref="Rock.Model.Person" /> matches the provided value.
        /// </returns>
        public IEnumerable<PageView> GetByPersonId( int? personId )
        {
            return Queryable().Where( t => ( t.PersonAlias != null && t.PersonAlias.PersonId == personId ) || ( personId == null && t.PersonAlias == null ) );
        }
    }
}
