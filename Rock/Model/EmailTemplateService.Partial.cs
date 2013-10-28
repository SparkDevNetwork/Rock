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
    /// Email Template POCO Service class
    /// </summary>
    public partial class EmailTemplateService 
    {
        /// <summary>
        /// Returns an <see cref="Rock.Model.EmailTemplate"/> by it's Guid identifier.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid unique identifier for the <see cref="Rock.Model.EmailTemplate"/> to search by.</param>
        /// <returns>The <see cref="Rock.Model.EmailTemplate"/> with a Guid identifier that matches the provided value. If no results were found, will return null.</returns>
        public EmailTemplate GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }
        
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.EmailTemplate">EmailTemplates</see> that are owned by a person. For public <see cref="Rock.Model.EmailTemplate">EmailTemplates</see> use null.
        /// </summary>
        /// <param name="personId"/>A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> to search by. For public <see cref="Rock.Model.EmailTemplate">EmailTemplates</see> use null.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.EmailTemplate">EmailTemplates</see> that are owned by the specified <see cref="Rock.Model.Person"/>.</returns>
        public IEnumerable<EmailTemplate> GetByPersonId( int? personId )
        {
            return Repository.Find( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) );
        }
    }
}
