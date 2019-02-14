using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rock.Apps.CheckScannerUtility.Behaviors
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
