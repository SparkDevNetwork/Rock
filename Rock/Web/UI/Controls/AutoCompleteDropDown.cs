//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control that will display a list of options as user types.
    /// </summary>
    [ToolboxData( "<{0}:AutoCompleteDropDown runat=server></{0}:AutoCompleteDropDown>" )]
    public class AutoCompleteDropDown : TextBox, IRockControl
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

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                base.ValidationGroup = value;
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ValidationGroup = value;
                }
            }
        }

        #endregion

        #region Controls

        public HiddenField _hfIdProperty;
        public HiddenField _hfNameProperty;
        public HiddenField _hfDropdownHeader;
        public HiddenField _hfDropdownFooter;
        public HiddenField _hfValue;
        public HiddenField _hfTemplate;
        public HiddenField _hfUrl;
        public HiddenField _hfLimit;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the identifier property.
        /// </summary>
        /// <value>
        /// The identifier property.
        /// </value>
        public string IdProperty
        {
            get
            {
                EnsureChildControls();
                return _hfIdProperty.Value;
            }
            set
            {
                EnsureChildControls();
                _hfIdProperty.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the name property.
        /// </summary>
        /// <value>
        /// The name property.
        /// </value>
        public string NameProperty
        {
            get
            {
                EnsureChildControls();
                return _hfNameProperty.Value;
            }
            set
            {
                EnsureChildControls();
                _hfNameProperty.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the dropdown header.
        /// </summary>
        /// <value>
        /// The dropdown header.
        /// </value>
        public string DropdownHeader
        {
            get
            {
                EnsureChildControls();
                return _hfDropdownHeader.Value;
            }
            set
            {
                EnsureChildControls();
                _hfDropdownHeader.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the dropdown footer.
        /// </summary>
        /// <value>
        /// The dropdown footer.
        /// </value>
        public string DropdownFooter
        {
            get
            {
                EnsureChildControls();
                return _hfDropdownFooter.Value;
            }
            set
            {
                EnsureChildControls();
                _hfDropdownFooter.Value = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get
            {
                EnsureChildControls();
                return _hfValue.Value;
            }
            set
            {
                EnsureChildControls();
                _hfValue.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        /// <value>
        /// The template.
        /// </value>
        public string Template
        {
            get
            {
                EnsureChildControls();
                return _hfTemplate.Value;
            }
            set
            {
                EnsureChildControls();
                _hfTemplate.Value = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        public string Url
        {
            get
            {
                EnsureChildControls();
                return _hfUrl.Value;
            }
            set
            {
                EnsureChildControls();
                _hfUrl.Value = value;
            }
        }        
       
        public int Limit
        {
            get
            {
                EnsureChildControls();
                return string.IsNullOrWhiteSpace( _hfLimit.Value ) ? 5 : int.Parse(_hfLimit.Value);
            }
            set
            {
                EnsureChildControls();
                _hfLimit.Value = value.ToString();
            }
        }      
        
        #endregion

        public AutoCompleteDropDown() : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = string.Format(
                @"Rock.controls.autoCompleteDropDown.initialize({{ controlId: '{0}', name: '{0}', valueControlId: '{1}', url: $('#{2}').val(), idkey: $('#{3}').val(), valuekey: $('#{4}').val(), template: $('#{5}').val(), header: $('#{6}').val(), footer: $('#{7}').val(), limit: parseInt($('#{8}').val()) }});",
                this.ClientID, _hfValue.ClientID, _hfUrl.ClientID, _hfIdProperty.ClientID, _hfNameProperty.ClientID, _hfTemplate.ClientID, _hfDropdownHeader.ClientID, _hfDropdownFooter.ClientID, _hfLimit.ClientID );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "auto-complete-drop-down-" + this.ID, script, true );
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _hfIdProperty = new HiddenField();
            _hfIdProperty.ID = "hfIdProperty";
            _hfIdProperty.Value = "Id";
            Controls.Add( _hfIdProperty );

            _hfNameProperty = new HiddenField();
            _hfNameProperty.ID = "hfNameProperty";
            _hfNameProperty.Value = "Name";
            Controls.Add( _hfNameProperty );

            _hfDropdownHeader = new HiddenField();
            _hfDropdownHeader.ID = "hfDropdownHeader";
            Controls.Add( _hfDropdownHeader );

            _hfDropdownFooter = new HiddenField();
            _hfDropdownFooter.ID = "hfDropdownFooter";
            Controls.Add( _hfDropdownFooter );

            _hfValue = new HiddenField();
            _hfValue.ID = "hfValue";
            Controls.Add( _hfValue );

            _hfTemplate = new HiddenField();
            _hfTemplate.ID = "hfResultTemplate";
            _hfTemplate.Value = "<p>{{value}}</p>";
            Controls.Add( _hfTemplate );

            _hfUrl = new HiddenField();
            _hfUrl.ID = "hfUrlTemplate";
            Controls.Add( _hfUrl );

            _hfLimit = new HiddenField();
            _hfLimit.ID = "hfLimit";
            _hfLimit.Value = "10";
            Controls.Add( _hfLimit );

            if ( RequiredFieldValidator != null )
            {
                RequiredFieldValidator.ID = "rfv";
                RequiredFieldValidator.ControlToValidate = _hfValue.ID;
                RequiredFieldValidator.Display = ValidatorDisplay.Dynamic;
                RequiredFieldValidator.CssClass = "validation-error help-inline";
                RequiredFieldValidator.Enabled = Required;
                Controls.Add( RequiredFieldValidator );
            }

            if ( HelpBlock != null )
            {
                HelpBlock.ID = "hb";
                Controls.Add( HelpBlock );
            }
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
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "control-group" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _hfIdProperty.RenderControl( writer );
            _hfNameProperty.RenderControl( writer );
            _hfDropdownHeader.RenderControl( writer );
            _hfDropdownFooter.RenderControl( writer );
            _hfValue.RenderControl( writer );
            _hfTemplate.RenderControl( writer );
            _hfUrl.RenderControl( writer );
            _hfLimit.RenderControl( writer );
            base.RenderControl( writer );

            writer.RenderEndTag();

        }

    }
}