using System.Windows;
using System.Windows.Media;

using BlockGenerator.Utility;

namespace BlockGenerator
{
    public static class FrameworkElementExtensions
    {
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
