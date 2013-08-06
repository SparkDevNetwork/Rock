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
    [ToolboxData( "<{0}:NumberRangeEditor runat=server></{0}:NumberRangeEditor>" )]
    public class NumberRangeEditor : CompositeControl, ILabeledControl, IRequiredControl
    {
        /// <summary>
        /// The label
        /// </summary>
        private Literal label;

        /// <summary>
        /// The lower value edit box
        /// </summary>
        private NumberBox tbLowerValue;

        /// <summary>
        /// The upper value edit box
        /// </summary>
        private NumberBox tbUpperValue;

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
        Description( "The text for the label." )
        ]
        public string LabelText
        {
            get
            {
                EnsureChildControls();
                return label.Text;
            }

            set
            {
                EnsureChildControls();
                label.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRequiredControl" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                if ( ViewState["Required"] != null )
                {
                    return (bool)ViewState["Required"];
                }
                else
                {
                    return false;
                }
            }

            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public string RequiredErrorMessage
        {
            get
            {
                EnsureChildControls();
                return tbLowerValue.RequiredErrorMessage;
            }

            set
            {
                tbLowerValue.RequiredErrorMessage = value;
                tbUpperValue.RequiredErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                EnsureChildControls();
                return tbLowerValue.IsValid && tbUpperValue.IsValid;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            label = new Literal();
            Controls.Add( label );

            tbLowerValue = new NumberBox();
            tbLowerValue.ID = this.ID + "_lower";
            tbLowerValue.CssClass = "input-mini";

            Controls.Add( tbLowerValue );

            tbUpperValue = new NumberBox();
            tbUpperValue.ID = this.ID + "_upper";
            tbUpperValue.CssClass = "input-mini";
            Controls.Add( tbUpperValue );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                bool renderControlGroupDiv = !string.IsNullOrWhiteSpace( LabelText );

                if ( renderControlGroupDiv )
                {
                    writer.AddAttribute( "class", "control-group" + ( IsValid ? "" : " error" ) + ( Required ? " required" : "" ) );

                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( "class", "control-label" );

                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    label.RenderControl( writer );
                    writer.RenderEndTag();
                }

                writer.AddAttribute( "class", "controls" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                tbLowerValue.RenderControl( writer );
                writer.Write( "<span> to </span>" );
                tbUpperValue.RenderControl( writer );

                writer.RenderEndTag();

                if ( renderControlGroupDiv )
                {
                    writer.RenderEndTag();
                }
            }
        }
    }
}
