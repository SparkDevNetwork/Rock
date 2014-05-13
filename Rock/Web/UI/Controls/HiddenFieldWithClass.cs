using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// from http://stackoverflow.com/a/8761161/1755417
    /// </summary>
    public class HiddenFieldWithClass : HiddenField
    {
        /// <summary>
        /// Gets or sets the CSS class.
        /// </summary>
        /// <value>
        /// The CSS class.
        /// </value>
        [CssClassProperty]
        [DefaultValue( "" )]
        public virtual string CssClass
        {
            get
            {
                string Value = this.ViewState["CssClass"] as string;
                if ( Value == null )
                    Value = "";
                return Value;
            }
            set
            {
                this.ViewState["CssClass"] = value;
            }
        }

        /// <summary>
        /// Renders the Web server control content to the client's browser using the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object used to render the server control content on the client's browser.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( this.CssClass != "" )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, this.CssClass );
            }
            base.Render( writer );
        }
    }
}
