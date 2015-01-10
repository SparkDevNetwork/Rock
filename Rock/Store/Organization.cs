using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Store
{
    /// <summary>
    /// Represents a store category for packages.
    /// </summary>
    public class Organization : StoreModel
    {
        /// <summary>
        /// Gets or sets the key for the organization. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the key of the organization.
        /// </value>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Organization. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Organization.
        /// </value>
        public string Name { get; set; }
    }
}
