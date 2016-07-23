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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A highlighted label
    /// </summary>
    [ToolboxData( "<{0}:HighlightLabel runat=server></{0}:HighlightLabel>" )]
    public class HighlightLabel : CompositeControl
    {
        /// <summary>
        /// Gets or sets the custom label suffix to use when generating the css class name.
        /// The value specified here will be be added to 'label-' when the lable type is Custom.
        /// </summary>
        /// <value>
        /// The text CSS class.
        /// </value>
        public string CustomClass
        {
            get { return ViewState["CustomClass"] as string ?? string.Empty; }
            set { ViewState["CustomClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the icon class name to use on the label.
        /// If set, a standard <i class="blah"></i> will be placed in front of the label text.
        /// </summary>
        /// <value>
        /// The icon CSS class name (such as icon-flag, icon-ok, etc.)
        /// </value>
        public string IconCssClass
        {
            get { return ViewState["IconCssClass"] as string ?? string.Empty; }
            set { ViewState["IconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text." )
        ]
        public string Text
        {
            get { return ViewState["Text"] as string ?? string.Empty; }
            set { ViewState["Text"] = value; }
        }

        /// <summary>
        /// Gets or sets the type of the label.
        /// </summary>
        /// <value>
        /// The type of the label.
        /// </value>
        public LabelType LabelType
        {
            get
            {
                string labelType = ViewState["LabelType"] as string;
                if ( labelType != null )
                {
                    return labelType.ConvertToEnum<LabelType>();
                }
                return LabelType.Default;
            }

            set { ViewState["LabelType"] = value.ConvertToInt().ToString(); }
        }

        /// <summary>
        /// Raises the <see cref="E:PreRender"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnPreRender( System.EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( this.ToolTip ) )
            {
                string script = "$('[data-toggle=\"tooltip\"]').tooltip({html: true});";
                ScriptManager.RegisterStartupScript( this, this.GetType(), "highlightlabel-tooltip", script, true );
            }
            base.OnPreRender( e );
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                string cssClass = LabelType != LabelType.Custom ? LabelType.ConvertToString().ToLower() : CustomClass;
                if ( !string.IsNullOrEmpty( this.CssClass ) )
                {
                    cssClass += " " + this.CssClass;
                }

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "label label-" + cssClass );
                if ( !string.IsNullOrEmpty( this.Style[HtmlTextWriterStyle.Display] ) )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, this.Style[HtmlTextWriterStyle.Display] );
                }

                if ( !string.IsNullOrWhiteSpace( this.ToolTip ) )
                {
                    writer.AddAttribute( "title", this.ToolTip );
                    writer.AddAttribute( "data-toggle", "tooltip" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Span );

                if ( !string.IsNullOrWhiteSpace( this.IconCssClass ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, this.IconCssClass );
                    writer.RenderBeginTag( HtmlTextWriterTag.I );
                    writer.RenderEndTag();
                    // add the obligatory space after the <i></i> tag.
                    writer.Write( " " );
                }
                writer.Write( Text );
                writer.RenderEndTag();
            }
        }

    }

    /// <summary>
    /// The type of highlighting to use.  Each type is associated with a particular css class to control the look (color)
    /// </summary>
    public enum LabelType
    {
        /// <summary>
        /// Default
        /// </summary>
        Default = 0,

        /// <summary>
        /// Primary
        /// </summary>
        Primary = 1,

        /// <summary>
        /// Success
        /// </summary>
        Success = 2,

        /// <summary>
        /// Info
        /// </summary>
        Info = 3,

        /// <summary>
        /// Warning
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Danger
        /// </summary>
        Danger = 5,

        /// <summary>
        /// Campus
        /// </summary>
        Campus = 6,

        /// <summary>
        /// Entity Type
        /// </summary>
        Type = 7,

        /// <summary>
        /// Custom
        /// </summary>
        Custom = 8,
    }

}