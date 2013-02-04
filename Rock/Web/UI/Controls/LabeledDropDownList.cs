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
    /// A <see cref="T:System.Web.UI.WebControls.DropDownList"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:LabeledDropDownList runat=server></{0}:LabeledDropDownList>" )]
    public class LabeledDropDownList : DropDownList, ILabeledControl
    {
        /// <summary>
        /// 
        /// </summary>
        protected Label label;
        
        /// <summary>
        /// 
        /// </summary>
        protected RequiredFieldValidator validator;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LabeledTextBox"/> is required.
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
                    return ( bool )ViewState["Required"];
                else
                    return false;
            }
            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the help tip.
        /// </summary>
        /// <value>
        /// The help tip.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help tip." )
        ]
        public string Tip
        {
            get
            {
                string s = ViewState["Tip"] as string;
                return s == null ? string.Empty : s;
            }
            set
            {
                ViewState["Tip"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                string s = ViewState["Help"] as string;
                return s == null ? string.Empty : s;
            }
            set
            {
                ViewState["Help"] = value;
            }
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
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            label = new Label();
            label.AssociatedControlID = this.ID;

            validator = new RequiredFieldValidator();
            validator.ID = this.ID + "_rfv";
            validator.ControlToValidate = this.ID;
            validator.Display = ValidatorDisplay.Dynamic;
            validator.CssClass = "help-inline";
            validator.Enabled = false;

            Controls.Add( label );
            Controls.Add( validator );
        }

        /// <summary>
        /// Renders a label and <see cref="T:System.Web.UI.WebControls.DropDownList"/> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter"/> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter"/> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            bool isValid = !Required || validator.IsValid;

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

            base.Render( writer );

            if ( Required )
            {
                validator.Enabled = true;
                validator.ErrorMessage = LabelText + " is Required.";
                validator.RenderControl( writer );
            }

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

        /// <summary>
        /// Method for inheriting classes to use to render just the base control
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected void RenderBase( HtmlTextWriter writer )
        {
            base.Render( writer );
        }

        /// <summary>
        /// Creates a collection to store child controls.
        /// </summary>
        /// <returns>
        /// Always returns an <see cref="T:System.Web.UI.EmptyControlCollection"/>.
        /// </returns>
        protected override ControlCollection CreateControlCollection()
        {
            // By default a DropDownList control does not allow adding of child controls.
            // This method needs to be overridden to allow this
            return new ControlCollection( this );
        }

        /// <summary>
        /// Returns the Value as Int or null if Value is <see cref="T:Rock.Constants.None"/>
        /// </summary>
        /// <param name="NoneAsNull">if set to <c>true</c>, will return Null if SelectedValue = <see cref="T:Rock.Constants.None" /> </param>
        /// <returns></returns>
        public int? SelectedValueAsInt(bool NoneAsNull = true)
        {
            if ( NoneAsNull )
            {
                if ( this.SelectedValue.Equals( Rock.Constants.None.Id.ToString() ) )
                {
                    return null;
                }
            }

            if ( string.IsNullOrWhiteSpace( this.SelectedValue ) )
            {
                return null;
            }
            else
            {
                return int.Parse( this.SelectedValue );
            }
        }

        /// <summary>
        /// Selecteds the value as enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T SelectedValueAsEnum<T>()
        {
            return (T)System.Enum.Parse( typeof(T), this.SelectedValue );
        }
    }
}