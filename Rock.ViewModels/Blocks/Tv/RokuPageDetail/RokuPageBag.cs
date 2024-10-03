using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Tv.RokuPageDetail
{
    public class RokuPageBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a value indicating when the Page should be displayed in the navigation.
        /// </summary>
        public bool ShowInMenu { get; set; }

        /// <summary>
        /// Gets or sets the internal name to use when administering this page
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the description of the page.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the scenegraph for the roku page.
        /// </summary>
        public string Scenegraph { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable page views].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page views]; otherwise, <c>false</c>.
        /// </value>
        public RockCacheabilityBag RockCacheability { get; set; }
    }
}
