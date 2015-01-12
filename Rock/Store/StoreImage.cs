using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Store
{
    /// <summary>
    /// Represents a package in the store.
    /// </summary>
    public class StoreImage : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Package.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid of the Package.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the image URL. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the image URL.
        /// </value>
        public string ImageUrl {
            get
            {
                string storeServer = ConfigurationManager.AppSettings["RockStoreUrl"];
                return string.Format("{0}GetImage.ashx?guid={1}", storeServer, this.Guid);
            }
        }
    }
}
