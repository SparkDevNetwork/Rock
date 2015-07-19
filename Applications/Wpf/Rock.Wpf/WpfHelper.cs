// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Rock.Wpf
{
    public static class WpfHelper
    {
        /// <summary>
        /// Does a FadeIn of the control
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="speed">The speed.</param>
        public static void FadeIn( Control control, int speed = 0 )
        {
            control.Opacity = 0;
            control.Visibility = Visibility.Visible;
            Storyboard storyboard = new Storyboard();
            TimeSpan duration = new TimeSpan( 0, 0, 0, 0, (int)speed );
            DoubleAnimation fadeInAnimation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration( duration ) };
            Storyboard.SetTargetName( fadeInAnimation, control.Name );
            Storyboard.SetTargetProperty( fadeInAnimation, new PropertyPath( "Opacity", 1 ) );
            storyboard.Children.Add( fadeInAnimation );

            EventHandler handleCompleted = new EventHandler( ( sender, e ) =>
            {
                control.Visibility = Visibility.Visible;
            } );

            storyboard.Completed += handleCompleted;
            storyboard.Begin( control, HandoffBehavior.SnapshotAndReplace, true );
            
        }

        /// <summary>
        /// Does a FadeOut of the control
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="speed">The speed.</param>
        public static void FadeOut( Control control, int speed = 2000 )
        {
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
            storyboard.Begin( control, HandoffBehavior.SnapshotAndReplace, true  );
        }
    }
}
