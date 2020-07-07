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

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A Modal Popup Dialog Window
    /// </summary>
    [ToolboxData( "<{0}:ModalDialog runat=server></{0}:ModalDialog>" )]
    public class ModalDialog : CompositeControl, INamingContainer, IHasValidationGroup
    {
        private HiddenFieldWithClass _hfModalVisible;
        private Panel _dialogPanel;

        private Panel _headerPanel;
        private HtmlGenericControl _closeLink;
        private HtmlGenericControl _titleH3;
        private Literal _title;
        private HtmlGenericControl _subtitleSmall;
        private Literal _subtitle;

        private Panel _bodyPanel;
        private Panel _contentPanel;

        private Panel _footerPanel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModalDialog"/> class.
        /// </summary>
        public ModalDialog() : base()
        {
            this.Visible = false;
        }

        /// <summary>
        /// Gets the server save link.
        /// </summary>
        /// <value>
        /// The server save link.
        /// </value>
        public HtmlAnchor ServerSaveLink
        {
            get
            {
                return _serverSaveLink;
            }
        }

        private HtmlAnchor _serverSaveLink;
        private HtmlAnchor _saveLink;
        private HtmlAnchor _cancelLink;
        private HtmlAnchor _saveThenAddLink;

        /// <summary>
        /// Raises the <see cref="E:Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var sm = ScriptManager.GetCurrent( this.Page );
            EnsureChildControls();

            if ( sm != null )
            {
                // register the buttons as async buttons in case the modal is used in an UpdatePanel
                sm.RegisterAsyncPostBackControl( _cancelLink );
                sm.RegisterAsyncPostBackControl( _serverSaveLink );
                sm.RegisterAsyncPostBackControl( _saveLink );
                sm.RegisterAsyncPostBackControl( _saveThenAddLink );
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [
            Category( "Appearance" ),
            DefaultValue( "" ),
            Description( "The title of the dialog." )
        ]
        public string Title
        {
            get
            {
                EnsureChildControls();
                return _title.Text;
            }

            set
            {
                EnsureChildControls();
                _title.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the subtitle.
        /// </summary>
        /// <value>
        /// The subtitle.
        /// </value>
        [
            Category( "Appearance" ),
            DefaultValue( "" ),
            Description( "The subtitle of the dialog." )
        ]
        public string SubTitle
        {
            get
            {
                EnsureChildControls();
                return _subtitle.Text;
            }

            set
            {
                EnsureChildControls();
                _subtitle.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the save button text.
        /// </summary>
        /// <value>The save button text.</value>
        public string SaveButtonText
        {
            get => ViewState["SaveButtonText"] as string ?? "Save";
            set => ViewState["SaveButtonText"] = value;
        }

        /// <summary>
        /// Gets or sets additional CSS classes for the modal save button.
        /// If js hooks are needed this is the place to add them.
        /// </summary>
        /// <value>
        /// The save button CSS class.
        /// </value>
        public string SaveButtonCssClass
        {
            get => ViewState["SaveButtonCSSClass"] as string ?? string.Empty;
            set => ViewState["SaveButtonCSSClass"] = value;
        }

        /// <summary>
        /// Gets the Control for the SaveButton
        /// </summary>
        /// <value>
        /// The save button.
        /// </value>
        public bool SaveButtonCausesValidation
        {
            get => ViewState["SaveButtonCausesValidation"] as bool? ?? true;
            set => ViewState["SaveButtonCausesValidation"] = value;
        }

        /// <summary>
        /// Gets or sets the save then add button text.
        /// </summary>
        /// <value>The post save then add button text.</value>
        public string SaveThenAddButtonText
        {
            get { return ViewState["SaveThenAddButtonText"] as string ?? "Save Then Add"; }
            set { ViewState["SaveThenAddButtonText"] = value; }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string ?? string.Empty; }
            set { ViewState["ValidationGroup"] = value; }
        }

        /// <summary>
        /// Gets or sets the on ok script.
        /// </summary>
        /// <value>
        /// The on ok script.
        /// </value>
        public string OnOkScript
        {
            get { return ViewState["OnOkScript"] as string ?? string.Empty; }
            set { ViewState["OnOkScript"] = value; }
        }

        /// <summary>
        /// Gets or sets the on cancel script.
        /// </summary>
        /// <value>
        /// The on cancel script.
        /// </value>
        public string OnCancelScript
        {
            get { return ViewState["OnCancelScript"] as string ?? string.Empty; }
            set { ViewState["OnCancelScript"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [cancel link visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [cancel link visible]; otherwise, <c>false</c>.
        /// </value>
        public bool CancelLinkVisible
        {
            get
            {
                EnsureChildControls();
                return _cancelLink.Visible;
            }

            set
            {
                EnsureChildControls();
                _cancelLink.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the close button (The X in the Header) is visible
        /// </summary>
        /// <value>
        ///   <c>true</c> if [close link visible]; otherwise, <c>false</c>.
        /// </value>
        public bool CloseLinkVisible
        {
            get
            {
                EnsureChildControls();
                return _closeLink.Visible;
            }
            set
            {
                EnsureChildControls();
                _closeLink.Visible = value;
            }
        }

        /// <summary>
        /// Header panel control
        /// </summary>
        public Panel Header
        {
            get
            {
                EnsureChildControls();
                return _headerPanel;
            }
        }

        /// <summary>
        /// The content of the popup.
        /// </summary>
        [
            DesignerSerializationVisibility( DesignerSerializationVisibility.Content ),
            NotifyParentProperty( true ),
            PersistenceMode( PersistenceMode.InnerProperty ),
            Category( "Data" ),
            DefaultValue( typeof( Panel ), "" ),
            Description( "The Contents of the dialog." )
        ]
        public Panel Content
        {
            get
            {
                EnsureChildControls();
                return _contentPanel;
            }
        }

        /// <summary>
        /// Footer panel control
        /// </summary>
        public Panel Footer
        {
            get
            {
                EnsureChildControls();
                return _footerPanel;
            }
        }

        /// <summary>
        /// Hides this instance.
        /// </summary>
        public void Hide()
        {
            Hide( null );
        }

        /// <summary>
        /// Hides this instance.
        /// </summary>
        /// <param name="managerId">The ID of the modal's manager (HTML element).</param>
        public void Hide( string managerId )
        {
            EnsureChildControls();
            _hfModalVisible.Value = "0";

            var managerSelector = managerId.IsNullOrWhiteSpace() ? GetModalManagerSelector() : $"#{managerId}";
            var managerArg = !string.IsNullOrWhiteSpace( managerId ) ? $", $('{managerSelector}')" : string.Empty;

            // make sure the close script gets fired, even if the modal isn't rendered
            string hideScript = string.Format( "Rock.controls.modal.closeModalDialog($('#{0}'){1});", _dialogPanel.ClientID, managerArg );
            ScriptManager.RegisterStartupScript( this, this.GetType(), "modaldialog-hide-" + this.ClientID, hideScript, true );

            this.Visible = false;
        }

        /// <summary>
        /// Shows this instance.
        /// </summary>
        public void Show()
        {
            this.Visible = true;

            EnsureChildControls();
            _hfModalVisible.Value = "1";
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            // main container
            _dialogPanel = new Panel();
            Controls.Add( _dialogPanel );
            _dialogPanel.ID = "modal_dialog_panel";
            _dialogPanel.CssClass = "modal container modal-content rock-modal rock-modal-frame";

            _hfModalVisible = new HiddenFieldWithClass();
            _hfModalVisible.CssClass = "js-modal-visible";
            _hfModalVisible.ID = "hfModalVisible";
            _dialogPanel.Controls.Add( _hfModalVisible );

            // modal-header
            _headerPanel = new Panel();
            _dialogPanel.Controls.Add( _headerPanel );
            _headerPanel.CssClass = "modal-header";

            _closeLink = new HtmlGenericControl( "button" );
            _headerPanel.Controls.Add( _closeLink );
            _closeLink.ID = "closeLink";
            _closeLink.Attributes.Add( "class", "close js-modaldialog-close-link" );
            _closeLink.Attributes.Add( "aria-hidden", "true" );
            _closeLink.Attributes.Add( "type", "button" );
            _closeLink.Attributes.Add( "data-dismiss", "modal" );
            _closeLink.InnerHtml = "&times;";

            _titleH3 = new HtmlGenericControl( "h3" );
            _titleH3.AddCssClass( "modal-title" );
            _headerPanel.Controls.Add( _titleH3 );

            // _title control for public this.Title
            _title = new Literal();
            _title.Text = string.Empty;
            _titleH3.Controls.Add( _title );

            // _subtitle controls for public this.Subtitle
            _subtitleSmall = new HtmlGenericControl( "small" );
            _subtitleSmall.AddCssClass( "js-subtitle" );
            _headerPanel.Controls.Add( _subtitleSmall );
            _subtitle = new Literal();
            _subtitle.Text = string.Empty;
            _subtitleSmall.Controls.Add( _subtitle );

            // modal-body and content
            _bodyPanel = new Panel();
            _bodyPanel.CssClass = "modal-body";
            _dialogPanel.Controls.Add( _bodyPanel );

            // for this.Content
            _contentPanel = new Panel();
            _bodyPanel.Controls.Add( _contentPanel );
            _contentPanel.ID = "contentPanel";

            // modal-footer
            _footerPanel = new Panel();
            _dialogPanel.Controls.Add( _footerPanel );
            _footerPanel.CssClass = "modal-footer";

            _cancelLink = new HtmlAnchor();
            _footerPanel.Controls.Add( _cancelLink );
            _cancelLink.ID = "cancelLink";
            _cancelLink.Attributes.Add( "class", "btn btn-link js-modaldialog-cancel-link" );
            _cancelLink.Attributes.Add( "data-dismiss", "modal" );
            _cancelLink.InnerText = "Cancel";

            _saveThenAddLink = new HtmlAnchor();
            _footerPanel.Controls.Add( _saveThenAddLink );
            _saveThenAddLink.ID = "saveThenAddLink";
            _saveThenAddLink.Attributes.Add( "class", "btn btn-link" );
            _saveThenAddLink.ServerClick += SaveThenAdd_ServerClick;

            _serverSaveLink = new HtmlAnchor();
            _footerPanel.Controls.Add( _serverSaveLink );
            _serverSaveLink.ID = "serverSaveLink";
            _serverSaveLink.Attributes.Add( "class", $"btn btn-primary {SaveButtonCssClass}" );
            _serverSaveLink.ServerClick += SaveLink_ServerClick;

            _saveLink = new HtmlAnchor();
            _footerPanel.Controls.Add( _saveLink );
            _saveLink.ID = "saveLink";
            _saveLink.Attributes.Add( "class", $"btn btn-primary js-modaldialog-save-link {SaveButtonCssClass}" );
        }

        /// <summary>
        /// Raises the <see cref="E:PreRender"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            RegisterJavaScript();

            _serverSaveLink.Visible = !string.IsNullOrWhiteSpace( SaveButtonText ) && SaveClick != null;
            _serverSaveLink.InnerText = SaveButtonText;
            _serverSaveLink.AddCssClass( SaveButtonCssClass );
            if ( SaveButtonCausesValidation )
            {
                _serverSaveLink.CausesValidation = true;
                _serverSaveLink.ValidationGroup = this.ValidationGroup;
            }
            else
            {
                _serverSaveLink.CausesValidation = false;
                _serverSaveLink.ValidationGroup = "";
            }

            _saveLink.Visible = !string.IsNullOrWhiteSpace( SaveButtonText ) && SaveClick == null && !string.IsNullOrWhiteSpace( OnOkScript );
            _saveLink.InnerText = SaveButtonText;
            _saveLink.AddCssClass( SaveButtonCssClass );
            _saveLink.ValidationGroup = this.ValidationGroup;

            _saveThenAddLink.Visible = !string.IsNullOrWhiteSpace( SaveThenAddButtonText ) && SaveThenAddClick != null;
            _saveThenAddLink.InnerText = SaveThenAddButtonText;
            _saveThenAddLink.ValidationGroup = this.ValidationGroup;

            if ( !_serverSaveLink.Visible && !_saveLink.Visible )
            {
                _cancelLink.InnerText = "OK";
            }
            else
            {
                _cancelLink.InnerText = "Cancel";
            }

            base.OnPreRender( e );
        }

        /// <summary>
        /// Gets the modal manager.
        /// </summary>
        /// <returns></returns>
        protected string GetModalManagerSelector()
        {
            var parentPanel = this.ParentUpdatePanel();
            return parentPanel != null ? "#" + parentPanel.ClientID : "body";
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        protected void RegisterJavaScript()
        {
            var modalManager = GetModalManagerSelector();

            // if this is Modal is a child of a another Modal, make sure the parent modal stays visible if this modal is closed
            var parentModal = this.FirstParentControlOfType<ModalDialog>();
            string showParentModalOnCloseScript = null;
            if ( parentModal != null )
            {
                showParentModalOnCloseScript = $@"Rock.controls.modal.showModalDialog($('#{parentModal._dialogPanel.ClientID}'), '{modalManager}');";
            }

            // if the Modal's manger is an UpdatePanel, provide the appropriate jQuery selector to the closeModalDialog function
            var closeDialogManagerArg = modalManager != "body" ? $", $('{modalManager}')" : string.Empty;

            string script = $@"
if ($('#{_hfModalVisible.ClientID}').val() == '1') {{
    Rock.controls.modal.showModalDialog($('#{_dialogPanel.ClientID}'), '{modalManager}');
}}
else {{
    Rock.controls.modal.closeModalDialog($('#{_dialogPanel.ClientID}'){closeDialogManagerArg});
}}

$('#{_cancelLink.ClientID}, #{_closeLink.ClientID}').on('click', function () {{
    {this.OnCancelScript}
    $('#{_hfModalVisible.ClientID}').val('0');
    Rock.controls.modal.closeModalDialog($('#{_dialogPanel.ClientID}'){closeDialogManagerArg});
    {showParentModalOnCloseScript}
}});

$('#{_saveLink.ClientID}').on('click', function () {{
    {this.OnOkScript}
    $('#{_hfModalVisible.ClientID}').val('0');
    Rock.controls.modal.closeModalDialog($('#{_dialogPanel.ClientID}'){closeDialogManagerArg});
}});

";

            ScriptManager.RegisterStartupScript( this, this.GetType(), "modaldialog-show-" + this.ClientID, script, true );
        }

        /// <summary>
        /// Handles the ServerClick event of the SaveLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void SaveLink_ServerClick( object sender, EventArgs e )
        {
            if ( SaveClick != null )
            {
                SaveClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the ServerClick event of the SaveThenAddLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void SaveThenAdd_ServerClick( object sender, EventArgs e )
        {
            if ( SaveThenAddClick != null )
            {
                SaveThenAddClick( sender, e );
            }
        }

        /// <summary>
        /// Occurs when the save button is clicked.
        /// </summary>
        public event EventHandler SaveClick;

        /// <summary>
        /// Occurs after the save then add button is clicked.
        /// </summary>
        public event EventHandler SaveThenAddClick;
    }
}