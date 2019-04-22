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

namespace Rock.Model
{
    /// <summary>
    /// Data Access/service class for <see cref="Rock.Model.PersonViewed"/> entities.
    /// </summary>
    public partial class PersonViewedService 
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.PersonViewed"/> entities by the Id of the Target <see cref="Rock.Model.Person"/>
        /// </summary>
        /// <param name="targetPersonId">An <see cref="System.Int32"/> representing the Id of the Target <see cref="Rock.Model.Person"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PersonViewed"/> entities where the Id of the target <see cref="Rock.Model.Person"/> matches the provided value.</returns>
        public IEnumerable<PersonViewed> GetByTargetPersonId( int? targetPersonId )
        {
            return Queryable().Where( t => ( t.TargetPersonAlias != null && t.TargetPersonAlias.PersonId == targetPersonId ) || ( targetPersonId == null && t.TargetPersonAlias == null ) );
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.PersonViewed"/> entities by the Id of the Viewer <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="viewerPersonId">A <see cref="System.Int32"/> representing the Id of the Viewer <see cref="Rock.Model.Person"/></param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PersonViewed"/> entities where the Id of the viewer <see cref="Rock.Model.Person"/> matches the provided value.</returns>
        public IEnumerable<PersonViewed> GetByViewerPersonId( int? viewerPersonId )
        {
            return Queryable().Where( t => ( t.ViewerPersonAlias != null && t.ViewerPersonAlias.PersonId == viewerPersonId ) || ( viewerPersonId == null && t.ViewerPersonAlias == null ) );
        }
    }
}
