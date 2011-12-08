//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.ComponentModel;
using System.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A composite control that renders a label, textbox, and datavalidation control for a specific field of a data model
    /// </summary>
    [ToolboxData( "<{0}:DataTextBox runat=server></{0}:DataTextBox>" )]
    public class DataTextBox : LabeledTextBox
    {
        private Validation.DataAnnotationValidator validator;

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
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            base.Render( writer );
            validator.RenderControl( writer );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            validator = new Validation.DataAnnotationValidator();
            validator.ID = "dav";
            validator.ControlToValidate = this.ID;
            validator.ForeColor = System.Drawing.Color.Red;

            Controls.Add( validator );

            base.CreateChildControls();
        }
    }
}