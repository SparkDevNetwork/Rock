// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public class PersonPicker : CompositeControl, IRockControl
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
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private HiddenFieldWithClass _hfPersonId;
        private HiddenFieldWithClass _hfPersonName;
        private HiddenFieldWithClass _hfIncludeBusinesses;
        private HtmlAnchor _btnSelect;
        private HtmlAnchor _btnSelectNone;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [include businesses].
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
        /// Gets the person's primary alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        public int? PersonAliasId
        {
            get
            {
                if (PersonId.HasValue)
                {
                    return new PersonAliasService( new RockContext() ).GetPrimaryAliasId ( PersonId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets or sets the selected value.
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
        /// Registers the java script.
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

            _hfPersonId = new HiddenFieldWithClass();
            _hfPersonId.CssClass = "js-person-id";
            Controls.Add( _hfPersonId );
            _hfPersonId.ID = "hfPersonId";
            _hfPersonId.Value = "0";

            _hfPersonName = new HiddenFieldWithClass();
            _hfPersonName.CssClass = "js-person-name";
            Controls.Add( _hfPersonName );
            _hfPersonName.ID = "hfPersonName";

            _hfIncludeBusinesses = new HiddenFieldWithClass();
            _hfIncludeBusinesses.CssClass = "js-include-businesses";
            Controls.Add( _hfIncludeBusinesses );
            _hfIncludeBusinesses.ID = "hfIncludeBusinesses";

            _btnSelect = new HtmlAnchor();
            Controls.Add( _btnSelect );
            _btnSelect.Attributes["class"] = "btn btn-xs btn-primary";
            _btnSelect.ID = "btnSelect";
            _btnSelect.InnerText = "Select";
            _btnSelect.CausesValidation = false;
            _btnSelect.ServerClick += btnSelect_Click;

            _btnSelectNone = new HtmlAnchor();
            Controls.Add( _btnSelectNone );
            _btnSelectNone.Attributes["class"] = "picker-select-none";
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
            RegisterJavaScript();

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
                writer.AddAttribute( "id", this.ClientID );
                writer.AddAttribute( "class", string.Format( "picker picker-select picker-person {0}", this.CssClass ) );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _hfPersonId.RenderControl( writer );
                _hfPersonName.RenderControl( writer );
                _hfIncludeBusinesses.RenderControl( writer );
                
                string pickerLabelHtmlFormat = @"
            <a class='picker-label' href='#'>
                <i class='fa fa-user'></i>
                <span id='{0}_selectedPersonLabel' class='picker-selectedperson'>{1}</span>
                <b class='fa fa-caret-down pull-right'></b>
            </a>
";
                writer.Write( string.Format( pickerLabelHtmlFormat, this.ClientID, this.PersonName ) );

                _btnSelectNone.RenderControl( writer );

                string pickMenuHtmlFormatStart = @"
          <div class='picker-menu dropdown-menu'>

             <h4>Search</h4>
             <input id='{0}_personPicker' type='text' class='picker-search form-control input-sm' />

             <hr />             

             <h4>Results</h4>
             
             <ul class='picker-select' id='{0}_personPickerItems'>
             </ul>
             <div class='picker-actions'>
";

                writer.Write( pickMenuHtmlFormatStart, this.ClientID, this.PersonName );

                _btnSelect.RenderControl( writer );

                string pickMenuHtmlFormatEnd = @"
            <a class='btn btn-link btn-xs' id='{0}_btnCancel'>Cancel</a>
            </div>
         </div>
";

                writer.Write( string.Format( pickMenuHtmlFormatEnd, this.ClientID, this.PersonName ) );
                writer.RenderEndTag();
            }
            else
            {
                // this picker is not enabled (readonly), so just render a readonly version
                writer.AddAttribute( "class", "picker picker-select" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                LinkButton linkButton = new LinkButton();
                linkButton.CssClass = "picker-label";
                linkButton.Text = string.Format( "<i class='{1}'></i><span>{0}</span>", this.PersonName, "fa fa-user" );
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
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnSelect_Click( object sender, EventArgs e )
        {
            if ( SelectPerson != null )
            {
                SelectPerson( sender, e );
            }
        }

        /// <summary>
        /// Gets or sets the select person.
        /// </summary>
        /// <value>
        /// The select person.
        /// </value>
        public event EventHandler SelectPerson;

        #endregion
    }
}