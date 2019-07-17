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
        public string IconCssClass { get; private set; }

        public IconCssClassAttribute( string iconCssClass )
        {
            IconCssClass = iconCssClass;
        }
    }
}
