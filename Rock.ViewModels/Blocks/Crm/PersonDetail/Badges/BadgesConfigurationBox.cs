using System.Collections.Generic;

using Rock.ViewModels.Crm;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.Badges
{
    /// <summary>
    /// Contains all the initial configuration data required to render the
    /// Badges block.
    /// </summary>
    public class BadgesConfigurationBox
    {
        /// <summary>
        /// Gets or sets the person key identifier of the person being viewed.
        /// </summary>
        /// <value>The person key identifier of the person being viewed.</value>
        public string PersonKey { get; set; }

        /// <summary>
        /// Gets or sets the top left rendered badge content.
        /// </summary>
        /// <value>The top left rendered badge content.</value>
        public List<RenderedBadgeBag> TopLeftBadges { get; set; }

        /// <summary>
        /// Gets or sets the top middle rendered badge content.
        /// </summary>
        /// <value>The top middle rendered badge content.</value>
        public List<RenderedBadgeBag> TopMiddleBadges { get; set; }

        /// <summary>
        /// Gets or sets the top right rendered badge content.
        /// </summary>
        /// <value>The top right rendered badge content.</value>
        public List<RenderedBadgeBag> TopRightBadges { get; set; }

        /// <summary>
        /// Gets or sets the bottom left rendered badge content.
        /// </summary>
        /// <value>The bottom left rendered badge content.</value>
        public List<RenderedBadgeBag> BottomLeftBadges { get; set; }

        /// <summary>
        /// Gets or sets the bottom right rendered badge content.
        /// </summary>
        /// <value>The bottom right rendered badge content.</value>
        public List<RenderedBadgeBag> BottomRightBadges { get; set; }
    }
}
