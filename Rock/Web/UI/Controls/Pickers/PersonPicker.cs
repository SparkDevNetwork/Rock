// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    ///
    /// </summary>
    public class PersonPicker : CompositeControl, IRockControl, IRockChangeHandlerControl
    {
        #region IRockControl implementation

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
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS Icon text.
        /// </summary>
        /// <value>
        /// The CSS icon class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string IconCssClass
        {
            get { return ViewState["IconCssClass"] as string ?? string.Empty; }
            set { ViewState["IconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
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
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }
            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }
            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
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
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return RequiredFieldValidator.ValidationGroup;
            }
            set
            {
                RequiredFieldValidator.ValidationGroup = value;
            }
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
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private Panel _hiddenFieldsPanel;
        private HiddenFieldWithClass _hfPersonId;
        private HiddenFieldWithClass _hfPersonName;
        private HiddenFieldWithClass _hfSelfPersonId;
        private HiddenFieldWithClass _hfSelfPersonName;
        private HiddenFieldWithClass _hfIncludeBusinesses;
        private HiddenFieldWithClass _hfIncludeDeceased;
        private HiddenFieldWithClass _hfExpandSearchFields;

        private Panel _searchPanel;
        private RockTextBox _tbSearchName;
        private RockTextBox _tbSearchAddress;
        private RockTextBox _tbSearchPhone;
        private RockTextBox _tbSearchEmail;

        private HtmlAnchor _btnSelect;
        private HtmlAnchor _btnSelectNone;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to include businesses (default false).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include businesses]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeBusinesses
        {
            get
            {
                EnsureChildControls();
                return _hfIncludeBusinesses.Value.AsBooleanOrNull() ?? false;
            }

            set
            {
                EnsureChildControls();
                _hfIncludeBusinesses.Value = value.Bit().ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether deceased people should be included in search results (default true).
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include deceased]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeDeceased
        {
            get
            {
                EnsureChildControls();
                return _hfIncludeDeceased.Value.AsBooleanOrNull() ?? true;
            }

            set
            {
                EnsureChildControls();
                _hfIncludeDeceased.Value = value.Bit().ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the additional search options should be expanded by default
        /// </summary>
        /// <value>
        ///   <c>true</c> if [expand search options]; otherwise, <c>false</c>.
        /// </value>
        public bool ExpandSearchOptions
        {
            get
            {
                EnsureChildControls();
                return _hfExpandSearchFields.Value.AsBooleanOrNull() ?? false;
            }

            set
            {
                EnsureChildControls();
                _hfExpandSearchFields.Value = value.Bit().ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable self selection].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable self selection]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSelfSelection
        {
            get => ViewState["EnableSelfSelection"] as bool? ?? false;
            set => ViewState["EnableSelfSelection"] = value;
        }

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int? PersonId
        {
            get
            {
                EnsureChildControls();
                if ( string.IsNullOrWhiteSpace( _hfPersonId.Value ) )
                {
                    _hfPersonId.Value = Rock.Constants.None.IdValue;
                }

                if ( _hfPersonId.Value.Equals( Rock.Constants.None.IdValue ) )
                {
                    return null;
                }
                else
                {
                    return _hfPersonId.Value.AsIntegerOrNull();
                }
            }

            set
            {
                EnsureChildControls();
                _hfPersonId.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets the selected person's primary alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId
        {
            get
            {
                if ( PersonId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        return new PersonAliasService( rockContext ).GetPrimaryAliasId( PersonId.Value );
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the Id of the selected person. See also <seealso cref="PersonId"/>.
        /// </summary>
        /// <value>
        /// The selected value.
        /// </value>
        public int? SelectedValue
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
                PersonId = person.Id;
                PersonName = person.FullName;
            }
            else
            {
                PersonId = Rock.Constants.None.Id;
                PersonName = Rock.Constants.None.TextHtml;
                _tbSearchName.Text = string.Empty;
                _tbSearchPhone.Text = string.Empty;
                _tbSearchAddress.Text = string.Empty;
                _tbSearchEmail.Text = string.Empty;
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonPicker" /> class.
        /// </summary>
        public PersonPicker()
        {
            // note we are using HiddenFieldValidator instead of RequiredFieldValidator
            RequiredFieldValidator = new HiddenFieldValidator();

            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion


        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var sm = ScriptManager.GetCurrent( this.Page );
            EnsureChildControls();

            if ( sm != null )
            {
                sm.RegisterAsyncPostBackControl( _btnSelect );
                sm.RegisterAsyncPostBackControl( _btnSelectNone );
            }
        }

        /// <summary>
        /// Registers the JavaScript.
        /// </summary>
        protected virtual void RegisterJavaScript()
        {
            string restUrl = this.ResolveUrl( "~/api/People/Search/" );
            const string scriptFormat = "Rock.controls.personPicker.initialize({{ controlId: '{0}', restUrl: '{1}' }});";
            string script = string.Format( scriptFormat, this.ClientID, restUrl );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "person_picker-" + this.ClientID, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            #region hidden fields

            _hiddenFieldsPanel = new Panel();
            _hiddenFieldsPanel.ID = "hiddenFieldsPanel";
            Controls.Add( _hiddenFieldsPanel );

            _hfPersonId = new HiddenFieldWithClass();
            _hfPersonId.CssClass = "js-person-id";
            _hiddenFieldsPanel.Controls.Add( _hfPersonId );
            _hfPersonId.ID = "hfPersonId";
            _hfPersonId.Value = "0";

            _hfPersonName = new HiddenFieldWithClass();
            _hfPersonName.CssClass = "js-person-name";
            _hiddenFieldsPanel.Controls.Add( _hfPersonName );
            _hfPersonName.ID = "hfPersonName";

            _hfSelfPersonId = new HiddenFieldWithClass();
            _hfSelfPersonId.CssClass = "js-self-person-id";
            _hiddenFieldsPanel.Controls.Add( _hfSelfPersonId );
            _hfSelfPersonId.ID = "hfSelfPersonId";
            _hfSelfPersonId.Value = "0";

            _hfSelfPersonName = new HiddenFieldWithClass();
            _hfSelfPersonName.CssClass = "js-self-person-name";
            _hiddenFieldsPanel.Controls.Add( _hfSelfPersonName );
            _hfSelfPersonName.ID = "hfSelfPersonName";

            _hfIncludeBusinesses = new HiddenFieldWithClass();
            _hfIncludeBusinesses.CssClass = "js-include-businesses";
            _hiddenFieldsPanel.Controls.Add( _hfIncludeBusinesses );
            _hfIncludeBusinesses.ID = "hfIncludeBusinesses";

            _hfIncludeDeceased = new HiddenFieldWithClass();
            _hfIncludeDeceased.CssClass = "js-include-deceased";
            _hiddenFieldsPanel.Controls.Add( _hfIncludeBusinesses );
            _hfIncludeDeceased.ID = "hfIncludeDeceased";

            _hfExpandSearchFields = new HiddenFieldWithClass();
            _hfExpandSearchFields.CssClass = "js-expand-search-fields";
            _hiddenFieldsPanel.Controls.Add( _hfExpandSearchFields );
            _hfExpandSearchFields.ID = "hfExpandSearchFields";

            #endregion hidden fields

            #region search  fields

            _searchPanel = new Panel();
            _searchPanel.ID = "searchPanel";
            _searchPanel.CssClass = "js-personpicker-search-panel personpicker-search-panel";
            this.Controls.Add( _searchPanel );

            _tbSearchName = new RockTextBox();
            _tbSearchName.ID = "tbSearchName";
            _tbSearchName.PrependText = "Name";
            _tbSearchName.CssClass = "input-group-sm js-personpicker-search-name js-personpicker-search-field personpicker-search-field";
            _tbSearchName.Attributes["autocapitalize"] = "off";
            _tbSearchName.Attributes["autocomplete"] = "off";
            _tbSearchName.Attributes["autocorrect"] = "off";
            _tbSearchName.Attributes["spellcheck"] = "false";
            _searchPanel.Controls.Add( _tbSearchName );

            var additionalSearchFieldsPanel = new Panel();
            additionalSearchFieldsPanel.CssClass = "js-personpicker-additional-search-fields personpicker-additional-search-fields";
            _searchPanel.Controls.Add( additionalSearchFieldsPanel );

            _tbSearchAddress = new RockTextBox();
            _tbSearchAddress.ID = "tbSearchAddress";
            _tbSearchAddress.PrependText = "Address";
            _tbSearchAddress.CssClass = "input-group-sm js-personpicker-search-address js-personpicker-search-field personpicker-search-field";
            _tbSearchAddress.Attributes["spellcheck"] = "false";
            additionalSearchFieldsPanel.Controls.Add( _tbSearchAddress );

            _tbSearchPhone = new RockTextBox();
            _tbSearchPhone.ID = "tbSearchPhone";
            _tbSearchPhone.PrependText = "Phone";
            _tbSearchPhone.CssClass = "input-group-sm js-personpicker-search-phone js-personpicker-search-field personpicker-search-field";
            _tbSearchPhone.Attributes["type"] = "tel";
            additionalSearchFieldsPanel.Controls.Add( _tbSearchPhone );

            _tbSearchEmail = new RockTextBox();
            _tbSearchEmail.ID = "tbSearchEmail";
            _tbSearchEmail.PrependText = "Email";
            _tbSearchEmail.CssClass = "input-group-sm js-personpicker-search-email js-personpicker-search-field personpicker-search-field";
            _tbSearchEmail.Attributes["type"] = "email";
            additionalSearchFieldsPanel.Controls.Add( _tbSearchEmail );

            #endregion search fields

            _btnSelect = new HtmlAnchor();
            Controls.Add( _btnSelect );
            _btnSelect.Attributes["class"] = "btn btn-xs btn-primary js-personpicker-select";
            _btnSelect.ID = "btnSelect";
            _btnSelect.InnerText = "Select";
            _btnSelect.CausesValidation = false;
            _btnSelect.ServerClick += btnSelect_Click;

            _btnSelectNone = new HtmlAnchor();
            Controls.Add( _btnSelectNone );
            _btnSelectNone.Attributes["class"] = "picker-select-none js-picker-select-none";
            _btnSelectNone.ID = "btnSelectNone";
            _btnSelectNone.InnerHtml = "<i class='fa fa-times'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.Style[HtmlTextWriterStyle.Display] = "none";
            _btnSelectNone.ServerClick += btnSelect_Click;

            RockControlHelper.CreateChildControls( this, Controls );

            // override a couple of property values on RequiredFieldValidator so that Validation works correctly
            RequiredFieldValidator.InitialValue = "0";
            RequiredFieldValidator.ControlToValidate = _hfPersonId.ID;
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( this.Enabled )
            {
                var rockBlock = this.RockBlock();
                _hfSelfPersonId.Value = rockBlock?.CurrentPersonId.ToString();
                _hfSelfPersonName.Value = rockBlock?.CurrentPerson?.ToString();

                // make sure the hidden fields get configured values (and make sure it they set to the default if it wasn't set)
                _hfIncludeDeceased.Value = this.IncludeDeceased.Bit().ToString();
                _hfIncludeBusinesses.Value = this.IncludeBusinesses.Bit().ToString();
                _hfExpandSearchFields.Value = this.ExpandSearchOptions.Bit().ToString();

                writer.AddAttribute( "id", this.ClientID );
                writer.AddAttribute( "class", string.Format( "picker picker-select picker-person {0}", this.CssClass ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _hiddenFieldsPanel.RenderControl( writer );

                writer.Write(
                    $@"
            <a class='picker-label js-personpicker-toggle' href='#'>
                <i class='fa fa-user'></i>
                <span class='js-personpicker-selectedperson-label picker-selectedperson'>{this.PersonName}</span>
                <b class='fa fa-caret-down pull-right'></b>
            </a>
" );

                _btnSelectNone.RenderControl( writer );

                // render picker-menu dropdown-menu
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "picker-menu dropdown-menu js-personpicker-menu" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // column1
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.Write( "<h4>Search</h4>" );
                writer.RenderEndTag();

                // column2
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-xs-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // actions div
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "pull-right" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                // Render Additional Search Fields toggler
                writer.Write( @"
                    <span class='js-toggle-additional-search-fields toggle-additional-search-fields' title='Advanced Search'>
                        <i class='fa fa-search-plus clickable' ></i>
                    </span>" );

                // Render Self Picker
                if ( this.EnableSelfSelection )
                {
                    writer.Write( @"
                    <span class='js-select-self' title='Select self'>
                        <i class='fa fa-user clickable' ></i>
                    </span>" );
                }

                // end actions div
                writer.RenderEndTag();

                // end column2
                writer.RenderEndTag();


                // end row
                writer.RenderEndTag();

                _searchPanel.RenderControl( writer );

                writer.Write( @"
             <hr />

             <div class='js-personpicker-scroll-container scroll-container scroll-container-vertical scroll-container-picker'>
                <div class='scrollbar'>
                    <div class='track'>
                        <div class='thumb'>
                            <div class='end'></div>
                        </div>
                    </div>
                </div>
                <div class='viewport'>
                    <div class='overview'>
                        <ul class='picker-select js-personpicker-searchresults'>
                        </ul>
                    </div>
                </div>
            </div>

             <div class='picker-actions'>
" );

                _btnSelect.RenderControl( writer );

                writer.Write( @"
            <a class='btn btn-link btn-xs js-personpicker-cancel'>Cancel</a>
            </div>
" );

                // picker-menu dropdown-menu
                writer.RenderEndTag();

                // picker picker-select picker-person
                writer.RenderEndTag();

                RegisterJavaScript();
            }
            else
            {
                // this picker is not enabled (readonly), so just render a readonly version
                writer.AddAttribute( "class", "picker picker-select" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                LinkButton linkButton = new LinkButton();
                linkButton.CssClass = "picker-label";
                linkButton.Text = $"<i class='fa fa-user'></i><span>{this.PersonName}</span>";
                linkButton.Enabled = false;
                linkButton.RenderControl( writer );
                writer.WriteLine();
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            SelectPerson?.Invoke( sender, e );

            ValueChanged?.Invoke( sender, e );
        }

        /// <summary>
        /// Gets or sets the select person.
        /// </summary>
        /// <value>
        /// The select person.
        /// </value>
        public event EventHandler SelectPerson;

        #endregion

        #region IRockChangeHandlerControl

        /// <summary>
        /// Occurs when the selected value has changed
        /// </summary>
        public event EventHandler ValueChanged;

        #endregion IRockChangeHandlerControl
    }
}