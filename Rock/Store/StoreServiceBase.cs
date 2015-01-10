using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Store
{
    /// <summary>
    /// Base service class for store services
    /// </summary>
    public class StoreServiceBase
    {
        /// <summary>
        /// Internal variable to store the url to the store.
        /// </summary>
        protected string _rockStoreUrl = string.Empty;
        protected int _clientTimeout = 12000;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreServiceBase"/> class.
        /// </summary>
        public StoreServiceBase()
        {
            // set configuration variables
            _rockStoreUrl = ConfigurationManager.AppSettings["RockStoreUrl"];
        }
    }
}
