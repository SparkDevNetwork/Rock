//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// Service class for Pledge objects.
    /// </summary>
    public partial class PledgeService : Service<Pledge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PledgeService"/> class.
        /// </summary>
        public PledgeService() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PledgeService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public PledgeService(IRepository<Pledge> repository) : base(repository)
        {
        }
    }
}