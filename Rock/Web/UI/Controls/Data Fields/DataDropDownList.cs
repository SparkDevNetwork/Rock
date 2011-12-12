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
    public class DataDropDownList : CompositeControl
    {
        private Label label;
        private DropDownList dropDownList;
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
        /// Gets or sets the drop down list text.
        /// </summary>
        /// <value>
        /// The drop down list text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the drop down list." )
        ]
        public string Text
        {
            get
            {
                EnsureChildControls();
                return dropDownList.Text;
            }
            set
            {
                EnsureChildControls();
                dropDownList.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected value of the drop down list.
        /// </summary>
        /// <value>
        /// The selected value of the drop down list.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "" ),
        Description( "The selected value of the drop down list." )
        ]
        public string SelectedValue
        {
            get
            {
                EnsureChildControls();
                return dropDownList.SelectedValue;
            }
            set
            {
                EnsureChildControls();
                dropDownList.SelectedValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the items of the drop down list.
        /// </summary>
        /// <value>
        /// The items of the drop down list.
        /// </value>
        [
        Bindable( true ),
        Category( "Misc" ),
        DefaultValue( "" ),
        Description( "The items of the drop down list." )
        ]
        public ListItemCollection Items
        {
            get
            {
                EnsureChildControls();
                return dropDownList.Items;
            }
        }

        /// <summary>
        /// Gets the drop down list.
        /// </summary>
        public DropDownList DropDownList
        {
            get
            {
                EnsureChildControls();
                return dropDownList;
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
            dropDownList.RenderControl( writer );
            validator.RenderControl( writer );
            writer.RenderEndTag();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            dropDownList = new DropDownList();
            dropDownList.ID = "ddl";

            label = new Label();
            label.AssociatedControlID = dropDownList.ID;

            validator = new Validation.DataAnnotationValidator();
            validator.ID = "dav";
            validator.ControlToValidate = dropDownList.ID;
            validator.Display = ValidatorDisplay.None;
            validator.ForeColor = System.Drawing.Color.Red;

            this.Controls.Add( label );
            this.Controls.Add( dropDownList );
            this.Controls.Add( validator );
        }
    }
}