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
    public class StoreService
    {
        /// <summary>
        /// Internal variable to store the url to the store.
        /// </summary>
        protected string _rockStoreUrl = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreService"/> class.
        /// </summary>
        public StoreService()
        {
            // set configuration variables
            _rockStoreUrl = ConfigurationManager.AppSettings["RockStoreUrl"];
        }
    }
}
