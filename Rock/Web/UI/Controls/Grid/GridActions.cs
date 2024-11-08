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
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    ///
    /// </summary>
    [ToolboxData( "<{0}:GridActions runat=server></{0}:GridActions>" )]
    public class GridActions : CompositeControl
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GridActions" /> class.
        /// </summary>
        /// <param name="parentGrid">The parent grid.</param>
        public GridActions( Grid parentGrid )
        {
            _parentGrid = parentGrid;
            _customActions = new List<Control>();
            _customActionConfigEvents = new List<CustomActionConfigEvent>();
        }

        #endregion

        #region Fields

        private Grid _parentGrid;

        #endregion

        #region Controls

        private List<Control> _customActions;
        private List<CustomActionConfigEvent> _customActionConfigEvents;
        private PlaceHolder _pnlCustomActions;
        private LinkButton _lbPersonMerge;
        private LinkButton _lbBusinessMerge;
        private LinkButton _lbBulkUpdate;
        private LinkButton _lbDefaultLaunchWorkflow;
        private List<LinkButton> _customActionButtons = new List<LinkButton>();
        private LinkButton _lbCommunicate;
        private HtmlGenericControl _aAdd;
        private LinkButton _lbAdd;
        private HtmlGenericControl _aExcelExport;
        private LinkButton _lbExcelExport;
        private LinkButton _lbMergeTemplate;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [show communicate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show communicate]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowCommunicate
        {
            get
            {
                // if the Grid has the PersonIdField set, default ShowCommunicate to True
                bool hasPersonIdField = _parentGrid.CommunicationRecipientPersonIdFields.Any() || _parentGrid.PersonIdField.IsNotNullOrWhiteSpace();

                return ViewState["ShowCommunicate"] as bool? ?? hasPersonIdField;
            }

            set
            {
                ViewState["ShowCommunicate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show merge person].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show merge person]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMergePerson
        {
            get
            {
                // if the Grid has the PersonIdField set, default ShowMergePerson to True
                bool hasPersonIdField = _parentGrid.PersonIdField.IsNotNullOrWhiteSpace();

                return ViewState["ShowMergePerson"] as bool? ?? hasPersonIdField;
            }

            set
            {
                ViewState["ShowMergePerson"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show launch workflow].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show launch workflow]; otherwise, <c>false</c>.
        /// </value>
        private bool ShowDefaultLaunchWorkflowButton
        {
            get =>
                _parentGrid.ShowWorkflowOrCustomActionButtons &&
                _parentGrid.EnableDefaultLaunchWorkflow &&
                _parentGrid.EntityTypeId.HasValue;
        }

        /// <summary>
        /// Gets a value indicating whether [show custom action buttons].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show custom action buttons]; otherwise, <c>false</c>.
        /// </value>
        private bool ShowCustomActionButtons
        {
            get =>
                _parentGrid.ShowWorkflowOrCustomActionButtons &&
                _parentGrid.CustomActionConfigs != null &&
                ( _parentGrid.CustomActionConfigs.Any() || _customActionConfigEvents.Any() ) &&
                _parentGrid.EntityTypeId.HasValue;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show bulk update].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show bulk update]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowBulkUpdate
        {
            get
            {
                // if the Grid has the PersonIdField set, default ShowBulkUpdate to True
                bool hasPersonIdField = _parentGrid.PersonIdField.IsNotNullOrWhiteSpace();

                return ViewState["ShowBulkUpdate"] as bool? ?? hasPersonIdField;
            }

            set
            {
                ViewState["ShowBulkUpdate"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show add].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show add]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAdd
        {
            get { return ViewState["ShowAdd"] as bool? ?? false; }
            set { ViewState["ShowAdd"] = value; }
        }

        /// <summary>
        /// Gets the add button.
        /// </summary>
        /// <value>
        /// The add button.
        /// </value>
        public LinkButton AddButton
        {
            get
            {
                EnsureChildControls();
                return _lbAdd;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show excel export].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show excel export]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowExcelExport
        {
            get { return ViewState["ShowExcelExport"] as bool? ?? true; }
            set { ViewState["ShowExcelExport"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show merge template].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show merge template]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMergeTemplate
        {
            get { return ViewState["ShowMergeTemplate"] as bool? ?? true; }
            set { ViewState["ShowMergeTemplate"] = value; }
        }

        /// <summary>
        /// Gets or sets the client add script.
        /// </summary>
        /// <value>
        /// The client add script.
        /// </value>
        public string ClientAddScript
        {
            get
            {
                EnsureChildControls();
                return _aAdd.Attributes["onclick"];
            }

            set
            {
                EnsureChildControls();
                _aAdd.Attributes["onclick"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the client excel export script.
        /// </summary>
        /// <value>
        /// The client excel export script.
        /// </value>
        public string ClientExcelExportScript
        {
            get
            {
                EnsureChildControls();
                return _aExcelExport.Attributes["onclick"];
            }

            set
            {
                EnsureChildControls();
                _aExcelExport.Attributes["onclick"] = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="T:System.Web.UI.HtmlTextWriterTag"/> value that corresponds to this Web server control. This property is used primarily by control developers.
        /// </summary>
        /// <returns>One of the <see cref="T:System.Web.UI.HtmlTextWriterTag"/> enumeration values.</returns>
        protected override HtmlTextWriterTag TagKey
        {
            get { return HtmlTextWriterTag.Unknown; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            EnsureChildControls();

            var sm = ScriptManager.GetCurrent( Page );

            // Excel Export requires a Full Page postback
            sm.RegisterPostBackControl( _lbExcelExport );

            // make sure delete button is registered for async postback (needed just in case the grid was created at runtime)
            sm.RegisterAsyncPostBackControl( _lbAdd );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _pnlCustomActions = new PlaceHolder();

            Controls.Add( _pnlCustomActions );

            if ( _customActions != null )
            {
                foreach ( Control control in _customActions )
                {
                    _pnlCustomActions.Controls.Add( control );
                }
            }

            // Combine the Custom Action Config lists into one.
            List<CustomActionConfig> configList = new List<CustomActionConfig>();
            configList.AddRange( _parentGrid.CustomActionConfigs );
            configList.AddRange( _customActionConfigEvents );

            // Build custom action buttons from the block or from the configs.
            if ( configList?.Any() == true )
            {
                var index = 1;
                _customActionButtons = new List<LinkButton>();

                foreach ( var config in configList )
                {
                    var linkButton = new LinkButton();
                    _customActionButtons.Add( linkButton );

                    linkButton.ID = $"lbCustomAction-{index}";
                    linkButton.CssClass = "btn-grid-action btn-custom-action btn btn-default btn-sm";
                    linkButton.ToolTip = config.HelpText.IsNullOrWhiteSpace() ? "Custom Action" : config.HelpText;

                    if ( config is ICustomActionEventHandler configEvent )
                    {
                        linkButton.Click += configEvent.EventHandler;
                        linkButton.CommandArgument = config.Route;
                    }
                    else
                    {
                        linkButton.CommandArgument = config.Route;
                        linkButton.CommandName = "Route";
                        linkButton.Command += lbCustomAction_Click;
                    }

                    linkButton.CausesValidation = false;
                    linkButton.PreRender += lb_PreRender;
                    Controls.Add( linkButton );

                    var icon = new HtmlGenericControl( "i" );
                    icon.Attributes.Add( "class", config.IconCssClass.IsNullOrWhiteSpace() ? "fa fa-cog fa-fw" : config.IconCssClass );

                    linkButton.Controls.Add( icon );
                    index++;
                }
            }

            // control for person merge
            _lbBusinessMerge = new LinkButton();
            Controls.Add( _lbBusinessMerge );
            _lbBusinessMerge.ID = "lbBusinessMerge";
            _lbBusinessMerge.CssClass = "btn btn-grid-action btn-merge btn-default btn-sm";
            _lbBusinessMerge.ToolTip = "Merge Business Records";
            _lbBusinessMerge.Click += lbPersonMerge_Click;
            _lbBusinessMerge.CausesValidation = false;
            _lbBusinessMerge.PreRender += lb_PreRender;
            Controls.Add( _lbBusinessMerge );
            HtmlGenericControl iBusinessMerge = new HtmlGenericControl( "i" );
            iBusinessMerge.Attributes.Add( "class", "fa fa-sign-in-alt fa-fw" );
            _lbBusinessMerge.Controls.Add( iBusinessMerge );

            // control for communicate
            _lbCommunicate = new LinkButton();
            Controls.Add( _lbCommunicate );
            _lbCommunicate.ID = "lbCommunicate";
            _lbCommunicate.CssClass = "btn btn-grid-action btn-communicate btn-default btn-sm";
            _lbCommunicate.ToolTip = "Communicate";
            _lbCommunicate.Click += lbCommunicate_Click;
            _lbCommunicate.CausesValidation = false;
            _lbCommunicate.PreRender += lb_PreRender;
            Controls.Add( _lbCommunicate );
            HtmlGenericControl iCommunicate = new HtmlGenericControl( "i" );
            iCommunicate.Attributes.Add( "class", "fa fa-comment fa-fw" );
            _lbCommunicate.Controls.Add( iCommunicate );

            // control for person merge
            _lbPersonMerge = new LinkButton();
            Controls.Add( _lbPersonMerge );
            _lbPersonMerge.ID = "lbPersonMerge";
            _lbPersonMerge.CssClass = "btn btn-grid-action btn-merge btn-default btn-sm";
            _lbPersonMerge.ToolTip = "Merge Person Records";
            _lbPersonMerge.Click += lbPersonMerge_Click;
            _lbPersonMerge.CausesValidation = false;
            _lbPersonMerge.PreRender += lb_PreRender;
            Controls.Add( _lbPersonMerge );
            HtmlGenericControl iPersonMerge = new HtmlGenericControl( "i" );
            iPersonMerge.Attributes.Add( "class", "fa fa-users fa-fw" );
            _lbPersonMerge.Controls.Add( iPersonMerge );

            // control for bulk update
            _lbBulkUpdate = new LinkButton();
            Controls.Add( _lbBulkUpdate );
            _lbBulkUpdate.ID = "lbBulkUpdate";
            _lbBulkUpdate.CssClass = "btn btn-grid-action btn-bulk-update btn-default btn-sm";
            _lbBulkUpdate.ToolTip = "Bulk Update";
            _lbBulkUpdate.Click += lbBulkUpdate_Click;
            _lbBulkUpdate.CausesValidation = false;
            _lbBulkUpdate.PreRender += lb_PreRender;
            Controls.Add( _lbBulkUpdate );
            HtmlGenericControl iBulkUpdate = new HtmlGenericControl( "i" );
            iBulkUpdate.Attributes.Add( "class", "fa fa-truck fa-fw" );
            _lbBulkUpdate.Controls.Add( iBulkUpdate );

            // control for default launch workflow
            _lbDefaultLaunchWorkflow = new LinkButton();
            _lbDefaultLaunchWorkflow.ID = "lbDefaultLaunchWorkflow";
            _lbDefaultLaunchWorkflow.CssClass = "btn-grid-action btn-launch-workflow btn btn-default btn-sm";
            _lbDefaultLaunchWorkflow.ToolTip = "Launch Workflow";
            _lbDefaultLaunchWorkflow.Click += lbLaunchWorkflow_Click;
            _lbDefaultLaunchWorkflow.CausesValidation = false;
            _lbDefaultLaunchWorkflow.PreRender += lb_PreRender;
            Controls.Add( _lbDefaultLaunchWorkflow );
            var iLaunchWorkflow = new HtmlGenericControl( "i" );
            iLaunchWorkflow.Attributes.Add( "class", "fa fa-cog fa-fw" );
            _lbDefaultLaunchWorkflow.Controls.Add( iLaunchWorkflow );

            // controls for excel export
            _aExcelExport = new HtmlGenericControl( "a" );
            Controls.Add( _aExcelExport );
            _aExcelExport.ID = "aExcelExport";
            _aExcelExport.Attributes.Add( "href", "#" );
            _aExcelExport.Attributes.Add( "class", "btn-excelexport" );
            _aExcelExport.InnerText = "Export To Excel";

            _lbExcelExport = new LinkButton();
            Controls.Add( _lbExcelExport );
            _lbExcelExport.ID = "lbExcelExport";
            _lbExcelExport.CssClass = "btn btn-grid-action btn-excelexport btn-default btn-sm";
            _lbExcelExport.ToolTip = "Export to Excel";
            _lbExcelExport.Click += lbExcelExport_Click;
            _lbExcelExport.CausesValidation = false;
            Controls.Add( _lbExcelExport );
            HtmlGenericControl iExcelExport = new HtmlGenericControl( "i" );
            iExcelExport.Attributes.Add( "class", "fa fa-table fa-fw" );
            _lbExcelExport.Controls.Add( iExcelExport );

            // control for merge template
            _lbMergeTemplate = new LinkButton();
            Controls.Add( _lbMergeTemplate );
            _lbMergeTemplate.ID = "lbMergeTemplate";
            _lbMergeTemplate.CssClass = "btn btn-grid-action btn-merge-template btn-default btn-sm";
            _lbMergeTemplate.ToolTip = "Merge Records into Merge Template";
            _lbMergeTemplate.Click += _lbMergeTemplate_Click;
            _lbMergeTemplate.CausesValidation = false;
            _lbMergeTemplate.PreRender += lb_PreRender;
            Controls.Add( _lbMergeTemplate );
            HtmlGenericControl iMergeTemplate = new HtmlGenericControl( "i" );
            iMergeTemplate.Attributes.Add( "class", "fa fa-files-o fa-fw" );
            _lbMergeTemplate.Controls.Add( iMergeTemplate );

            // controls for add
            _aAdd = new HtmlGenericControl( "a" );
            Controls.Add( _aAdd );
            _aAdd.ID = "aAdd";
            _aAdd.Attributes.Add( "href", "#" );
            _aAdd.Attributes.Add( "class", "btn btn-grid-action btn-add btn-default btn-sm" );

            _aAdd.InnerHtml = "<i class='fa fa-plus-circle fa-fw'></i>";

            _lbAdd = new LinkButton();
            Controls.Add( _lbAdd );
            _lbAdd.ID = "lbAdd";
            _lbAdd.CssClass = "btn btn-grid-action btn-add btn-default btn-sm";
            _lbAdd.ToolTip = "Alt+N";
            _lbAdd.Click += lbAdd_Click;
            _lbAdd.CausesValidation = false;
            _lbAdd.PreRender += lb_PreRender;
            _lbAdd.Attributes.Add("data-shortcut-key", "n");
            Controls.Add( _lbAdd );
            HtmlGenericControl iAdd = new HtmlGenericControl( "i" );
            iAdd.Attributes.Add( "class", "fa fa-plus-circle fa-fw" );
            _lbAdd.Controls.Add( iAdd );
        }

        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag( HtmlTextWriter writer )
        {
            // suppress the writing of a wrapper tag
            if ( this.TagKey != HtmlTextWriterTag.Unknown )
            {
                base.RenderBeginTag( writer );
            }
        }

        /// <summary>
        /// Renders the HTML closing tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderEndTag( HtmlTextWriter writer )
        {
            // suppress the writing of a wrapper tag
            if ( this.TagKey != HtmlTextWriterTag.Unknown )
            {
                base.RenderEndTag( writer );
            }
        }

        /// <summary>
        /// Renders the control but only renders custom actions in the once, in the actionFooterRow.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="renderAsMirrored">if set to <c>true</c> [render as mirrored].</param>
        public void RenderControl( HtmlTextWriter writer, bool renderAsMirrored )
        {
            if ( renderAsMirrored )
            {
                // Custom Actions usually don't work when mirrored, especially if they are Inputs such as DropDownLists, etc, so don't render them in the mirrored copy
                _pnlCustomActions.Visible = false;
                RenderControl( writer );
                _pnlCustomActions.Visible = true;
            }
            else
            {
                RenderControl( writer );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            var rockPage = Page as RockPage;

            _lbPersonMerge.Visible = !_parentGrid.IsBusiness && ShowMergePerson && _parentGrid.CanViewTargetPage( _parentGrid.PersonMergePageRoute );
            _lbBusinessMerge.Visible = _parentGrid.IsBusiness && ShowMergePerson && _parentGrid.CanViewTargetPage( _parentGrid.BusinessMergePageRoute );
            _lbBulkUpdate.Visible = ShowBulkUpdate && _parentGrid.CanViewTargetPage( _parentGrid.BulkUpdatePageRoute );

            var defaultLaunchWorkflowRoute = _parentGrid.DefaultLaunchWorkflowPageRoute;
            var canViewDefaultLaunchWorkflowRoute = _parentGrid.CanViewTargetPage( defaultLaunchWorkflowRoute );
            _lbDefaultLaunchWorkflow.Visible = ShowDefaultLaunchWorkflowButton && canViewDefaultLaunchWorkflowRoute;

            foreach ( var customLaunchWorkflowButton in _customActionButtons )
            {
                var customRoute = customLaunchWorkflowButton.CommandArgument.ToStringSafe();
                var hasCustomRoute = !customRoute.IsNullOrWhiteSpace();

                customLaunchWorkflowButton.Visible = ShowCustomActionButtons &&
                    ( hasCustomRoute ? _parentGrid.CanViewTargetPage( customRoute ) : canViewDefaultLaunchWorkflowRoute );
            }

            if ( ShowCommunicate )
            {
                string url = _parentGrid.CommunicationPageRoute;
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    var pageRef = rockPage.Site.CommunicationPageReference;
                    if ( pageRef.PageId > 0 )
                    {
                        pageRef.Parameters.AddOrReplace( "CommunicationId", "0" );
                        url = pageRef.BuildUrl();
                    }
                    else
                    {
                        url = "~/Communication/{0}";
                    }
                }

                _lbCommunicate.Visible = _parentGrid.CanViewTargetPage( url );
            }
            else
            {
                _lbCommunicate.Visible = false;
            }

            _aAdd.Visible = ShowAdd && !String.IsNullOrWhiteSpace( ClientAddScript );
            _lbAdd.Visible = ShowAdd && String.IsNullOrWhiteSpace( ClientAddScript );

            _aExcelExport.Visible = ShowExcelExport && !String.IsNullOrWhiteSpace( ClientExcelExportScript );
            _lbExcelExport.Visible = ShowExcelExport && String.IsNullOrWhiteSpace( ClientExcelExportScript );

            _lbMergeTemplate.Visible = ShowMergeTemplate && _parentGrid.CanViewTargetPage( _parentGrid.MergeTemplatePageRoute );

            base.RenderControl( writer );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the PreRender event of the linkbutton controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lb_PreRender( object sender, EventArgs e )
        {
            var lb = sender as LinkButton;
            if ( lb != null )
            {
                lb.Enabled = _parentGrid.Enabled;
                if ( lb.Enabled )
                {
                    lb.Attributes.Remove( "disabled" );
                }
                else
                {
                    lb.Attributes["disabled"] = "disabled";
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbPersonMerge_Click( object sender, EventArgs e )
        {
            if ( PersonMergeClick != null )
            {
                PersonMergeClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbBulkUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbBulkUpdate_Click( object sender, EventArgs e )
        {
            if ( BulkUpdateClick != null )
            {
                BulkUpdateClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbBulkUpdate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbLaunchWorkflow_Click( object sender, EventArgs e )
        {
            WorkflowOrCustomActionClick?.Invoke( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the lbCustomAction control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbCustomAction_Click( object sender, EventArgs e )
        {
            WorkflowOrCustomActionClick?.Invoke( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the lbCommunicate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbCommunicate_Click( object sender, EventArgs e )
        {
            CommunicateClick?.Invoke( sender, e );
        }

        /// <summary>
        /// Handles the Click event of the lbAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbAdd_Click( object sender, EventArgs e )
        {
            if ( AddClick != null )
            {
                AddClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbExcelExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbExcelExport_Click( object sender, EventArgs e )
        {
            if ( ExcelExportClick != null )
            {
                ExcelExportClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the _lbMergeTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _lbMergeTemplate_Click( object sender, EventArgs e )
        {
            if ( MergeTemplateClick != null )
            {
                MergeTemplateClick( sender, e );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the custom action controls.
        /// </summary>
        public void ClearCustomActionControls()
        {
            _customActions.Clear();
        }

        /// <summary>
        /// Adds the custom action control.
        /// </summary>
        /// <param name="control">The control.</param>
        public void AddCustomActionControl( Control control )
        {
            _customActions.Add( control );
            RecreateChildControls();
        }

        /// <summary>
        /// Adds the custom action Block button.
        /// </summary>
        /// <param name="customConfig">The custom button control.</param>
        public void AddCustomActionBlockButton( CustomActionConfigEvent customConfig )
        {
            _customActionConfigEvents.Add( customConfig );
            RecreateChildControls();
        }

        /// <summary>
        /// Allows a grid action to invoke the same EventHandler as the Communicate click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void InvokeCommunicateClick( object sender, EventArgs e )
        {
            CommunicateClick?.Invoke( sender, e );
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when Person merge action is clicked.
        /// </summary>
        public event EventHandler PersonMergeClick;

        /// <summary>
        /// Occurs when bulk update action is clicked.
        /// </summary>
        public event EventHandler BulkUpdateClick;

        /// <summary>
        /// Occurs when [workflow or custom action click].
        /// </summary>
        public event EventHandler WorkflowOrCustomActionClick;

        /// <summary>
        /// Occurs when communicate action is clicked.
        /// </summary>
        public event EventHandler CommunicateClick;

        /// <summary>
        /// Occurs when add action is clicked.
        /// </summary>
        public event EventHandler AddClick;

        /// <summary>
        /// Occurs when add action is clicked.
        /// </summary>
        public event EventHandler ExcelExportClick;

        /// <summary>
        /// Occurs when the Merge Template action is clicked
        /// </summary>
        public event EventHandler MergeTemplateClick;

        #endregion
    }
}