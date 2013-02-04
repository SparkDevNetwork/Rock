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
    /// A composite control that renders a label, dropdownlist, and datavalidation control for a specific field of a data model
    /// </summary>
    [ToolboxData( "<{0}:DataDropDownList runat=server></{0}:DataDropDownList>" )]
    public class DataDropDownList : LabeledDropDownList
    {
        private Validation.DataAnnotationValidator dataValidator;

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
                return dataValidator.SourceTypeName;
            }
            set
            {
                EnsureChildControls();
                dataValidator.SourceTypeName = value;
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
                return dataValidator.PropertyName;
            }
            set
            {
                EnsureChildControls();
                dataValidator.PropertyName = value;
                if ( this.LabelText == string.Empty )
                    this.LabelText = value.SplitCase();
            }
        }

        /// <summary>
        /// Shows the error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        public void ShowErrorMessage( string errorMessage )
        {
            dataValidator.ErrorMessage = errorMessage;
            dataValidator.IsValid = false;
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            dataValidator = new Validation.DataAnnotationValidator();
            dataValidator.ID = this.ID + "_dav";
            dataValidator.ControlToValidate = this.ID;
            dataValidator.Display = ValidatorDisplay.Dynamic;
            dataValidator.CssClass = "help-inline";

            Controls.Add( dataValidator );
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.TextBox"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            bool isValid = ( !Required || validator.IsValid ) && dataValidator.IsValid;

            writer.AddAttribute( "class", "control-group" +
                ( isValid ? "" : " error" ) +
                ( Required ? " required" : "" ) );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( "class", "control-label" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            label.RenderControl( writer );

            if ( Help.Trim() != string.Empty )
            {
                HelpBlock helpBlock = new HelpBlock();
                helpBlock.Text = Help.Trim();
                helpBlock.RenderControl( writer );
            }

            writer.RenderEndTag();

            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            RenderBase( writer );

            if ( Required )
            {
                validator.ErrorMessage = LabelText + " is Required.";
                validator.RenderControl( writer );
            }

            dataValidator.RenderControl( writer );

            if ( Tip.Trim() != string.Empty )
            {
                writer.AddAttribute( "class", "help-tip" );
                writer.AddAttribute( "href", "#" );
                writer.RenderBeginTag( HtmlTextWriterTag.A );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( Tip.Trim() );
                writer.RenderEndTag();
                writer.RenderEndTag();
            }

            writer.RenderEndTag();

            writer.RenderEndTag();
        }
    }
}