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
    public class PackageVersion : StoreModel
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
        /// Gets or sets the version's label. 
        /// </summary>
        /// <value>
        /// A <see cref="System.string"/> representing the versions lable.
        /// </value>
        public string VersionLabel { get; set; }

        /// <summary>
        /// Gets or sets the required Rock version. 
        /// </summary>
        /// <value>
        /// A <see cref="System.string"/> required Rock version.
        /// </value>
        public string RequiredRockVersion { get; set; }

        /// <summary>
        /// Gets or sets the version description. 
        /// </summary>
        /// <value>
        /// A <see cref="System.string"/> representing the version description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the documentation URL. 
        /// </summary>
        /// <value>
        /// A <see cref="System.string"/> representing URL to the version's documentation.
        /// </value>
        public string DocumentationUrl { get; set; }

        /// <summary>
        /// Gets or sets the date added. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the version was added.
        /// </value>
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Gets or sets the version ratings. 
        /// </summary>
        /// <value>
        /// A list <see cref="List<Rock.Store.PackageVersionRating>"/> representing the version ratings.
        /// </value>
        public List<PackageVersionRating> Ratings { get; set; }

        /// <summary>
        /// Gets or sets the version screenshots. 
        /// </summary>
        /// <value>
        /// A list <see cref="List<Rock.Store.StoreImage>"/> representing the version screenshots.
        /// </value>
        public List<StoreImage> ScreenShots { get; set; }
    }
}
