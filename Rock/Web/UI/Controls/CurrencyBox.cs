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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.CurrencyBox"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:CurrencyBox runat=server></{0}:CurrencyBox>" )]
    public class CurrencyBox : NumberBoxBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyBox" /> class.
        /// </summary>
        public CurrencyBox()
            : base()
        {
            this.NumberType = ValidationDataType.Currency;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            base.OnInit( e );

            var globalAttributes = GlobalAttributesCache.Get();
            if ( globalAttributes != null )
            {
                string symbol = globalAttributes.GetValue( "CurrencySymbol" );
                this.PrependText = string.IsNullOrWhiteSpace( symbol ) ? "$" : symbol;
            }
        }

        /// <summary>
        /// Renders the base control and allows a dec to show on mobile keyboards
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            this.Attributes["step"] = "0.01";
            base.RenderBaseControl( writer );
        }

        /// <summary>
        /// Gets or sets the currency value
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal? Value
        {
            get
            {
                return this.Text.AsDecimalOrNull().FormatAsCurrency().AsDecimalOrNull();
            }

            set
            {
                this.Text = value?.ToString("F2");
            }
        }
    }
}