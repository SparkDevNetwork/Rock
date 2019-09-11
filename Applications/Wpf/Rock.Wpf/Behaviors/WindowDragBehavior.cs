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
using System.Windows;

namespace Rock.Wpf
{
    public static class WindowDragBehavior
    {
        public static Window GetLeftMouseButtonDrag( DependencyObject obj )
        {
            return ( Window ) obj.GetValue( LeftMouseButtonDrag );
        }

        public static void SetLeftMouseButtonDrag( DependencyObject obj, Window window )
        {
            obj.SetValue( LeftMouseButtonDrag, window );
        }

        public static readonly DependencyProperty LeftMouseButtonDrag = DependencyProperty.RegisterAttached( "LeftMouseButtonDrag",
            typeof( Window ), typeof( WindowDragBehavior ),
            new UIPropertyMetadata( null, OnLeftMouseButtonDragChanged ) );

        private static void OnLeftMouseButtonDragChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            var element = sender as UIElement;

            if ( element != null )
            {
                element.MouseLeftButtonDown += buttonDown;

            }
        }

        private static void buttonDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            var element = sender as UIElement;

            var targetWindow = element.GetValue( LeftMouseButtonDrag ) as Window;

            if ( targetWindow != null )
            {
                targetWindow.DragMove();
            }
        }
    }
}
