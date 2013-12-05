//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

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
            return Repository.Find( t => ( t.TargetPersonId == targetPersonId || ( targetPersonId == null && t.TargetPersonId == null ) ) );
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.PersonViewed"/> entities by the Id of the Viewer <see cref="Rock.Model.Person"/>.
        /// </summary>
        /// <param name="viewerPersonId">A <see cref="System.Int32"/> representing the Id of the Viewer <see cref="Rock.Model.Person"/></param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.PersonViewed"/> entities where the Id of the viewer <see cref="Rock.Model.Person"/> matches the provided value.</returns>
        public IEnumerable<PersonViewed> GetByViewerPersonId( int? viewerPersonId )
        {
            return Repository.Find( t => ( t.ViewerPersonId == viewerPersonId || ( viewerPersonId == null && t.ViewerPersonId == null ) ) );
        }
    }
}
