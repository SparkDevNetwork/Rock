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
    /// A composite control that renders a label, textbox, and datavalidation control for a specific field of a data model
    /// </summary>
    [ToolboxData( "<{0}:DataTextBox runat=server></{0}:DataTextBox>" )]
    public class DataTextBox : CompositeControl
    {
        private Label label;
        private TextBox textBox;
        private Validation.DataAnnotationValidator validator;

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
        /// Gets or sets the textbox text.
        /// </summary>
        /// <value>
        /// The textbox text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the textbox." )
        ]
        public string Text
        {
            get
            {
                EnsureChildControls();
                return textBox.Text;
            }
            set
            {
                EnsureChildControls();
                textBox.Text = value;
            }
        }

        /// <summary>
        /// Gets the text box.
        /// </summary>
        public TextBox TextBox
        {
            get
            {
                EnsureChildControls();
                return textBox;
            }
        }

        /// <summary>
        /// Gets or sets the name of the assembly qualified name of the entity that is being validated
        /// </summary>
        /// <value>
        /// The name of the assembly qualified type name.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The model to validate." )
        ]
        public string SourceTypeName
        {
            get
            {
                EnsureChildControls();
                return validator.SourceTypeName;
            }
            set
            {
                EnsureChildControls();
                validator.SourceTypeName = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the entity property.
        /// </summary>
        /// <value>
        /// The name of the entity property.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The model property that is annotated." )
        ]
        public string PropertyName
        {
            get
            {
                EnsureChildControls();
                return validator.PropertyName;
            }
            set
            {
                EnsureChildControls();
                validator.PropertyName = value;
                if ( this.LabelText == string.Empty )
                    this.LabelText = value.SplitCase();
            }
        }

        /// <summary>
        /// Recreates the child controls in a control derived from <see cref="T:System.Web.UI.WebControls.CompositeControl"/>.
        /// </summary>
        protected override void RecreateChildControls()
        {
            EnsureChildControls();
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            AddAttributesToRender( writer );

            bool isValid = validator.IsValid;

            writer.AddAttribute( "class", isValid ? "" : "error" );
            writer.RenderBeginTag( HtmlTextWriterTag.Dt );
            
            label.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( "class", isValid ? "" : "error" );
            writer.RenderBeginTag( HtmlTextWriterTag.Dd );
            textBox.RenderControl( writer );
            validator.RenderControl( writer );
            writer.RenderEndTag();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            textBox = new TextBox();
            textBox.ID = "tb";

            label = new Label();
            label.AssociatedControlID = textBox.ID;

            validator = new Validation.DataAnnotationValidator();
            validator.ID = "dav";
            validator.ControlToValidate = textBox.ID;
            validator.Display = ValidatorDisplay.None;
            validator.ForeColor = System.Drawing.Color.Red;

            this.Controls.Add( label );
            this.Controls.Add( textBox );
            this.Controls.Add( validator );
        }
    }
}