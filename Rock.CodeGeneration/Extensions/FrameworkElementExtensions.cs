using System.Windows;
using System.Windows.Media;

using Rock.CodeGeneration.Utility;

namespace Rock.CodeGeneration
{
    /// <summary>
    /// Extension methods to the WPF controls.
    /// </summary>
    public static class FrameworkElementExtensions
    {
        /// <summary>
        /// Gets the navigation controller for the given control.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>A <see cref="INavigation"/> instance or <c>null</c> if one wasn't found.</returns>
        public static INavigation Navigation( this FrameworkElement control )
        {
            for ( FrameworkElement parent = VisualTreeHelper.GetParent( control ) as FrameworkElement; parent != null; parent = VisualTreeHelper.GetParent( parent ) as FrameworkElement )
            {
                if ( parent is INavigation navigation )
                {
                    return navigation;
                }
            }

            return null;
        }
    }
}
