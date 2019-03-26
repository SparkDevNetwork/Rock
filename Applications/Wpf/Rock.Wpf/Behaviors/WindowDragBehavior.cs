
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
