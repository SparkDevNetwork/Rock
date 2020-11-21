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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Field where the Text can be set either at design time, or on RowDataBound by doing a (e.Row.FindControl( "ID" ) as Literal).Text =...
    /// Make sure to set ID if you want to use the FindControl stuff
    /// </summary>
    [ToolboxData( "<{0}:RockLiteralField runat=server ></{0}:RockLiteralField>" )]
    public class RockLiteralField : RockTemplateField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockLiteralField"/> class.
        /// </summary>
        public RockLiteralField()
        {
            this.ItemTemplate = new LiteralTemplate();
        }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text
        {
            get
            {
                return ViewState["Text"] as string;
            }

            set
            {
                ViewState["Text"] = value;
            }
        }

        /// <summary>
        /// Occurs when [on data bound].
        /// </summary>
        public event EventHandler<RowEventArgs> DataBound;

        /// <summary>
        /// Handles the on data bound.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        internal void HandleOnDataBound( object sender, RowEventArgs e )
        {
            if ( this.DataBound != null )
            {
                this.DataBound( sender, e );
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.Web.UI.ITemplate" />
        public class LiteralTemplate : ITemplate
        {
            private RockLiteralField RockLiteralField { get; set; }

            /// <summary>
            /// When implemented by a class, defines the <see cref="T:System.Web.UI.Control" /> object that child controls and templates belong to. These child controls are in turn defined within an inline template.
            /// </summary>
            /// <param name="container">The <see cref="T:System.Web.UI.Control" /> object to contain the instances of controls from the inline template.</param>
            public void InstantiateIn( Control container )
            {
                var cell = container as DataControlFieldCell;
                if ( cell != null )
                {
                    var rockLiteralField = cell.ContainingField as RockLiteralField;
                    this.RockLiteralField = rockLiteralField;
                    if ( rockLiteralField != null )
                    {
                        if ( container.Controls.Count == 0 )
                        {
                            Literal lLiteral = new Literal();
                            lLiteral.ID = rockLiteralField.ID;
                            lLiteral.Text = rockLiteralField.Text;
                            lLiteral.Visible = rockLiteralField.Visible;
                            lLiteral.DataBinding += literal_DataBinding;

                            container.Controls.Add( lLiteral );
                        }
                    }
                }
            }

            /// <summary>
            /// Handles the DataBinding event of the literal control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
            private void literal_DataBinding( object sender, EventArgs e )
            {
                GridViewRow row = ( GridViewRow ) ( ( Literal ) sender ).Parent.Parent;
                RowEventArgs args = new RowEventArgs( row );
                this.RockLiteralField.HandleOnDataBound( sender, args );
            }
        }
    }
}
