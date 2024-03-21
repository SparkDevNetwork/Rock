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
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block to display the benevolence types.
    /// </summary>
    [DisplayName( "Benevolence Type Detail" )]
    [Category( "Finance" )]
    [Description( "Block to display the benevolence type detail." )]

    [AttributeField(
        "Benevolence Type Attributes",
        Key = AttributeKey.BenevolenceTypeAttributes,
        EntityTypeGuid = Rock.SystemGuid.EntityType.BENEVOLENCE_TYPE,
        Description = "The attributes that should be displayed / edited for benevolence types.",
        IsRequired = false,
        AllowMultiple = true,
        Order = 1 )]

    [Rock.SystemGuid.BlockTypeGuid( "C96479B6-E309-4B1A-B024-1F1276122A13" )]
    public partial class BenevolenceTypeDetail : Rock.Web.UI.RockBlock
    {
        #region Variables

        private int benevolenceTypeIdPageParameter = 0;
        private const string WORKFLOW_SESSIONSTATE_KEY = "BENEVOLENCEWORKFLOWSTATE";

        #endregion Variables

        #region Attribute Key

        private static class AttributeKey
        {
            public const string BenevolenceTypeAttributes = "BenevolenceTypeAttributes";
        }

        #endregion Attribute Key

        #region Properties        
        /// <summary>
        /// Gets or sets the workflow state model.
        /// </summary>
        /// <value>The workflow state model.</value>
        public List<BenevolenceWorkflow> WorkflowStateModel { get; set; }
        #endregion Properties

        #region Control Methods
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            benevolenceTypeIdPageParameter = PageParameter( "BenevolenceTypeId" ).AsInteger();
            InitializeWorkflowGrid( benevolenceTypeIdPageParameter );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                BindWorkflowGrid();
                ShowDetail( benevolenceTypeIdPageParameter );
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var json = ViewState[WORKFLOW_SESSIONSTATE_KEY] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                WorkflowStateModel = new List<BenevolenceWorkflow>();
            }
            else
            {
                WorkflowStateModel = JsonConvert.DeserializeObject<List<BenevolenceWorkflow>>( json );
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState[WORKFLOW_SESSIONSTATE_KEY] = JsonConvert.SerializeObject( WorkflowStateModel, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Initializes the workflow grid.
        /// </summary>
        /// <param name="benevolenceTypeId">The benevolence type identifier.</param>
        private void InitializeWorkflowGrid( int benevolenceTypeId )
        {
            BenevolenceType benevolenceType = null;
            var rockContext = new RockContext();
            if ( !benevolenceTypeId.Equals( 0 ) )
            {
                benevolenceType = new BenevolenceTypeService( rockContext ).Get( benevolenceTypeId );
            }

            if ( benevolenceType == null )
            {
                benevolenceType = new BenevolenceType { Id = 0 };
            }

            WorkflowStateModel = new List<BenevolenceWorkflow>();
            foreach ( var benevolenceWorkflow in benevolenceType.BenevolenceWorkflows )
            {
                var modelState = new BenevolenceWorkflow
                {
                    Id = benevolenceWorkflow.Id,
                    Guid = benevolenceWorkflow.Guid,
                    BenevolenceTypeId = benevolenceTypeId,
                    QualifierValue = benevolenceWorkflow.QualifierValue,
                    TriggerType = benevolenceWorkflow.TriggerType,
                    WorkflowTypeId = benevolenceWorkflow.WorkflowTypeId,
                    WorkflowType = benevolenceWorkflow.WorkflowType
                };

                WorkflowStateModel.Add( modelState );
            }

            gBenevolenceTypeWorkflows.DataKeyNames = new string[] { "Guid" };
            gBenevolenceTypeWorkflows.Actions.ShowAdd = true;
            gBenevolenceTypeWorkflows.Actions.AddClick += gBenevolenceTypeWorkflows_Add;
            gBenevolenceTypeWorkflows.GridRebind += gBenevolenceTypeWorkflows_GridRebind;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="benevolenceTypeId">The benevolence type identifier.</param>
        private void ShowDetail( int benevolenceTypeId )
        {
            pnlDetails.Visible = true;
            var rockContext = new RockContext();
            BenevolenceType benevolenceType = null;

            if ( benevolenceTypeId != 0 )
            {
                benevolenceType = new BenevolenceTypeService( new RockContext() ).Get( benevolenceTypeIdPageParameter );
                lActionTitle.Text = ActionTitle.Edit( BenevolenceType.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( benevolenceType, ResolveRockUrl( "~" ) );
            }

            if ( benevolenceType == null )
            {
                benevolenceType = new BenevolenceType
                {
                    Id = 0,
                    IsActive = true,
                    ShowFinancialResults = true
                };

                lActionTitle.Text = ActionTitle.Add( BenevolenceType.FriendlyTypeName ).FormatAsHtmlTitle();

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            tbName.Text = benevolenceType.Name;
            cbIsActive.Checked = benevolenceType.IsActive;
            numberBoxMaxDocuments.IntegerValue = benevolenceType.AdditionalSettingsJson?.FromJsonOrNull<BenevolenceType.AdditionalSettings>().MaximumNumberOfDocuments;
            cbShowFinancialResults.Checked = benevolenceType.ShowFinancialResults;
            tbDescription.Text = benevolenceType.Description;
            ceLavaTemplate.Text = benevolenceType.RequestLavaTemplate;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;
            nbEditModeMessage.Text = string.Empty;

            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( BenevolenceType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( BenevolenceType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            ceLavaTemplate.ReadOnly = readOnly;
            cbIsActive.Enabled = !readOnly;

            SetHighlightLabelVisibility( benevolenceType, readOnly );

            btnSave.Visible = !readOnly;
        }

        /// <summary>
        /// Sets the highlight label visibility.
        /// </summary>
        /// <param name="benevolenceType">The benevolence type.</param>
        private void SetHighlightLabelVisibility( BenevolenceType benevolenceType, bool readOnly )
        {
            if ( readOnly )
            {
                // if we are just showing readonly detail of the group, we don't have to worry about the highlight labels changing while editing on the client
                hlInactive.Visible = !benevolenceType.IsActive;
            }
            else
            {
                // in edit mode, the labels will have javascript handle if/when they are shown
                hlInactive.Visible = true;
            }

            if ( benevolenceType.IsActive )
            {
                hlInactive.Style[HtmlTextWriterStyle.Display] = "none";
            }
        }
        #endregion Control Methods

        #region Events        
        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            Page.Validate();
            BenevolenceType benevolenceType = null;

            using ( var rockContext = new RockContext() )
            {
                var benevolenceService = new BenevolenceTypeService( rockContext );
                var benevolenceWorkflowService = new BenevolenceWorkflowService( rockContext );

                var benevolenceTypeId = benevolenceTypeIdPageParameter;

                if ( benevolenceTypeId != 0 )
                {
                    benevolenceType = benevolenceService.Get( benevolenceTypeId );
                }

                if ( benevolenceType == null )
                {
                    // Check for existing
                    var existingBenevolence = benevolenceService.Queryable()
                        .Where( d => d.Name == tbName.Text )
                        .FirstOrDefault();

                    if ( existingBenevolence != null )
                    {
                        nbDuplicateDevice.Text = $"A benevolence type already exists with the name '{existingBenevolence.Name}'. Please use a different benevolence type name.";
                        nbDuplicateDevice.Visible = true;
                    }
                    else
                    {
                        benevolenceType = new BenevolenceType();
                        benevolenceService.Add( benevolenceType );
                    }
                }

                if ( benevolenceType != null )
                {
                    benevolenceType.Name = tbName.Text;
                    benevolenceType.IsActive = cbIsActive.Checked;
                    benevolenceType.ShowFinancialResults = cbShowFinancialResults.Checked;
                    benevolenceType.Description = tbDescription.Text;
                    benevolenceType.RequestLavaTemplate = ceLavaTemplate.Text;

                    var additionalSettings = benevolenceType.AdditionalSettingsJson?.FromJsonOrNull<BenevolenceType.AdditionalSettings>() ?? new BenevolenceType.AdditionalSettings();
                    additionalSettings.MaximumNumberOfDocuments = numberBoxMaxDocuments.IntegerValue;
                    benevolenceType.AdditionalSettingsJson = additionalSettings.ToJson();

                    // remove any workflows that were removed in the UI
                    var uiWorkflows = WorkflowStateModel.Select( l => l.Guid );

                    foreach ( var benevolenceWorkflow in benevolenceType.BenevolenceWorkflows.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList() )
                    {
                        benevolenceType.BenevolenceWorkflows.Remove( benevolenceWorkflow );
                        benevolenceWorkflowService.Delete( benevolenceWorkflow );
                    }

                    // Add or Update workflows from the UI
                    foreach ( var workflowStateModel in WorkflowStateModel )
                    {
                        BenevolenceWorkflow benevolenceWorkflow = benevolenceType.BenevolenceWorkflows
                            .Where( b => !workflowStateModel.Guid.Equals( Guid.Empty ) && b.Guid == workflowStateModel.Guid ).FirstOrDefault();

                        if ( benevolenceWorkflow == null )
                        {
                            benevolenceWorkflow = new BenevolenceWorkflow
                            {
                                BenevolenceTypeId = benevolenceTypeId
                            };
                            benevolenceType.BenevolenceWorkflows.Add( benevolenceWorkflow );
                        }

                        // Set the properties on the state model
                        benevolenceWorkflow.CopyPropertiesFrom( workflowStateModel );
                    }

                    if ( !benevolenceType.IsValid )
                    {
                        // Controls will render the error messages
                        return;
                    }

                    rockContext.WrapTransaction( () =>
                    {
                        rockContext.SaveChanges();
                    } );

                    BenevolenceWorkflowService.RemoveCachedTriggers();
                    NavigateToParentPage();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnHideDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnHideDialog_Click( object sender, EventArgs e )
        {
            HideDialog();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }
        #endregion Events

        #region BenevolenceType Workflow Grid/Dialog Events

        /// <summary>
        /// Handles the SaveClick event of the mdWorkflowDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdWorkflowDetails_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var triggerType = ddlTriggerType.SelectedValueAsEnum<BenevolenceWorkflowTriggerType>();
            var workflowQualifier = $"|{ddlPrimaryQualifier.SelectedValue}|{ddlSecondaryQualifier.SelectedValue}|";

            int workflowTypeId = wpWorkflowType.SelectedValueAsId().Value;
            var workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeId );

            var findWorkFlow = new BenevolenceWorkflowService( rockContext ).Queryable()
                .FirstOrDefault( w => w.BenevolenceTypeId == benevolenceTypeIdPageParameter
                    && w.TriggerType == triggerType
                    && w.WorkflowTypeId == workflowTypeId );

            var existingWorkflow = findWorkFlow != null && findWorkFlow.QualifierValue.Md5Hash() == workflowQualifier.Md5Hash();

            if ( existingWorkflow )
            {
                DisplayError( "That workflow record already exist for this benevolence type." );
                HideDialog();
                return;
            }

            var guid = hfWorkflowGuid.Value.AsGuid();
            var workflowState = WorkflowStateModel.Where( w => w.Guid.Equals( guid ) && !guid.Equals( Guid.Empty ) ).FirstOrDefault();
            if ( workflowState == null )
            {
                workflowState = new BenevolenceWorkflow
                {
                    Guid = guid
                };

                WorkflowStateModel.Add( workflowState );
            }

            if ( workflowType != null )
            {
                workflowState.WorkflowTypeId = workflowType.Id;
                workflowState.WorkflowType = workflowType;
            }

            workflowState.TriggerType = triggerType;
            workflowState.QualifierValue = workflowQualifier;
            BindWorkflowGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the Delete event of the gBenevolenceTypeWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBenevolenceTypeWorkflows_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            var workflowTypeStateObj = WorkflowStateModel.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( workflowTypeStateObj != null )
            {
                WorkflowStateModel.Remove( workflowTypeStateObj );
            }

            BindWorkflowGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gBenevolenceTypeWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBenevolenceTypeWorkflows_GridRebind( object sender, EventArgs e )
        {
            BindWorkflowGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gBenevolenceTypeWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gBenevolenceTypeWorkflows_Edit( object sender, RowEventArgs e )
        {
            var benevolenceWorkflowGuid = ( Guid ) e.RowKeyValue;
            gBenevolenceTypeWorkflows_ShowEdit( benevolenceWorkflowGuid );
        }

        /// <summary>
        /// Shows the benevolence workflow details edit dialog.
        /// </summary>
        /// <param name="benevolenceWorkflowGuid">The benevolence workflow unique identifier.</param>
        protected void gBenevolenceTypeWorkflows_ShowEdit( Guid benevolenceWorkflowGuid )
        {
            var workflowState = WorkflowStateModel.FirstOrDefault( l => l.Guid.Equals( benevolenceWorkflowGuid ) );
            if ( workflowState != null )
            {
                wpWorkflowType.SetValue( workflowState.WorkflowTypeId );
                ddlTriggerType.SelectedValue = workflowState.TriggerType.ConvertToInt().ToString();
            }

            hfWorkflowGuid.Value = benevolenceWorkflowGuid.ToString();
            UpdateTriggerQualifiers();
            ShowDialog( "WorkflowDetails", true );
        }

        /// <summary>
        /// Handles the Add event of the gBenevolenceTypeWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gBenevolenceTypeWorkflows_Add( object sender, EventArgs e )
        {
            gBenevolenceTypeWorkflows_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gBenevolenceTypeWorkflows control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gBenevolenceTypeWorkflows_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var benevolenceTypeId = e.Row.DataItem.GetPropertyValue( "BenevolenceTypeId" ) as int?;
                if ( benevolenceTypeId != null )
                {
                    e.Row.AddCssClass( "inactive" );

                    var deleteField = gBenevolenceTypeWorkflows.Columns.OfType<DeleteField>().First();
                    var cell = ( e.Row.Cells[gBenevolenceTypeWorkflows.GetColumnIndex( deleteField )] as DataControlFieldCell ).Controls[0];
                    if ( cell != null )
                    {
                        cell.Visible = true;
                    }

                    var editField = gBenevolenceTypeWorkflows.Columns.OfType<EditField>().First();
                    cell = ( e.Row.Cells[gBenevolenceTypeWorkflows.GetColumnIndex( editField )] as DataControlFieldCell ).Controls[0];
                    if ( cell != null )
                    {
                        cell.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlTriggerType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlTriggerType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateTriggerQualifiers();
        }

        /// <summary>
        /// Updates the trigger qualifiers.
        /// </summary>
        private void UpdateTriggerQualifiers()
        {
            RockContext rockContext = new RockContext();
            string[] qualifierValues = new string[2];

            var workflowTypeStateObj = WorkflowStateModel.FirstOrDefault( l => l.Guid.Equals( hfWorkflowGuid.Value.AsGuid() ) );
            var benevolenceWorkflowTriggerType = ddlTriggerType.SelectedValueAsEnum<BenevolenceWorkflowTriggerType>();
            int benevolenceTypeId = PageParameter( "BenevolenceTypeId" ).AsInteger();
            var benevolenceType = new BenevolenceTypeService( rockContext ).Get( benevolenceTypeId );
            bool isPrimaryQualifierVisible = false;
            bool isSecondaryQualifierVisible = false;
            switch ( benevolenceWorkflowTriggerType )
            {
                case BenevolenceWorkflowTriggerType.RequestStarted:
                case BenevolenceWorkflowTriggerType.CaseworkerAssigned:
                case BenevolenceWorkflowTriggerType.Manual:
                    {
                        ddlPrimaryQualifier.Visible = isPrimaryQualifierVisible = false;
                        ddlPrimaryQualifier.Items.Clear();
                        ddlSecondaryQualifier.Visible = isSecondaryQualifierVisible = false;
                        ddlSecondaryQualifier.Items.Clear();
                        break;
                    }

                case BenevolenceWorkflowTriggerType.StatusChanged:
                    {
                        var benevolenceStatusTypes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.BENEVOLENCE_REQUEST_STATUS.AsGuid() ).DefinedValues;

                        ddlPrimaryQualifier.Label = "From";
                        ddlPrimaryQualifier.Visible = isPrimaryQualifierVisible = true;
                        ddlPrimaryQualifier.Items.Clear();
                        ddlPrimaryQualifier.Items.Add( new ListItem( string.Empty, string.Empty ) );
                        foreach ( var status in benevolenceStatusTypes )
                        {
                            ddlPrimaryQualifier.Items.Add( new ListItem( status.Value, status.Id.ToString().ToUpper() ) );
                        }

                        ddlSecondaryQualifier.Label = "To";
                        ddlSecondaryQualifier.Visible = isSecondaryQualifierVisible = true;
                        ddlSecondaryQualifier.Items.Clear();
                        ddlSecondaryQualifier.Items.Add( new ListItem( string.Empty, string.Empty ) );
                        foreach ( var status in benevolenceStatusTypes )
                        {
                            ddlSecondaryQualifier.Items.Add( new ListItem( status.Value, status.Id.ToString().ToUpper() ) );
                        }

                        break;
                    }
            }

            if ( workflowTypeStateObj != null )
            {
                if ( workflowTypeStateObj.TriggerType == ddlTriggerType.SelectedValueAsEnum<BenevolenceWorkflowTriggerType>() )
                {
                    qualifierValues = workflowTypeStateObj.QualifierValue.SplitDelimitedValues( "|" );
                    /*
                        Visible property of ddlPrimaryQualifier and ddlSecondaryQualifier don't reflect the new assigned value till the request complete.
                        That is the reason isPrimaryQualifierVisible & isSecondaryQualifierVisible are introduced to potentially fix the issue raised in #2029
                        https://github.com/SparkDevNetwork/Rock/issues/2029
                    */
                    if ( isPrimaryQualifierVisible && qualifierValues.Length > 1 )
                    {
                        ddlPrimaryQualifier.SelectedValue = qualifierValues[1];
                    }

                    if ( isSecondaryQualifierVisible && qualifierValues.Length > 2 )
                    {
                        ddlSecondaryQualifier.SelectedValue = qualifierValues[2];
                    }
                }
            }
        }

        /// <summary>
        /// Binds the workflow grid.
        /// </summary>
        private void BindWorkflowGrid()
        {
            gBenevolenceTypeWorkflows.DataSource = WorkflowStateModel.Select( w => new
            {
                w.Id,
                w.Guid,
                WorkflowTypeName = w.WorkflowType.Name,
                Inherited = w.BenevolenceTypeId > 0,
                WorkflowType = w.BenevolenceTypeId > 0 ? w.WorkflowType.Name + " <span class='label label-default'>Inherited</span>" : w.WorkflowType.Name,
                Trigger = w.TriggerType.ConvertToString(),
                w.BenevolenceTypeId
            } )
       .OrderByDescending( w => w.Inherited )
       .ThenBy( w => w.WorkflowTypeName )
       .ToList();
            gBenevolenceTypeWorkflows.DataBind();
        }

        #endregion BenevolenceType Workflow Grid/Dialog Events

        #region Methods
        /// <summary>
        /// Displays the error.
        /// </summary>
        /// <param name="message">The message.</param>
        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            var lit = new LiteralControl( message ) { ID = "lError" };
            pnlMessage.Controls.Add( lit );
            pnlMessage.CssClass = "alert alert-danger block-message error";
            pnlMessage.Visible = true;
        }

        /// <summary>
        /// Hides the error.
        /// </summary>
        private void HideError()
        {
            pnlMessage.Controls.Clear();
            pnlMessage.CssClass = string.Empty;
            pnlMessage.Visible = false;
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            HideError();
            switch ( hfActiveDialog.Value )
            {
                case "WORKFLOWDETAILS":
                    {
                        mdWorkflowDetails.Show();
                        break;
                    }
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "WORKFLOWDETAILS":
                    mdWorkflowDetails.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }
        #endregion Internal Methods
    }
}