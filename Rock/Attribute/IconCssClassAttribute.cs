using System;

namespace Rock.Attribute
{
    /// <summary>
    /// Defines a CSS icon that will be used in conjunction with this item.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public class IconCssClassAttribute : System.Attribute
    {
        /// <summary>
        /// Gets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IconCssClassAttribute"/> class.
        /// </summary>
        /// <param name="iconCssClass">The icon CSS class.</param>
        public IconCssClassAttribute( string iconCssClass )
        {
            IconCssClass = iconCssClass;
        }
    }
}
