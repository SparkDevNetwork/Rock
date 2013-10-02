//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        /// the value specified here will be be added to 'label-' when the lable type is Custom.
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
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                string cssClass = LabelType != LabelType.Custom ? LabelType.ConvertToString().ToLower() : CustomClass;
                if (this.CssClass != string.Empty)
                {
                    cssClass += " " + this.CssClass;
                }
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "label label-" + cssClass );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
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