//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonPicker : CompositeControl, ILabeledControl
    {
        private Label _label;
        private HiddenField _hfPersonId;
        private HiddenField _hfPersonName;
        private LinkButton _btnSelect;
        private LinkButton _btnSelectNone;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPicker" /> class.
        /// </summary>
        public PersonPicker()
        {
            _label = new Label();
            _btnSelect = new LinkButton();
            _btnSelectNone = new LinkButton();
        }


        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string LabelText
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        /// <summary>
        /// Gets or sets the name of the field to display in validation messages
        /// when a LabelText is not entered
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName
        {
            get { return _label.Text; }
            set { _label.Text = value; }
        }

        /// <summary>
        /// The required validator
        /// </summary>
        protected HiddenFieldValidator RequiredValidator;

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public string PersonId
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfPersonId.Value ) )
                {
                    _hfPersonId.Value = Rock.Constants.None.IdValue;
                }

                return _hfPersonId.Value;
            }

            set
            {
                EnsureChildControls();
                _hfPersonId.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected value.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public string SelectedValue
        {
            get
            {
                return PersonId;
            }

            set
            {
                PersonId = value;
            }
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="person">The person.</param>
        public void SetValue( Rock.Model.Person person )
        {
            if ( person != null )
            {
                PersonId = person.Id.ToString();
                PersonName = person.FullName;
            }
            else
            {
                PersonId = Rock.Constants.None.IdValue;
                PersonName = Rock.Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfPersonName.Value ) )
                {
                    _hfPersonName.Value = Rock.Constants.None.TextHtml;
                }

                return _hfPersonName.Value;
            }

            set
            {
                EnsureChildControls();
                _hfPersonName.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="PersonPicker"/> is required.
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
                    return (bool)ViewState["Required"];
                else
                    return false;
            }
            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the select person.
        /// </summary>
        /// <value>
        /// The select person.
        /// </value>
        public event EventHandler SelectPerson;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            RegisterJavaScript();
            var sm = ScriptManager.GetCurrent( this.Page );

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSelect );
                sm.RegisterAsyncPostBackControl( _btnSelectNone );
            }
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            string restUrl = this.ResolveUrl( "~/api/People/Search/" );
            const string scriptFormat = "Rock.controls.personPicker.initialize({{ controlId: '{0}', restUrl: '{1}' }});";
            string script = string.Format( scriptFormat, this.ID, restUrl );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "person_picker-" + this.ID, script, true );
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredValidator.IsValid;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hfPersonId = new HiddenField();
            _hfPersonId.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _hfPersonId.ID = string.Format( "hfPersonId_{0}", this.ID );
            _hfPersonName = new HiddenField();
            _hfPersonName.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _hfPersonName.ID = string.Format( "hfPersonName_{0}", this.ID );

            _btnSelect.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            _btnSelect.CssClass = "btn btn-mini btn-primary";
            _btnSelect.ID = string.Format( "btnSelect_{0}", this.ID );
            _btnSelect.Text = "Select";
            _btnSelect.CausesValidation = false;
            _btnSelect.Click += btnSelect_Click;

            _btnSelectNone.ClientIDMode = ClientIDMode.Static;
            _btnSelectNone.CssClass = "rock-picker-select-none";
            _btnSelectNone.ID = string.Format( "btnSelectNone_{0}", this.ID );
            _btnSelectNone.Text = "<i class='icon-remove'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.Style[HtmlTextWriterStyle.Display] = "none";
            _btnSelectNone.Click += btnSelect_Click;

            Controls.Add( _label );
            Controls.Add( _hfPersonId );
            Controls.Add( _hfPersonName );
            Controls.Add( _btnSelect );
            Controls.Add( _btnSelectNone );

            RequiredValidator = new HiddenFieldValidator();
            RequiredValidator.ID = this.ID + "_rfv";
            RequiredValidator.InitialValue = "0";
            RequiredValidator.ControlToValidate = _hfPersonId.ID;
            RequiredValidator.Display = ValidatorDisplay.Dynamic;
            RequiredValidator.CssClass = "validation-error help-inline";
            RequiredValidator.Enabled = false;

            Controls.Add( RequiredValidator );
        }

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            if ( SelectPerson != null )
            {
                SelectPerson( sender, e );
            }
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            bool renderLabel = !string.IsNullOrEmpty( LabelText );

            if ( renderLabel )
            {
                writer.AddAttribute( "class", "control-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _label.AddCssClass( "control-label" );

                _label.RenderControl( writer );

                writer.AddAttribute( "class", "controls" );

                writer.RenderBeginTag( HtmlTextWriterTag.Div );
            }

            if ( Required )
            {
                RequiredValidator.Enabled = true;
                RequiredValidator.ErrorMessage = LabelText + " is Required.";
                RequiredValidator.RenderControl( writer );
            }

            _hfPersonId.RenderControl( writer );
            _hfPersonName.RenderControl( writer );

            if ( this.Enabled )
            {
                string controlHtmlFormatStart = @"
    <span id='{0}'>
        <span class='rock-picker rock-picker-select' id='{0}'> 
            <a class='rock-picker' href='#'>
                <i class='icon-user'></i>
                <span id='selectedPersonLabel_{0}'>{1}</span>
                <b class='caret'></b>
            </a>
";

                writer.Write( string.Format( controlHtmlFormatStart, this.ID, this.PersonName ) );

                // if there is a PostBack registered, create a real LinkButton, otherwise just spit out HTML (to prevent the autopostback)
                if ( SelectPerson != null )
                {
                    _btnSelectNone.RenderControl( writer );
                }
                else
                {
                    writer.Write( "<a class='rock-picker-select-none' id='btnSelectNone_{0}' href='#' style='display:none'><i class='icon-remove'></i></a>", this.ID );
                }

                string controlHtmlFormatMiddle = @"
        </span>
        <div class='dropdown-menu rock-picker rock-picker-person'>

            <h4>Search</h4>
            <input id='personPicker_{0}' type='text' class='rock-picker-search' />
            <h4>Results</h4>
            <hr />
            <ul class='rock-picker-select' id='personPickerItems_{0}'>
            </ul>
            <hr />
";

                writer.Write( controlHtmlFormatMiddle, this.ID, this.PersonName );

                // if there is a PostBack registered, create a real LinkButton, otherwise just spit out HTML (to prevent the autopostback)
                if ( SelectPerson != null )
                {
                    _btnSelect.RenderControl( writer );
                }
                else
                {
                    writer.Write( string.Format( "<a class='btn btn-mini btn-primary' id='btnSelect_{0}'>Select</a>", this.ID ) );
                }

                string controlHtmlFormatEnd = @"
            <a class='btn btn-mini' id='btnCancel_{0}'>Cancel</a>
        </div>
    </span>
";

                writer.Write( string.Format( controlHtmlFormatEnd, this.ID, this.PersonName ) );
            }
            else
            {
                string controlHtmlFormatDisabled = @"
        <i class='icon-file-alt'></i>
        <span id='selectedItemLabel_{0}'>{1}</span>
";
                writer.Write( controlHtmlFormatDisabled, this.ID, this.PersonName );
            }

            if ( renderLabel )
            {
                writer.RenderEndTag();

                writer.RenderEndTag();
            }
        }
    }
}