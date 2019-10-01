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
