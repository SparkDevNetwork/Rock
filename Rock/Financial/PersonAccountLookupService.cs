//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// Service class for PersonAccountLookup objects.
    /// </summary>
    public partial class PersonAccountLookupService : Service<PersonAccountLookup, PersonAccountLookupDTO>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAccountLookupService"/> class.
        /// </summary>
        public PersonAccountLookupService() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonAccountLookupService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public PersonAccountLookupService(IRepository<PersonAccountLookup> repository) : base(repository)
        {
        }

        /// <summary>
        /// Creates a new model
        /// </summary>
        /// <returns></returns>
        public override PersonAccountLookup CreateNew()
        {
            return new PersonAccountLookup();
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<PersonAccountLookupDTO> QueryableDTO()
        {
            return this.Queryable().Select( m => new PersonAccountLookupDTO( m ) );
        }
    }
}