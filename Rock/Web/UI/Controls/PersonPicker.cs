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
        private Label label;
        private Literal literal;
        private HiddenField hfPersonId;
        private HiddenField hfPersonName;
        private LinkButton btnSelect;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPicker" /> class.
        /// </summary>
        public PersonPicker()
        {
            btnSelect = new LinkButton();
        }

        /// <summary>
        /// The required validator
        /// </summary>
        protected HiddenFieldValidator requiredValidator;

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
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
                if ( string.IsNullOrWhiteSpace( hfPersonId.Value ) )
                {
                    hfPersonId.Value = Rock.Constants.None.IdValue;
                }

                return hfPersonId.Value;
            }

            set
            {
                EnsureChildControls();
                hfPersonId.Value = value;
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
                if ( string.IsNullOrWhiteSpace( hfPersonName.Value ) )
                {
                    hfPersonName.Value = Rock.Constants.None.TextHtml;
                }

                return hfPersonName.Value;
            }

            set
            {
                EnsureChildControls();
                hfPersonName.Value = value;
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

            string restUrl = this.ResolveUrl( "~/api/People/Search/" );

            string scriptFormat = @"
        $('#personPicker_{0}').autocomplete({{
            source: function (request, response) {{
                $.ajax({{
                    url: '{1}' + request.term,
                    dataType: 'json',
                    success: function (data, status, xhr) {{
                        $('#personPickerItems_{0}')[0].innerHTML = '';
                        response($.map(data, function (item) {{
                            return item;
                        }}
                        ))
                    }},
                    error: function (xhr, status, error) {{
                        alert(status + ' [' + error + ']: ' + xhr.responseText);
                    }}
                }});
            }},
            minLength: 3,
            html: true,
            appendTo: 'personPickerItems_{0}',
            messages: {{
                noResults: function () {{ }},
                results: function () {{ }}
            }}
        }});

        $('a.rock-picker').click(function (e) {{
            e.preventDefault();
            $(this).next('.rock-picker').toggle();
        }});

        $('.rock-picker-select').on('click', '.rock-picker-select-item', function (e) {{
            var selectedItem = $(this).attr('data-person-id');

            // hide other open details
            $('.rock-picker-select-item-details').each(function (index) {{
                var currentItem = $(this).parent().attr('data-person-id');

                if (currentItem != selectedItem) {{
                    $(this).slideUp();
                }}
            }});

            $(this).find('.rock-picker-select-item-details:hidden').slideDown();
        }});

        $('#btnCancel_{0}').click(function (e) {{
            $(this).parent().slideUp();
        }});

        $('#btnSelect_{0}').click(function (e) {{
            var radInput = $('#{0}').find('input:checked');

            var selectedValue = radInput.val();
            var selectedText = radInput.parent().text();

            var selectedPersonLabel = $('#selectedPersonLabel_{0}');

            var hiddenPersonId = $('#hfPersonId_{0}');
            var hiddenPersonName = $('#hfPersonName_{0}');

            hiddenPersonId.val(selectedValue);
            hiddenPersonName.val(selectedText);

            selectedPersonLabel.val(selectedValue);
            selectedPersonLabel.text(selectedText);

            $(this).parent().slideUp();
        }});
";

            string script = string.Format( scriptFormat, this.ID, restUrl );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "person_picker-" + this.ID.ToString(), script, true );

            var sm = ScriptManager.GetCurrent( this.Page );
            sm.RegisterAsyncPostBackControl( btnSelect );
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
                return !Required || requiredValidator.IsValid;
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
            literal = new Literal();
            hfPersonId = new HiddenField();
            hfPersonId.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            hfPersonId.ID = string.Format( "hfPersonId_{0}", this.ID );
            hfPersonName = new HiddenField();
            hfPersonName.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            hfPersonName.ID = string.Format( "hfPersonName_{0}", this.ID );

            btnSelect.ClientIDMode = System.Web.UI.ClientIDMode.Static;
            btnSelect.CssClass = "btn btn-mini btn-primary";
            btnSelect.ID = string.Format( "btnSelect_{0}", this.ID );
            btnSelect.Text = "Select";
            btnSelect.CausesValidation = false;
            btnSelect.Click += btnSelect_Click;

            Controls.Add( label );
            Controls.Add( literal );
            Controls.Add( hfPersonId );
            Controls.Add( hfPersonName );
            Controls.Add( btnSelect );

            requiredValidator = new HiddenFieldValidator();
            requiredValidator.ID = this.ID + "_rfv";
            requiredValidator.InitialValue = "0";
            requiredValidator.ControlToValidate = hfPersonId.ID;
            requiredValidator.Display = ValidatorDisplay.Dynamic;
            requiredValidator.CssClass = "validation-error";
            requiredValidator.Enabled = false;

            Controls.Add( requiredValidator );
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
            string controlHtmlFormatStart = @"
<div class='control-group' id='{0}'>
    <div class='controls'>
        <a class='rock-picker' href='#'>
            <i class='icon-user'></i>
            <span id='selectedPersonLabel_{0}'>{1}</span>
            <b class='caret'></b>
        </a>
        <div class='dropdown-menu rock-picker rock-picker-person'>

            <h4>Search</h4>
            <input id='personPicker_{0}' type='text' class='rock-picker-search' />
            <h4>Results</h4>
            <hr />
            <ul class='rock-picker-select' id='personPickerItems_{0}'>
            </ul>
            <hr />
";
            string controlHtmlFormatEnd = @"
            <a class='btn btn-mini' id='btnCancel_{0}'>Cancel</a>
        </div>
    </div>
</div>
";

            writer.AddAttribute( "class", "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            label.AddCssClass( "control-label" );

            label.RenderControl( writer );

            writer.AddAttribute( "class", "controls" );

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            if ( Required )
            {
                requiredValidator.Enabled = true;
                requiredValidator.ErrorMessage = LabelText + " is Required.";
                requiredValidator.RenderControl( writer );
            }

            hfPersonId.RenderControl( writer );
            hfPersonName.RenderControl( writer );

            writer.Write( string.Format( controlHtmlFormatStart, this.ID, this.PersonName ) );

            // if there is a PostBack registered, create a real LinkButton, otherwise just spit out HTML (to prevent the autopostback)
            if ( SelectPerson != null )
            {
                btnSelect.RenderControl( writer );
            }
            else
            {
                writer.Write( string.Format( "<a class='btn btn-mini btn-primary' id='btnSelect_{0}'>Select</a>", this.ID ) );
            }

            writer.Write( string.Format( controlHtmlFormatEnd, this.ID, this.PersonName ) );

            writer.RenderEndTag();

            writer.RenderEndTag();
        }
    }
}