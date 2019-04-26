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
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    [DefaultProperty( "PickerButtonTemplate" )]
    [ParseChildren( true, "PickerButtonTemplate" )]
    public class ItemFromBlockPicker : CompositeControl, IRockControl
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the CSS class of the select control.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "CSS class of the select control." )
        ]
        public string SelectControlCssClass
        {
            get { return ViewState["SelectControlCssClass"] as string ?? string.Empty; }
            set { ViewState["SelectControlCssClass"] = value; }
        }

        #endregion

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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemFromBlockPicker" /> class.
        /// </summary>
        public ItemFromBlockPicker()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion

        #region Fields

        private DynamicControlsPanel _pickerPanel;
        private Panel _pnlRolloverContainer;
        private LinkButton _lbShowPicker;
        private LinkButton _btnSelectNone;
        private ModalDialog _pickerDialog;
        private UserControl _pickerBlock;
        private HiddenField _hfPickerBlockSelectedValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the note view lava template.
        /// </summary>
        /// <value>
        /// The note view lava template.
        /// </value>
        [PersistenceMode( PersistenceMode.InnerDefaultProperty )]
        public string PickerButtonTemplate
        {
            get => this.ViewState["PickerButtonTemplate"] as string;
            set => this.ViewState["PickerButtonTemplate"] = value;
        }

        /// <summary>
        /// Gets or sets the Cascading Style Sheet (CSS) class rendered by the Web server control on the client.
        /// </summary>
        public override string CssClass
        {
            get => ViewState["CssClass"] as string;
            set => ViewState["CssClass"] = value;
        }

        /// <summary>
        /// Gets or sets the button text lava template.
        /// HINT: Use {{ SelectedText }} 
        /// </summary>
        /// <value>
        /// The button text template.
        /// </value>
        public string ButtonTextTemplate
        {
            get => ViewState["ButtonTextTemplate"] as string ?? "Select";
            set => ViewState["ButtonTextTemplate"] = value;
        }


        /// <summary>
        /// Gets or sets the BlockType.Guid to be used as the block that will present the Picker UI
        /// </summary>
        /// <value>
        /// The block type unique identifier.
        /// </value>
        public string BlockTypePath
        {
            get => ViewState["BlockTypePath"] as string;
            set => ViewState["BlockTypePath"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in modal].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in modal]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInModal
        {
            get => ViewState["ShowInModal"] as bool? ?? false;

            set
            {
                EnsureChildControls();
                ViewState["ShowInModal"] = value;
                _lbShowPicker.Visible = value;
                _pickerDialog.Visible = value;

                // make sure the picker is a control in the Dialog if in ShowModal mode
                Control pickerParent;
                if ( value )
                {
                    pickerParent = _pickerDialog.Content;
                }
                else
                {
                    pickerParent = this;
                }

                if ( _pickerPanel.Parent != pickerParent )
                {
                    _pickerPanel.Parent.Controls.Remove( _pickerPanel );
                    pickerParent.Controls.Add( _pickerPanel );
                }

                if ( _pickerBlock is IPickerBlock )
                {
                    // make sure out SelectItem event is only triggered from the Picker's SelectItem event when NOT in modal mode
                    // In ShowInModal mode, our SelectItem event will be trigger when the Modal dialog is closed with the 'Save/Select' button
                    ( _pickerBlock as IPickerBlock ).SelectItem -= PickerBlock_SelectItem;
                    if ( !value )
                    {
                        ( _pickerBlock as IPickerBlock ).SelectItem += PickerBlock_SelectItem;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the modal save button text.
        /// </summary>
        /// <value>
        /// The modal save button text.
        /// </value>
        public string ModalSaveButtonText
        {
            get
            {
                EnsureChildControls();
                return _pickerDialog.SaveButtonText;
            }

            set
            {
                EnsureChildControls();
                _pickerDialog.SaveButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets additional CSS classes for the modal save button.
        /// If js hooks are needed this is the place to add them.
        /// </summary>
        /// <value>
        /// The modal save button CSS class.
        /// </value>
        public string ModalSaveButtonCssClass
        {
            get
            {
                EnsureChildControls();
                return _pickerDialog.SaveButtonCssClass;
            }

            set
            {
                EnsureChildControls();
                _pickerDialog.SaveButtonCssClass = value;
            }
        }

        /// <summary>
        /// Gets or sets the modal title.
        /// </summary>
        /// <value>
        /// The modal title.
        /// </value>
        public string ModalTitle
        {
            get
            {
                EnsureChildControls();
                return _pickerDialog.Title;
            }
            set
            {
                EnsureChildControls();
                _pickerDialog.Title = value;
            }
        }

        /// <summary>
        /// Shows the modal.
        /// </summary>
        public void ShowModal()
        {
            EnsureChildControls();
            if ( _pickerBlock is IPickerBlock )
            {
                // ensure the picker has the SelectedValue set to what we currently have as the SelectedValue
                ( _pickerBlock as IPickerBlock ).SelectedValue = this.SelectedValue;
            }

            _pickerDialog.Show();
        }

        /// <summary>
        /// Gets the picker block.
        /// </summary>
        /// <value>
        /// The picker block.
        /// </value>
        public IPickerBlock PickerBlock
        {
            get
            {
                EnsureChildControls();
                return _pickerBlock as IPickerBlock;
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _pnlRolloverContainer = new Panel();
            this.Controls.Add( _pnlRolloverContainer );

            _lbShowPicker = new LinkButton();
            _lbShowPicker.CausesValidation = false;
            _lbShowPicker.ID = this.ID + "_lbShowPicker";
            _lbShowPicker.Click += _lbShowPicker_Click;
            _pnlRolloverContainer.Controls.Add( _lbShowPicker );

            _btnSelectNone = new LinkButton();
            _btnSelectNone.ID = this.ID + "_btnSelectNone";
            _btnSelectNone.CssClass = "picker-select-none";
            _btnSelectNone.Text = "<i class='fa fa-times'></i>";
            _btnSelectNone.CausesValidation = false;
            _btnSelectNone.Click += _lbClearPicker_Click;
            _pnlRolloverContainer.Controls.Add( _btnSelectNone );

            _pickerDialog = new ModalDialog();
            _pickerDialog.ID = this.ID + "_pickerDialog";
            _pickerDialog.SaveClick += _pickerDialog_SaveClick;
            this.Controls.Add( _pickerDialog );

            // NOTE: ShowInModal could be set AFTER CreateChildControls, so there is also logic in ShowInModal's setter that configures based on ShowInModal
            _lbShowPicker.Visible = this.ShowInModal;
            _pickerDialog.Visible = this.ShowInModal;
            _pickerDialog.SaveButtonText = "Select";

            _hfPickerBlockSelectedValue = new HiddenField
            {
                ID = "_hfPickerBlockSelectedValue"
            };

            Controls.Add( _hfPickerBlockSelectedValue );

            _pickerPanel = new DynamicControlsPanel()
            {
                ID = "_pickerPanel"
            };

            if ( BlockTypePath.IsNotNullOrWhiteSpace() )
            {
                var rockPage = System.Web.HttpContext.Current.Handler as RockPage;
                _pickerBlock = rockPage.TemplateControl.LoadControl( BlockTypePath ) as UserControl;
                _pickerBlock.ID = "_pickerBlock";

                var pageCache = PageCache.Get( rockPage.PageId );
                ( _pickerBlock as RockBlock )?.SetBlock( pageCache, null, false, false );

                if ( this.ShowInModal )
                {
                    _pickerDialog.Content.Controls.Add( _pickerPanel );
                }
                else
                {
                    this.Controls.Add( _pickerPanel );
                    if ( _pickerBlock is IPickerBlock )
                    {
                        ( _pickerBlock as IPickerBlock ).SelectItem += PickerBlock_SelectItem;
                    }
                }

                if ( _pickerBlock is IPickerBlock )
                {
                    _pickerPanel.Controls.Add( _pickerBlock );
                }
                else
                {
                    // enforce that the block has to implement IPickerBLock
                    var nbInvalidBLock = new NotificationBox { NotificationBoxType = NotificationBoxType.Danger, Text = $"<strong>{BlockTypePath}<strong> is not a valid PickerBlock", Visible = true };
                    _pickerPanel.Controls.Add( nbInvalidBLock );
                }

            }
        }

        /// <summary>
        /// Handles the SaveClick event of the _pickerDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _pickerDialog_SaveClick( object sender, EventArgs e )
        {
            _pickerDialog.Hide();

            // if the picker was in a modal dialog, track the SelectValue and SelectedText in a hidden when saved 
            _hfPickerBlockSelectedValue.Value = ( _pickerBlock as IPickerBlock )?.SelectedValue;

            SelectItem?.Invoke( this, e );
        }

        /// <summary>
        /// Handles the Click event of the _lbShowPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _lbShowPicker_Click( object sender, EventArgs e )
        {
            ShowModal();
        }

        /// <summary>
        /// Handles the Click event of the _lbClearPicker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _lbClearPicker_Click( object sender, EventArgs e )
        {
            this.SelectedValue = null;
            SelectItem?.Invoke( this, e );
        }

        /// <summary>
        /// Handles the SelectItem event of the PickerBlock control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void PickerBlock_SelectItem( object sender, EventArgs e )
        {
            SelectItem?.Invoke( this, e );
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
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockBlock().RockPage, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "SelectedText", SelectedText );
            mergeFields.Add( "SelectedValue", SelectedValue ?? string.Empty );

            _lbShowPicker.Text = this.PickerButtonTemplate.ResolveMergeFields( mergeFields );
            _btnSelectNone.Visible = SelectedValue.IsNotNullOrWhiteSpace() && _lbShowPicker.Visible;

            if ( this.ShowInModal )
            {
                _lbShowPicker.CssClass = this.SelectControlCssClass;
                _pnlRolloverContainer.CssClass = this.CssClass;
            }
            else
            {
                base.CssClass = this.SelectControlCssClass + " " + this.CssClass;
            }

            base.RenderControl( writer );
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
                EnsureChildControls();

                if ( this.ShowInModal == true )
                {
                    // if shown in a modal, track the SelectedValue in _hfPickerBlockSelectedValue since the pickerBlock could be cancelled
                    return _hfPickerBlockSelectedValue.Value;
                }
                else
                {
                    var pickerBlock = _pickerBlock as IPickerBlock;
                    return pickerBlock?.SelectedValue;
                }
            }

            set
            {
                EnsureChildControls();

                var pickerBlock = _pickerBlock as IPickerBlock;
                if ( pickerBlock != null )
                {
                    pickerBlock.SelectedValue = value;
                }

                _hfPickerBlockSelectedValue.Value = value;
            }
        }

        /// <summary>
        /// Gets the selected text.
        /// </summary>
        /// <value>
        /// The selected text.
        /// </value>
        public string SelectedText
        {
            get
            {
                EnsureChildControls();
                var pickerBlock = _pickerBlock as IPickerBlock;
                return pickerBlock?.GetSelectedText( this.SelectedValue );
            }
        }

        /// <summary>
        /// Occurs when [select item].
        /// </summary>
        public event EventHandler SelectItem;

        #endregion
    }
}
