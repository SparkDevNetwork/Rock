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
    /// Service class for Fund objects.
    /// </summary>
    public partial class FundService : Service<Fund, DTO.Fund>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundService"/> class.
        /// </summary>
        public FundService() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FundService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public FundService(IRepository<Fund> repository) : base(repository)
        {
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<DTO.Fund> QueryableDTO()
        {
            throw new System.NotImplementedException();
        }
    }
}