﻿// <copyright>
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
        /// Gets or sets the ID of the Literal control that will be created for this field.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public string ID
        {
            get
            {
                return ViewState["ID"] as string;
            }

            set
            {
                ViewState["ID"] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class LiteralTemplate : ITemplate
        {
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
                    if ( rockLiteralField != null )
                    {
                        if ( container.Controls.Count == 0 )
                        {
                            Literal lLiteral = new Literal();
                            lLiteral.ID = rockLiteralField.ID;
                            lLiteral.Text = rockLiteralField.Text;
                            lLiteral.Visible = rockLiteralField.Visible;

                            container.Controls.Add( lLiteral );
                        }
                    }
                }
            }
        }
    }
}
