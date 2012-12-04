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
    public partial class EmailTemplateService : Service<EmailTemplate, EmailTemplateDto>
    {
        /// <summary>
        /// Gets Email Template by Guid
        /// </summary>
        /// <param name="guid">Guid.</param>
        /// <returns>EmailTemplate object.</returns>
        public EmailTemplate GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }
        
        /// <summary>
        /// Gets Email Templates by Person Id
        /// </summary>
        /// <param name="personId">Person Id.</param>
        /// <returns>An enumerable list of EmailTemplate objects.</returns>
        public IEnumerable<EmailTemplate> GetByPersonId( int? personId )
        {
            return Repository.Find( t => ( t.PersonId == personId || ( personId == null && t.PersonId == null ) ) );
        }
    }
}
