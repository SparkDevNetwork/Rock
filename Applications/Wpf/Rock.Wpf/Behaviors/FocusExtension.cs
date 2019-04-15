using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Rock.Wpf
{
    public static class FocusExtension
    {
        public static bool GetIsFocused( Control textBox )
        {
            return ( bool ) textBox.GetValue( IsFocusedProperty );
        }

        public static void SetIsFocused( Control control, bool value )
        {
            control.SetValue( IsFocusedProperty, value );
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached( "IsFocused", typeof( bool ), typeof( FocusExtension ),
                new UIPropertyMetadata( false, OnIsFocusedPropertyChanged ) );

        private static void OnIsFocusedPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            Control control = d as Control;
            if ( ( bool ) e.NewValue )
            { 
                control.Dispatcher.BeginInvoke( new Action( () =>
                {
                    Keyboard.Focus( control );
                } ), DispatcherPriority.Background );
            }
        }
    }
}
