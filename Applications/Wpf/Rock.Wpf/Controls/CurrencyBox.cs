using System.Windows;
using System.Windows.Controls;

namespace Rock.Wpf.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.TextBox" />
    public class CurrencyBox : TextBox
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyBox"/> class.
        /// </summary>
        public CurrencyBox()
        {
            Behaviors.NumberOnlyBehaviour.SetAllowDecimals( this, true );
            Behaviors.NumberOnlyBehaviour.SetIsEnabled( this, true );
            Style = Application.Current.Resources["textboxStyleCurrency"] as Style;
            HorizontalContentAlignment = HorizontalAlignment.Right;
        }

        private bool isValid = true;

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get => isValid;
            set
            {
                isValid = value;
                if ( value )
                {
                    Style = Application.Current.Resources["textboxStyleCurrency"] as Style;
                }
                else
                {
                    Style = Application.Current.Resources["textboxStyleCurrencyError"] as Style;
                }
            }
        }
    }
}
