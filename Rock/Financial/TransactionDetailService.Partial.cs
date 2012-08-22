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
    /// Service class for TransactionDetail objects
    /// </summary>
    public partial class TransactionDetailService : Service<TransactionDetail, TransactionDetailDTO>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDetailService"/> class.
        /// </summary>
        public TransactionDetailService() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionDetailService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public TransactionDetailService(IRepository<TransactionDetail> repository) : base(repository)
        {
        }
    }
}