//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// Service class for Transaction objects.
    /// </summary>
    public partial class TransactionService : Service<Transaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        public TransactionService() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public TransactionService(IRepository<Transaction> repository) : base(repository)
        {
        }
    }
}