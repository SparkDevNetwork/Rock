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
            HorizontalContentAlignment = HorizontalAlignment.Left;
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
