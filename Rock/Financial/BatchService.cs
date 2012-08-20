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
    /// Service class for Batch objects.
    /// </summary>
    public partial class BatchService : Service<Batch, DTO.Batch>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BatchService"/> class.
        /// </summary>
        public BatchService() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BatchService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public BatchService(IRepository<Batch> repository) : base(repository)
        {
        }

        /// <summary>
        /// Query DTO objects
        /// </summary>
        /// <returns>A queryable list of related DTO objects.</returns>
        public override IQueryable<DTO.Batch> QueryableDTO()
        {
            throw new System.NotImplementedException();
        }
    }
}