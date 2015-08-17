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
        }

        #endregion

        #region Fields

        private Grid _parentGrid;

        #endregion

        #region Controls

        private List<Control> _customActions;
        private LinkButton _lbPersonMerge;
        private LinkButton _lbBulkUpdate;
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
                bool hasPersonIdField = !string.IsNullOrWhiteSpace(_parentGrid.PersonIdField);
                
                return ViewState["ShowCommunicate"] as bool? ?? hasPersonIdField; 
            }
            
            set 
            { 
                ViewState["ShowCommunicate"] = value; 
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
        /// Recreates the child controls in a control derived from <see cref="T:System.Web.UI.WebControls.CompositeControl"/>.
        /// </summary>
        //protected override void RecreateChildControls()
        //{
        //    EnsureChildControls();
        //}

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            if ( _customActions != null )
            {
                foreach ( Control control in _customActions )
                {
                    Controls.Add( control );
                }
            }

            // controls for add
            _aAdd = new HtmlGenericControl( "a" );
            Controls.Add( _aAdd );
            _aAdd.ID = "aAdd";
            _aAdd.Attributes.Add( "href", "#" );
            _aAdd.Attributes.Add( "class", "btn-add btn btn-default btn-sm" );
            _aAdd.InnerText = "Add";

            _lbAdd = new LinkButton();
            Controls.Add( _lbAdd );
            _lbAdd.ID = "lbAdd";
            _lbAdd.CssClass = "btn-add btn btn-default btn-sm";
            _lbAdd.ToolTip = "Add";
            _lbAdd.Click += lbAdd_Click;
            _lbAdd.CausesValidation = false;
            _lbAdd.PreRender += lb_PreRender;
            _lbAdd.AccessKey = "n";
            Controls.Add( _lbAdd );
            HtmlGenericControl iAdd = new HtmlGenericControl( "i" );
            iAdd.Attributes.Add( "class", "fa fa-plus-circle" );
            _lbAdd.Controls.Add( iAdd );

            // control for communicate
            _lbCommunicate = new LinkButton();
            Controls.Add( _lbCommunicate );
            _lbCommunicate.ID = "lbCommunicate";
            _lbCommunicate.CssClass = "btn-communicate btn btn-default btn-sm";
            _lbCommunicate.ToolTip = "Communicate";
            _lbCommunicate.Click += lbCommunicate_Click;
            _lbCommunicate.CausesValidation = false;
            _lbCommunicate.PreRender += lb_PreRender;
            Controls.Add( _lbCommunicate );
            HtmlGenericControl iCommunicate = new HtmlGenericControl( "i" );
            iCommunicate.Attributes.Add( "class", "fa fa-comment" );
            _lbCommunicate.Controls.Add( iCommunicate );

            // control for person merge
            _lbPersonMerge = new LinkButton();
            Controls.Add( _lbPersonMerge );
            _lbPersonMerge.ID = "lbPersonMerge";
            _lbPersonMerge.CssClass = "btn-merge btn btn-default btn-sm";
            _lbPersonMerge.ToolTip = "Merge Person Records";
            _lbPersonMerge.Click += lbPersonMerge_Click;
            _lbPersonMerge.CausesValidation = false;
            _lbPersonMerge.PreRender += lb_PreRender;
            Controls.Add( _lbPersonMerge );
            HtmlGenericControl iPersonMerge = new HtmlGenericControl( "i" );
            iPersonMerge.Attributes.Add( "class", "fa fa-users" );
            _lbPersonMerge.Controls.Add( iPersonMerge );

            // control for bulk update
            _lbBulkUpdate = new LinkButton();
            Controls.Add( _lbBulkUpdate );
            _lbBulkUpdate.ID = "lbBulkUpdate";
            _lbBulkUpdate.CssClass = "btn-bulk-update btn btn-default btn-sm";
            _lbBulkUpdate.ToolTip = "Bulk Update";
            _lbBulkUpdate.Click += lbBulkUpdate_Click;
            _lbBulkUpdate.CausesValidation = false;
            _lbBulkUpdate.PreRender += lb_PreRender;
            Controls.Add( _lbBulkUpdate );
            HtmlGenericControl iBulkUpdate = new HtmlGenericControl( "i" );
            iBulkUpdate.Attributes.Add( "class", "fa fa-truck" );
            _lbBulkUpdate.Controls.Add( iBulkUpdate );
            
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
            _lbExcelExport.CssClass = "btn-excelexport btn btn-default btn-sm";
            _lbExcelExport.ToolTip = "Export to Excel";
            _lbExcelExport.Click += lbExcelExport_Click;
            _lbExcelExport.CausesValidation = false;
            Controls.Add( _lbExcelExport );
            HtmlGenericControl iExcelExport = new HtmlGenericControl( "i" );
            iExcelExport.Attributes.Add( "class", "fa fa-table" );
            _lbExcelExport.Controls.Add( iExcelExport );

            // control for merge template
            _lbMergeTemplate = new LinkButton();
            Controls.Add( _lbMergeTemplate );
            _lbMergeTemplate.ID = "lbMergeTemplate";
            _lbMergeTemplate.CssClass = "btn-merge-template btn btn-default btn-sm";
            _lbMergeTemplate.ToolTip = "Merge Records into Merge Template";
            _lbMergeTemplate.Click += _lbMergeTemplate_Click;
            _lbMergeTemplate.CausesValidation = false;
            _lbMergeTemplate.PreRender += lb_PreRender;
            Controls.Add( _lbMergeTemplate );
            HtmlGenericControl iMergeTemplate = new HtmlGenericControl( "i" );
            iMergeTemplate.Attributes.Add( "class", "fa fa-files-o" );
            _lbMergeTemplate.Controls.Add( iMergeTemplate );
        }

        /// <summary>
        /// Renders the HTML opening tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderBeginTag( HtmlTextWriter writer )
        {
            // suppress the writing of a wrapper tag
            if (this.TagKey != HtmlTextWriterTag.Unknown)
            {
                base.RenderBeginTag(writer);
            }
        }

        /// <summary>
        /// Renders the HTML closing tag of the control to the specified writer. This method is used primarily by control developers.
        /// </summary>
        /// <param name="writer">A <see cref="T:System.Web.UI.HtmlTextWriter"/> that represents the output stream to render HTML content on the client.</param>
        public override void RenderEndTag(HtmlTextWriter writer)
        {
            // suppress the writing of a wrapper tag
            if (this.TagKey != HtmlTextWriterTag.Unknown)
            {
                base.RenderEndTag(writer);
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            bool personGrid = !string.IsNullOrWhiteSpace( _parentGrid.PersonIdField );
            _lbPersonMerge.Visible = personGrid;
            _lbBulkUpdate.Visible = personGrid;
            _lbCommunicate.Visible = ShowCommunicate;

            _aAdd.Visible = ShowAdd && !String.IsNullOrWhiteSpace( ClientAddScript );
            _lbAdd.Visible = ShowAdd && String.IsNullOrWhiteSpace( ClientAddScript );

            _aExcelExport.Visible = ShowExcelExport && !String.IsNullOrWhiteSpace( ClientExcelExportScript );
            _lbExcelExport.Visible = ShowExcelExport && String.IsNullOrWhiteSpace( ClientExcelExportScript );

            _lbMergeTemplate.Visible = ShowMergeTemplate;

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
        /// Handles the Click event of the lbAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void lbCommunicate_Click( object sender, EventArgs e )
        {
            if ( CommunicateClick != null )
            {
                CommunicateClick( sender, e );
            }
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
            if (MergeTemplateClick != null)
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
        public void AddCustomActionControl( Control control)
        {
            _customActions.Add( control );
            RecreateChildControls();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Occurs when Person merge action is clicked.
        /// </summary>
        [Obsolete("Use PersonMergeClick instead")]
        public event EventHandler MergeClick
        {
            add
            {
                PersonMergeClick += value;
            }

            remove
            {
                PersonMergeClick -= value;
            }
        }
               
        /// <summary>
        /// Occurs when Person merge action is clicked.
        /// </summary>
        public event EventHandler PersonMergeClick;

        /// <summary>
        /// Occurs when bulk update action is clicked.
        /// </summary>
        public event EventHandler BulkUpdateClick;

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