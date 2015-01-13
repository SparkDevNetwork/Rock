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
    public class PackageCategory : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the Package Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the Package Category.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Name of the Package Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the Package Category.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Package Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Guid"/> representing the Guid of the Package Category.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the CSS Icon Class of the Package Category. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the CSS Icon Class of the Package Category.
        /// </value>
        public string IconCssClass { get; set; }
    }
}
