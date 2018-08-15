using System.Linq;
using System.Net;
using System.Web.Http;
using org.newpointe.Stars.Data;
using org.newpointe.Stars.Model;

using Rock.Rest;
using Rock.Rest.Filters;

namespace org.newpointe.Stars.Rest
{
    /// <summary>
    /// REST API for Stars Transactions
    /// </summary>
    public class StarsController : ApiController<Model.Stars>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StarsController"/> class.
        /// </summary>

        public StarsController() : base(new StarsService(new Data.StarsProjectContext())) { }
    }
}
