using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace ContributionStatementApp
{
    public static class WpfHelper
    {

        // <summary>
        /// Toggles the fade.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="speed">The speed.</param>
        public static void FadeIn( Control control, int speed = 0 )
        {
            // TODO Move this to a shared dll
            control.Opacity = 0;
            control.Visibility = Visibility.Visible;
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan( 0, 0, 0, 0, (int)speed );
            DoubleAnimation fadeInAnimation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration( duration ) };
            Storyboard.SetTargetName( fadeInAnimation, control.Name );
            Storyboard.SetTargetProperty( fadeInAnimation, new PropertyPath( "Opacity", 1 ) );
            storyboard.Children.Add( fadeInAnimation );
            storyboard.Begin( control );
        }

        /// <summary>
        /// Fades the out.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="speed">The speed.</param>
        public static void FadeOut( Control control, int speed = 2000 )
        {
            // TODO Move this to a shared dll
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan( 0, 0, 0, 0, (int)speed );
            DoubleAnimation fadeOutAnimation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration( duration ) };
            Storyboard.SetTargetName( fadeOutAnimation, control.Name );
            Storyboard.SetTargetProperty( fadeOutAnimation, new PropertyPath( "Opacity", 0 ) );
            storyboard.Children.Add( fadeOutAnimation );

            EventHandler handleCompleted = new EventHandler( ( sender, e ) =>
            {
                control.Visibility = Visibility.Collapsed;
            } );

            storyboard.Completed += handleCompleted;
            storyboard.Begin( control );
        }
    }
}
