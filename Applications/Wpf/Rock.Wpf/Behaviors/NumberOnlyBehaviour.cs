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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rock.Wpf.Behaviors
{
    /// <summary>
    ///     Exposes attached behaviors that can be
    ///     applied to Control objects.
    /// </summary>
    public static class NumberOnlyBehaviour
    {
       
        public static readonly DependencyProperty IsEnabledProperty =
                DependencyProperty.RegisterAttached( "IsEnabled", typeof( bool ),
                typeof( NumberOnlyBehaviour ), new UIPropertyMetadata( false, OnValueChanged ) );
        public static readonly DependencyProperty AllowDecimalsProperty =
                DependencyProperty.RegisterAttached("AllowDecimals",typeof(bool),typeof( NumberOnlyBehaviour ));
 

        public static bool GetIsEnabled( Control o ) { return ( bool ) o.GetValue( IsEnabledProperty ); }

        public static void SetIsEnabled( Control o, bool value ) { o.SetValue( IsEnabledProperty, value ); }


        public static bool GetAllowDecimals( Control o ) { return ( bool ) o.GetValue( AllowDecimalsProperty ); }

        public static void SetAllowDecimals( Control o, bool value ) { o.SetValue( AllowDecimalsProperty, value ); }

        private static void OnValueChanged( DependencyObject dependencyObject,
                DependencyPropertyChangedEventArgs e )
        {
            var uiElement = dependencyObject as Control;
            if ( uiElement == null )
                return;
            if ( e.NewValue is bool && ( bool ) e.NewValue )
            {
                uiElement.PreviewTextInput += OnTextInput;
                uiElement.PreviewKeyDown += OnPreviewKeyDown;
                DataObject.AddPastingHandler( uiElement, OnPaste );
            }

            else
            {
                uiElement.PreviewTextInput -= OnTextInput;
                uiElement.PreviewKeyDown -= OnPreviewKeyDown;
                DataObject.RemovePastingHandler( uiElement, OnPaste );
            }
        }

        private static void OnTextInput( object sender, TextCompositionEventArgs e )
        {
            var uiElement = sender as Control;
            var allowDecimal = GetAllowDecimals( uiElement );
        
            if (!allowDecimal || e.Text != "." )
            {
                if ( e.Text.Any( c => !char.IsDigit( c ) ) )
                { e.Handled = true; }
            }
        }

        private static void OnPreviewKeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Space )
                e.Handled = true;
        }

        private static void OnPaste( object sender, DataObjectPastingEventArgs e )
        {
            if ( e.DataObject.GetDataPresent( DataFormats.Text ) )
            {
                var text = Convert.ToString( e.DataObject.GetData( DataFormats.Text ) ).Trim();
                if ( text.Any( c => !char.IsDigit( c ) ) )
                { e.CancelCommand(); }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
