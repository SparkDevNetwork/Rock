using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Store
{
    /// <summary>
    /// Represents a store promo.
    /// </summary>
    public class InstalledPackage : StoreModel
    {
        /// <summary>
        /// Gets or sets the PackageId of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PackageId.
        /// </value>
        public int PackageId { get; set; }

        /// <summary>
        /// Gets or sets the package label of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the package label.
        /// </value>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the VersionId of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the VersionId.
        /// </value>
        public int VersionId { get; set; }

        /// <summary>
        /// Gets or sets the version label of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the version label.
        /// </value>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the VendorId of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the VendorId.
        /// </value>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the vendor name of the installation. 
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the vendor name.
        /// </value>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets the install date/time. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the install date/time.
        /// </value>
        public DateTime InstallDateTime { get; set; }
       
    }
}
