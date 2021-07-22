using System;

namespace Rock.Model
{
    /// <summary>
    /// Represents a GroupTypePath object in Rock.
    /// </summary>
    [RockObsolete( "1.12" )]
    [Obsolete( "Use CheckinAreaPath instead" )]
    public class GroupTypePath
    {
        /// <summary>
        /// Gets or sets the ID of the GroupType.
        /// </summary>
        /// <value>
        /// ID of the GroupType.
        /// </value>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the full associated ancestor path (of group type associations). 
        /// </summary>
        /// <value>
        /// Full path of the ancestor group type associations. 
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Returns the Path of the GroupTypePath
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Path;
        }
    }
}
