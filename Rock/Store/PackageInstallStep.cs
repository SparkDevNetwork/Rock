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
    public class PackageInstallStep : StoreModel
    {
        /// <summary>
        /// Gets or sets the Id of the Package version. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the package version.
        /// </value>
        public int VersionId { get; set; }

        /// <summary>
        /// Gets or sets the Label of the Version. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Label of the Version.
        /// </value>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Install Package. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing URL of the Install Package.
        /// </value>
        public string InstallPackageUrl { get; set; }

        /// <summary>
        /// Gets or sets the post install instructions. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing post install instructions.
        /// </value>
        public string PostInstallInstructions { get; set; }
    }
}
