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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkflowPicker : CompositeControl, IRockControl
    {
        #region IRockControl implementation (Custom implementation)

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
            get 
            {
                EnsureChildControls();
                return _ddlWorkflow.Required; 
            }
            set 
            {
                EnsureChildControls();
                _ddlWorkflow.Required = value; 
            }
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
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
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

        private RockDropDownList _ddlWorkflowType;
        private RockDropDownList _ddlWorkflow;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the workflow type id.
        /// </summary>
        /// <value>
        /// The workflow type id.
        /// </value>
        public int? WorkflowTypeId
        {
            get
            {
                return ViewState["WorkflowTypeId"] as int?;
            }

            set
            {
                ViewState["WorkflowTypeId"] = value;
                if ( value.HasValue )
                {
                    LoadWorkflows( value.Value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the workflow id.
        /// </summary>
        /// <value>
        /// The workflow id.
        /// </value>
        public int? WorkflowId
        {
            get
            {
                EnsureChildControls();
                int workflowId = int.MinValue;
                if ( int.TryParse( _ddlWorkflow.SelectedValue, out workflowId ) && workflowId > 0 )
                {
                    return workflowId;
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                int workflowId = value.HasValue ? value.Value : 0;
                if ( _ddlWorkflow.SelectedValue != workflowId.ToString() )
                {
                    if ( !WorkflowTypeId.HasValue )
                    {
                        var workflow = new Rock.Model.WorkflowService( new RockContext() ).Get( workflowId );
                        if ( workflow != null &&
                            _ddlWorkflowType.SelectedValue != workflow.WorkflowTypeId.ToString() )
                        {
                            _ddlWorkflowType.SelectedValue = workflow.WorkflowTypeId.ToString();

                            LoadWorkflows( workflow.WorkflowTypeId );
                        }
                    }

                    _ddlWorkflow.SetValue( workflowId.ToString() );
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowPicker"/> class.
        /// </summary>
        public WorkflowPicker()
            : base()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _ddlWorkflowType = new RockDropDownList();
            _ddlWorkflowType.ID = this.ID + "_ddlWorkflowType";
            _ddlWorkflowType.AutoPostBack = true;
            _ddlWorkflowType.SelectedIndexChanged += _ddlWorkflowType_SelectedIndexChanged;
            Controls.Add( _ddlWorkflowType );

            _ddlWorkflow = new RockDropDownList();
            _ddlWorkflow.ID = this.ID + "_ddlWorkflow";
            _ddlWorkflow.Label = "Workflow";
            Controls.Add( _ddlWorkflow );

            LoadWorkflowTypes();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlWorkflowType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _ddlWorkflowType_SelectedIndexChanged( object sender, EventArgs e )
        {
            int workflowTypeId = _ddlWorkflowType.SelectedValue.AsInteger();
            LoadWorkflows( workflowTypeId );
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
            if ( !WorkflowTypeId.HasValue )
            {
                _ddlWorkflowType.RenderControl( writer );
            }
            _ddlWorkflow.RenderControl( writer );
        }

        /// <summary>
        /// Loads the workflow types.
        /// </summary>
        private void LoadWorkflowTypes()
        {
            _ddlWorkflowType.Items.Clear();
            
            if ( !Required )
            {
                _ddlWorkflowType.Items.Add( new ListItem( string.Empty, Rock.Constants.None.IdValue ) );
            }

            using ( var rockContext = new RockContext() )
            {
                var workflowTypeService = new Rock.Model.WorkflowTypeService( rockContext );

                var workflowTypes = workflowTypeService.Queryable().AsNoTracking()
                    .Where( t => 
                        t.Category != null &&
                        t.IsActive.HasValue &&
                        t.IsActive.Value)
                    .OrderBy( t => t.Category.Name)
                    .ThenBy( t => t.Name )
                    .Select(a => new { a.Id, CategoryName = a.Category.Name, a.Name} )
                    .AsNoTracking()
                    .ToList();
                foreach ( var t in workflowTypes )
                {
                    _ddlWorkflowType.Items.Add( new ListItem( string.Format( "{0}: {1}", t.CategoryName, t.Name), t.Id.ToString().ToUpper() ) );
                }
            }
        }

        /// <summary>
        /// Loads the workflows.
        /// </summary>
        /// <param name="workflowTypeId">The workflow type unique identifier.</param>
        private void LoadWorkflows( int? workflowTypeId )
        {
            int? currentWorkflowId = this.WorkflowId;
            _ddlWorkflow.SelectedValue = null;
            _ddlWorkflow.Items.Clear();
            if ( workflowTypeId.HasValue )
            {
                if ( !Required )
                {
                    _ddlWorkflow.Items.Add( new ListItem( string.Empty, Rock.Constants.None.IdValue ) );
                }

                var workflowService = new Rock.Model.WorkflowService( new RockContext() );
                var workflows = workflowService.Queryable()
                    .Where( w => 
                        w.WorkflowTypeId == workflowTypeId.Value &&
                        w.ActivatedDateTime.HasValue && 
                        !w.CompletedDateTime.HasValue )
                    .OrderBy( w => w.Name )
                    .Select(a => new
                    {
                        a.Id,
                        a.Name
                    } )
                    .ToList();

                foreach ( var w in workflows )
                {
                    var workflowItem = new ListItem( w.Name, w.Id.ToString().ToUpper() );
                    workflowItem.Selected = w.Id == currentWorkflowId;
                    _ddlWorkflow.Items.Add( workflowItem );
                }
            }
        }
    }
}