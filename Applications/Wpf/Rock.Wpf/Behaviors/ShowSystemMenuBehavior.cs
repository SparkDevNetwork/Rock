// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Rock.Wpf.Behaviors;

namespace Rock.Wpf
{
    public static class ShowSystemMenuBehavior
    {
        #region TargetWindow

        public static Window GetTargetWindow( DependencyObject obj )
        {
            return ( Window ) obj.GetValue( TargetWindow );
        }

        public static void SetTargetWindow( DependencyObject obj, Window window )
        {
            obj.SetValue( TargetWindow, window );
        }

        public static readonly DependencyProperty TargetWindow = DependencyProperty.RegisterAttached( "TargetWindow", typeof( Window ), typeof( ShowSystemMenuBehavior ) );

        #endregion

        #region LeftButtonShowAt

        public static UIElement GetLeftButtonShowAt( DependencyObject obj )
        {
            return ( UIElement ) obj.GetValue( LeftButtonShowAt );
        }

        public static void SetLeftButtonShowAt( DependencyObject obj, UIElement element )
        {
            obj.SetValue( LeftButtonShowAt, element );
        }

        public static readonly DependencyProperty LeftButtonShowAt = DependencyProperty.RegisterAttached( "LeftButtonShowAt",
            typeof( UIElement ), typeof( ShowSystemMenuBehavior ),
            new UIPropertyMetadata( null, LeftButtonShowAtChanged ) );

        #endregion

        #region RightButtonShow

        public static bool GetRightButtonShow( DependencyObject obj )
        {
            return ( bool ) obj.GetValue( RightButtonShow );
        }

        public static void SetRightButtonShow( DependencyObject obj, bool arg )
        {
            obj.SetValue( RightButtonShow, arg );
        }

        public static readonly DependencyProperty RightButtonShow = DependencyProperty.RegisterAttached( "RightButtonShow",
            typeof( bool ), typeof( ShowSystemMenuBehavior ),
            new UIPropertyMetadata( false, RightButtonShowChanged ) );

        #endregion

        #region LeftButtonShowAt

        static void LeftButtonShowAtChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            var element = sender as UIElement;

            if ( element != null )
            {
                element.MouseLeftButtonDown += LeftButtonDownShow;
            }
        }

        static bool leftButtonToggle = true;

        static void LeftButtonDownShow( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            if ( leftButtonToggle )
            {
                var element = ( ( UIElement ) sender ).GetValue( LeftButtonShowAt );

                var showMenuAt = ( ( Visual ) element ).PointToScreen( new Point( 0, 0 ) );

                var targetWindow = ( ( UIElement ) sender ).GetValue( TargetWindow ) as Window;

                SystemMenuManager.ShowMenu( targetWindow, showMenuAt );

                leftButtonToggle = !leftButtonToggle;
            }
            else
            {
                leftButtonToggle = !leftButtonToggle;
            }
        }

        #endregion

        #region RightButtonShow handlers

        private static void RightButtonShowChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            var element = sender as UIElement;

            if ( element != null )
            {
                element.MouseRightButtonDown += RightButtonDownShow;
            }
        }

        static void RightButtonDownShow( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            var element = ( UIElement ) sender;

            var targetWindow = element.GetValue( TargetWindow ) as Window;

            var showMenuAt = targetWindow.PointToScreen( Mouse.GetPosition( ( targetWindow ) ) );

            SystemMenuManager.ShowMenu( targetWindow, showMenuAt );
        }

        #endregion       
    }
}