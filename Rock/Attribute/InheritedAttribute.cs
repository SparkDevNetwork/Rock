//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Attribute
{
    /// <summary>
    /// Helper class for inherited attributes
    /// </summary>
    [Serializable]
    public class InheritedAttribute
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        public string GroupType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InheritedAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="url">The URL.</param>
        /// <param name="groupType">Type of the group.</param>
        public InheritedAttribute( string name, string description, string url, string groupType )
        {
            Name = name;
            Description = description;
            Url = url;
            GroupType = groupType;
        }
    }

}
