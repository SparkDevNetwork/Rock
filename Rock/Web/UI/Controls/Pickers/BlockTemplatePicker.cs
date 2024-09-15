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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a template to use in a block
    /// </summary>
    [ToolboxData( "<{0}:BlockTemplatePicker runat=server></{0}:BlockTemplatePicker>" )]
    public class BlockTemplatePicker : CompositeControl, IRockControl
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

        private HiddenFieldWithClass _hfTemplateKey;

        private Panel _pnlStandard;
        private Panel _pnlCustom;
        private CodeEditor _ceLavaTemplate;
        private RockRadioButton _rbbCustom;
        private LinkButton _btnCustomize;
        private LinkButton _btnStandard;
        private HtmlGenericControl _actionDiv;
        private static readonly Guid _CustomGuid = new Guid( "ffffffff-ffff-ffff-ffff-ffffffffffff" );
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the template key.
        /// </summary>
        /// <value>
        /// The template key.
        /// </value>
        public Guid? TemplateKey
        {
            get
            {
                EnsureChildControls();
                return _hfTemplateKey.Value.AsGuidOrNull();
            }

            set
            {
                EnsureChildControls();
                _hfTemplateKey.Value = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the template value.
        /// </summary>
        /// <value>
        /// The template value.
        /// </value>
        public string TemplateValue
        {
            get
            {
                EnsureChildControls();
                return _ceLavaTemplate.Text;
            }

            set
            {
                EnsureChildControls();
                _ceLavaTemplate.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the template block defined value identifier ( Required )
        /// </summary>
        /// <value>
        /// The template block defined value identifier.
        /// </value>
        public int? TemplateBlockValueId
        {
            get
            {
                return _templateBlockValueId;
            }

            set
            {
                _templateBlockValueId = value;
            }
        }

        /// <summary>
        /// The template block identifier
        /// </summary>
        private int? _templateBlockValueId;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockTemplatePicker" /> class.
        /// </summary>
        public BlockTemplatePicker()
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
                sm.RegisterAsyncPostBackControl( _btnCustomize );
                sm.RegisterAsyncPostBackControl( _btnStandard );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _hfTemplateKey = new HiddenFieldWithClass();
            _hfTemplateKey.ID = this.ID + "_hfValue";
            _hfTemplateKey.CssClass = "js-template-id";
            this.Controls.Add( _hfTemplateKey );

            _pnlStandard = new Panel();
            _pnlStandard.ID = this.ID + "_pnlOption";
            _pnlStandard.CssClass = "js-template-option-panel";
            this.Controls.Add( _pnlStandard );

            _btnCustomize = new LinkButton();
            _btnCustomize.CssClass = "btn btn-xs btn-link";
            _btnCustomize.ID = string.Format( "{0}_btnCustomize", this.ID );
            _btnCustomize.Text = "Customize";
            _btnCustomize.CausesValidation = false;
            _btnCustomize.Click += btnCustomize_Click;
            this.Controls.Add( _btnCustomize );

            _pnlCustom = new Panel();
            _pnlCustom.ID = this.ID + "pnlCustom";
            _pnlCustom.CssClass = "js-template-custom-panel";
            this.Controls.Add( _pnlCustom );

            _rbbCustom = new RockRadioButton();
            _rbbCustom.Checked = true;
            _rbbCustom.Text = "<b>Custom</b>";
            _pnlCustom.Controls.Add( _rbbCustom );

            _actionDiv = new HtmlGenericControl( "div" );
            _pnlCustom.Controls.Add( _actionDiv );
            _actionDiv.Attributes["class"] = "actions clearfix";

            _btnStandard = new LinkButton();
            _btnStandard.CssClass = "btn btn-xs btn-link pull-right";
            _btnStandard.ID = string.Format( "{0}_btnStandard", this.ID );
            _btnStandard.Text = "Use Standard Templates";
            _btnStandard.CausesValidation = false;
            _btnStandard.Click += btnStandard_Click;
            _actionDiv.Controls.Add( _btnStandard );

            _ceLavaTemplate = new CodeEditor();
            _ceLavaTemplate.ID = this.ID + "_ceLavaTemplate";
            _ceLavaTemplate.CssClass = "js-template-lava-field";
            _pnlCustom.Controls.Add( _ceLavaTemplate );

            RockControlHelper.CreateChildControls( this, Controls );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                if ( !HasAnyTemplates() )
                {
                    _hfTemplateKey.Value = _CustomGuid.ToString();
                    _btnStandard.Visible = false;
                }
                else
                {
                    _btnStandard.Visible = true;
                }

                RequiredFieldValidator.InitialValue = "";
                if ( _hfTemplateKey.Value.AsGuidOrNull() == _CustomGuid )
                {
                    RequiredFieldValidator.ControlToValidate = _ceLavaTemplate.ID;
                }
                else
                {
                    // override a couple of property values on RequiredFieldValidator so that Validation works correctly

                    RequiredFieldValidator.ControlToValidate = _hfTemplateKey.ID;
                }

                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {

            var value = _hfTemplateKey.Value.AsGuidOrNull();
            bool isCustom = value == _CustomGuid;
            _pnlCustom.Visible = isCustom;
            _pnlStandard.Visible = !isCustom;

            var row = new HtmlGenericControl( "div" );
            _pnlStandard.Controls.Add( row );
            row.Attributes["class"] = "row js-template-row";

            int optionCount = 0;
            StringBuilder htmlBuilder = new StringBuilder();
            if ( _templateBlockValueId.HasValue )
            {
                var blockTemplateDefinedValue = DefinedValueCache.Get( _templateBlockValueId.Value );

                if ( blockTemplateDefinedValue != null )
                {
                    var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.TEMPLATE );
                    definedType.DefinedValues.LoadAttributes();
                    foreach ( var item in definedType.DefinedValues )
                    {
                        if ( item.GetAttributeValue( "TemplateBlock" ).AsGuid() == blockTemplateDefinedValue.Guid )
                        {
                            optionCount += 1;
                            if ( _hfTemplateKey.Value.IsNullOrWhiteSpace() )
                            {
                                _hfTemplateKey.Value = item.Guid.ToString();
                            }

                            var imageUrl = FileUrlHelper.GetImageUrl( item.GetAttributeValue( "Icon" ).AsGuid() );
                            var imgSrc = $"<img src='{ResolveUrl( imageUrl )}' width='100%'/>";

                            string html = string.Format( @" <div class='col-md-2 col-sm-4 template-picker-item'>
                                                    <div class='radio'>
                                                        <label><input type='radio' class='js-template-picker' name='template-id-{5}' id='{0}' value='{0}' {2} {3}><span class='label-text'><b>{1}</b></span></label>
                                                    </div>
                                                    {4}
                                                </div>",
                                item.Guid,
                                item.Value,
                                _hfTemplateKey.Value.AsGuid() == item.Guid ? "checked" : "",
                                this.Enabled ? "" : "disabled",
                                imgSrc,
                                this.ID );
                            htmlBuilder.Append( html );
                        }
                    }
                }
            }


            _hfTemplateKey.RenderControl( writer );

            if ( optionCount > 0 )
            {
                row.Controls.Add( new LiteralControl { Text = htmlBuilder.ToString() } );

                var actionDiv = new HtmlGenericControl( "div" );
                _pnlStandard.Controls.Add( actionDiv );
                actionDiv.Attributes["class"] = "actions clearfix";

                _pnlStandard.Controls.Add( _btnCustomize );
            }

            _pnlStandard.Enabled = this.Enabled;
            _pnlCustom.Enabled = this.Enabled;

            _pnlStandard.RenderControl( writer );
            _rbbCustom.Checked = true;
            _pnlCustom.RenderControl( writer );

            RegisterClientScript();
        }

        /// <summary>
        /// Determines whether we have any valid templates to choose from. If not then
        /// we should configure as custom content only.
        /// </summary>
        /// <returns>A boolean that indicates if we have any block templates.</returns>
        private bool HasAnyTemplates()
        {
            if ( _templateBlockValueId.HasValue )
            {
                var blockTemplateDefinedValue = DefinedValueCache.Get( _templateBlockValueId.Value );

                if ( blockTemplateDefinedValue != null )
                {
                    var definedType = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.TEMPLATE );

                    foreach ( var item in definedType.DefinedValues )
                    {
                        if ( item.GetAttributeValue( "TemplateBlock" ).AsGuid() == blockTemplateDefinedValue.Guid )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void RegisterClientScript()
        {
            string script = $@"
      $('input[type=radio][name=template-id-{this.ID}]').on('change', function () {{
            $('#{_hfTemplateKey.ClientID}').val($(this).val());
        }});
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "block-template-picker-"+this.ID, script, true );
        }

        /// <summary>
        /// Handles the Click event of the _btnStandard control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnStandard_Click( object sender, EventArgs e )
        {
            _hfTemplateKey.Value = "";
            _ceLavaTemplate.Text = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the _btnCustomize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCustomize_Click( object sender, EventArgs e )
        {
            var definedValue = DefinedValueCache.Get( _hfTemplateKey.Value );
            _hfTemplateKey.Value = _CustomGuid.ToString();
            _ceLavaTemplate.Text = definedValue.Description;
        }

        #endregion
    }
}