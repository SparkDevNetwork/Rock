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
    /// Person Viewed POCO Service class
    /// </summary>
    public partial class PersonViewedService 
    {
        /// <summary>
        /// Gets Person Vieweds by Target Person Id
        /// </summary>
        /// <param name="targetPersonId">Target Person Id.</param>
        /// <returns>An enumerable list of PersonViewed objects.</returns>
        public IEnumerable<PersonViewed> GetByTargetPersonId( int? targetPersonId )
        {
            return Repository.Find( t => ( t.TargetPersonId == targetPersonId || ( targetPersonId == null && t.TargetPersonId == null ) ) );
        }
        
        /// <summary>
        /// Gets Person Vieweds by Viewer Person Id
        /// </summary>
        /// <param name="viewerPersonId">Viewer Person Id.</param>
        /// <returns>An enumerable list of PersonViewed objects.</returns>
        public IEnumerable<PersonViewed> GetByViewerPersonId( int? viewerPersonId )
        {
            return Repository.Find( t => ( t.ViewerPersonId == viewerPersonId || ( viewerPersonId == null && t.ViewerPersonId == null ) ) );
        }
    }
}
