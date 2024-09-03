using System;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.CampusDetail
{
    public class CampusTopicBag
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the Campus Topics.
        /// The Campus Topics is a Defined Value.
        /// </summary>
        /// <value>The Campus Topic.</value>
        public ListItemBag Type { get; set; }

        /// <summary>
        /// Gets or sets the Email.
        /// </summary>
        /// <value>The Email.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets if the Campus Topic is Public.
        /// </summary>
        /// <value>The boolean value whether the Campus Topic is Public or not.</value>
        public bool IsPublic { get; set; }
    }
}