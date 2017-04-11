// <copyright>
// Copyright by the Central Christian Church
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
using System.Web.UI;
using System.Web.UI.WebControls;
using com.centralaz.RoomManagement.Model;
using Newtonsoft.Json;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_centralaz.RoomManagement
{
    [DisplayName( "Reservation Configuration" )]
    [Category( "com_centralaz > Room Management" )]
    [Description( "Displays the details of the given Connection Type for editing." )]
    public partial class ReservationConfiguration : RockBlock
    {
        #region Properties

        private List<ReservationMinistry> MinistriesState { get; set; }
        private List<ReservationWorkflowTrigger> WorkflowTriggersState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["MinistriesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                MinistriesState = new List<ReservationMinistry>();
            }
            else
            {
                MinistriesState = JsonConvert.DeserializeObject<List<ReservationMinistry>>( json );
            }


            json = ViewState["WorkflowTriggersState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                WorkflowTriggersState = new List<ReservationWorkflowTrigger>();
            }
            else
            {
                WorkflowTriggersState = JsonConvert.DeserializeObject<List<ReservationWorkflowTrigger>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gMinistries.DataKeyNames = new string[] { "Guid" };
            gMinistries.Actions.ShowAdd = true;
            gMinistries.Actions.AddClick += gMinistries_Add;
            gMinistries.GridRebind += gMinistries_GridRebind;

            gWorkflowTriggers.DataKeyNames = new string[] { "Guid" };
            gWorkflowTriggers.Actions.ShowAdd = true;
            gWorkflowTriggers.Actions.AddClick += gWorkflowTriggers_Add;
            gWorkflowTriggers.GridRebind += gWorkflowTriggers_GridRebind;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail();
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

            ViewState["MinistriesState"] = JsonConvert.SerializeObject( MinistriesState, Formatting.None, jsonSetting );
            ViewState["WorkflowTriggersState"] = JsonConvert.SerializeObject( WorkflowTriggersState, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        #endregion

        #region Events

        #region Control Events        

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ReservationMinistryService reservationMinistryService = new ReservationMinistryService( rockContext );
                ReservationWorkflowTriggerService reservationWorkflowTriggerService = new ReservationWorkflowTriggerService( rockContext );

                var reservationMinistries = reservationMinistryService.Queryable().ToList();
                var reservationWorkflowTriggers = reservationWorkflowTriggerService.Queryable().ToList();

                var uiWorkflows = WorkflowTriggersState.Select( l => l.Guid );
                foreach ( var reservationWorkflowTrigger in reservationWorkflowTriggers.Where( l => !uiWorkflows.Contains( l.Guid ) ).ToList() )
                {
                    reservationWorkflowTriggerService.Delete( reservationWorkflowTrigger );
                }

                var uiActivityTypes = MinistriesState.Select( r => r.Guid );
                foreach ( var reservationMinistry in reservationMinistries.Where( r => !uiActivityTypes.Contains( r.Guid ) ).ToList() )
                {
                    reservationMinistryService.Delete( reservationMinistry );
                }

                foreach ( var reservationMinistryState in MinistriesState )
                {
                    ReservationMinistry reservationMinistry = reservationMinistries.Where( a => a.Guid == reservationMinistryState.Guid ).FirstOrDefault();
                    if ( reservationMinistry == null )
                    {
                        reservationMinistry = new ReservationMinistry();
                        reservationMinistryService.Add( reservationMinistry );
                    }

                    reservationMinistry.CopyPropertiesFrom( reservationMinistryState );
                }

                foreach ( ReservationWorkflowTrigger reservationWorkflowTriggerState in WorkflowTriggersState )
                {
                    ReservationWorkflowTrigger reservationWorkflowTrigger = reservationWorkflowTriggers.Where( a => a.Guid == reservationWorkflowTriggerState.Guid ).FirstOrDefault();
                    if ( reservationWorkflowTrigger == null )
                    {
                        reservationWorkflowTrigger = new ReservationWorkflowTrigger();
                        reservationWorkflowTriggerService.Add( reservationWorkflowTrigger );
                    }
                    else
                    {
                        reservationWorkflowTriggerState.Id = reservationWorkflowTrigger.Id;
                        reservationWorkflowTriggerState.Guid = reservationWorkflowTrigger.Guid;
                    }

                    reservationWorkflowTrigger.CopyPropertiesFrom( reservationWorkflowTriggerState );
                }

                rockContext.SaveChanges();

                ReservationWorkflowTriggerService.FlushCachedTriggers();

                NavigateToParentPage();

            }
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

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail();
        }

        #endregion

        #region ReservationMinistry Events

        /// <summary>
        /// Handles the Delete event of the gMinistries control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMinistries_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            MinistriesState.RemoveEntity( rowGuid );
            BindReservationMinistrysGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnAddReservationMinistry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddMinistry_Click( object sender, EventArgs e )
        {
            ReservationMinistry reservationMinistry = null;
            Guid guid = hfAddMinistryGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                reservationMinistry = MinistriesState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( reservationMinistry == null )
            {
                reservationMinistry = new ReservationMinistry();
            }
            reservationMinistry.Name = tbMinistryName.Text;
            if ( !reservationMinistry.IsValid )
            {
                return;
            }
            if ( MinistriesState.Any( a => a.Guid.Equals( reservationMinistry.Guid ) ) )
            {
                MinistriesState.RemoveEntity( reservationMinistry.Guid );
            }
            MinistriesState.Add( reservationMinistry );

            BindReservationMinistrysGrid();

            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gMinistries control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gMinistries_GridRebind( object sender, EventArgs e )
        {
            BindReservationMinistrysGrid();
        }

        /// <summary>
        /// Handles the Add event of the gMinistries control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gMinistries_Add( object sender, EventArgs e )
        {
            gMinistries_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gMinistries control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gMinistries_Edit( object sender, RowEventArgs e )
        {
            Guid reservationMinistryGuid = (Guid)e.RowKeyValue;
            gMinistries_ShowEdit( reservationMinistryGuid );
        }

        /// <summary>
        /// gs the statuses_ show edit.
        /// </summary>
        /// <param name="reservationMinistryGuid">The connection status unique identifier.</param>
        protected void gMinistries_ShowEdit( Guid reservationMinistryGuid )
        {
            ReservationMinistry reservationMinistry = MinistriesState.FirstOrDefault( l => l.Guid.Equals( reservationMinistryGuid ) );
            if ( reservationMinistry != null )
            {
                tbMinistryName.Text = reservationMinistry.Name;
            }
            else
            {
                tbMinistryName.Text = string.Empty;
            }
            hfAddMinistryGuid.Value = reservationMinistryGuid.ToString();
            ShowDialog( "ReservationMinistries", true );
        }

        /// <summary>
        /// Binds the connection activity types grid.
        /// </summary>
        private void BindReservationMinistrysGrid()
        {
            SetReservationMinistryListOrder( MinistriesState );
            gMinistries.DataSource = MinistriesState.OrderBy( a => a.Name ).ToList();

            gMinistries.DataBind();
        }

        /// <summary>
        /// Sets the attribute list order.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void SetReservationMinistryListOrder( List<ReservationMinistry> reservationMinistryList )
        {
            if ( reservationMinistryList != null )
            {
                if ( reservationMinistryList.Any() )
                {
                    reservationMinistryList.OrderBy( a => a.Name ).ToList();
                }
            }
        }

        #endregion

        #region ReservationWorkflowTrigger Events

        /// <summary>
        /// Handles the SaveClick event of the dlgReservationWorkflowTrigger control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgWorkflowTrigger_SaveClick( object sender, EventArgs e )
        {
            ReservationWorkflowTrigger reservationWorkflowTrigger = null;
            Guid guid = hfAddWorkflowTriggerGuid.Value.AsGuid();
            if ( !guid.IsEmpty() )
            {
                reservationWorkflowTrigger = WorkflowTriggersState.FirstOrDefault( l => l.Guid.Equals( guid ) );
            }

            if ( reservationWorkflowTrigger == null )
            {
                reservationWorkflowTrigger = new ReservationWorkflowTrigger();
            }
            try
            {
                reservationWorkflowTrigger.WorkflowType = new WorkflowTypeService( new RockContext() ).Get( ddlWorkflowType.SelectedValueAsId().Value );
            }
            catch { }
            reservationWorkflowTrigger.WorkflowTypeId = ddlWorkflowType.SelectedValueAsId().Value;
            reservationWorkflowTrigger.TriggerType = ddlTriggerType.SelectedValueAsEnum<ReservationWorkflowTriggerType>();
            reservationWorkflowTrigger.QualifierValue = String.Format( "|{0}|{1}|", ddlPrimaryQualifier.SelectedValue, ddlSecondaryQualifier.SelectedValue );
            if ( !reservationWorkflowTrigger.IsValid )
            {
                return;
            }
            if ( WorkflowTriggersState.Any( a => a.Guid.Equals( reservationWorkflowTrigger.Guid ) ) )
            {
                WorkflowTriggersState.RemoveEntity( reservationWorkflowTrigger.Guid );
            }

            WorkflowTriggersState.Add( reservationWorkflowTrigger );
            BindReservationWorkflowTriggersGrid();
            HideDialog();
        }

        /// <summary>
        /// Handles the Delete event of the gWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflowTriggers_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = (Guid)e.RowKeyValue;
            WorkflowTriggersState.RemoveEntity( rowGuid );

            BindReservationWorkflowTriggersGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWorkflowTriggers_GridRebind( object sender, EventArgs e )
        {
            BindReservationWorkflowTriggersGrid();
        }

        /// <summary>
        /// Handles the Edit event of the gWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gWorkflowTriggers_Edit( object sender, RowEventArgs e )
        {
            Guid reservationWorkflowTriggerGuid = (Guid)e.RowKeyValue;
            gWorkflowTriggers_ShowEdit( reservationWorkflowTriggerGuid );
        }

        /// <summary>
        /// Gs the workflows_ show edit.
        /// </summary>
        /// <param name="reservationWorkflowTriggerGuid">The connection workflow unique identifier.</param>
        protected void gWorkflowTriggers_ShowEdit( Guid reservationWorkflowTriggerGuid )
        {
            ReservationWorkflowTrigger reservationWorkflowTrigger = WorkflowTriggersState.FirstOrDefault( l => l.Guid.Equals( reservationWorkflowTriggerGuid ) );
            if ( reservationWorkflowTrigger != null )
            {
                ddlTriggerType.BindToEnum<ReservationWorkflowTriggerType>();
                ddlWorkflowType.Items.Clear();
                ddlWorkflowType.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var workflowType in new WorkflowTypeService( new RockContext() ).Queryable().OrderBy( w => w.Name ) )
                {
                    if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlWorkflowType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                    }
                }

                if ( reservationWorkflowTrigger.WorkflowTypeId == null )
                {
                    ddlWorkflowType.SelectedValue = "0";
                }
                else
                {
                    ddlWorkflowType.SelectedValue = reservationWorkflowTrigger.WorkflowTypeId.ToString();
                }

                ddlTriggerType.SelectedValue = reservationWorkflowTrigger.TriggerType.ConvertToInt().ToString();
            }
            else
            {
                ddlTriggerType.BindToEnum<ReservationWorkflowTriggerType>();
                ddlWorkflowType.Items.Clear();
                ddlWorkflowType.Items.Add( new ListItem( string.Empty, string.Empty ) );

                foreach ( var workflowType in new WorkflowTypeService( new RockContext() ).Queryable().OrderBy( w => w.Name ) )
                {
                    if ( workflowType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        ddlWorkflowType.Items.Add( new ListItem( workflowType.Name, workflowType.Id.ToString() ) );
                    }
                }
            }

            hfAddWorkflowTriggerGuid.Value = reservationWorkflowTriggerGuid.ToString();
            UpdateTriggerQualifiers();
            ShowDialog( "ReservationWorkflowTriggers", true );
        }

        /// <summary>
        /// Handles the Add event of the gWorkflowTriggers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gWorkflowTriggers_Add( object sender, EventArgs e )
        {
            gWorkflowTriggers_ShowEdit( Guid.Empty );
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
            using ( var rockContext = new RockContext() )
            {
                String[] qualifierValues = new String[2];
                ReservationWorkflowTrigger reservationWorkflowTrigger = WorkflowTriggersState.FirstOrDefault( l => l.Guid.Equals( hfAddWorkflowTriggerGuid.Value.AsGuid() ) );
                ReservationWorkflowTriggerType reservationWorkflowTriggerType = ddlTriggerType.SelectedValueAsEnum<ReservationWorkflowTriggerType>();
                switch ( reservationWorkflowTriggerType )
                {
                    case ReservationWorkflowTriggerType.ReservationCreated:
                        ddlPrimaryQualifier.Visible = false;
                        ddlPrimaryQualifier.Items.Clear();
                        ddlSecondaryQualifier.Visible = false;
                        ddlSecondaryQualifier.Items.Clear();
                        break;

                    case ReservationWorkflowTriggerType.Manual:
                        ddlPrimaryQualifier.Visible = false;
                        ddlPrimaryQualifier.Items.Clear();
                        ddlSecondaryQualifier.Visible = false;
                        ddlSecondaryQualifier.Items.Clear();
                        break;

                    case ReservationWorkflowTriggerType.StateChanged:
                        ddlPrimaryQualifier.Label = "From";
                        ddlPrimaryQualifier.Visible = true;
                        ddlPrimaryQualifier.BindToEnum<ReservationApprovalState>( true );

                        ddlSecondaryQualifier.Label = "To";
                        ddlSecondaryQualifier.Visible = true;
                        ddlSecondaryQualifier.BindToEnum<ReservationApprovalState>( true );

                        break;

                    case ReservationWorkflowTriggerType.ReservationUpdated:
                        ddlPrimaryQualifier.Visible = false;
                        ddlPrimaryQualifier.Items.Clear();
                        ddlSecondaryQualifier.Visible = false;
                        ddlSecondaryQualifier.Items.Clear();
                        break;

                }

                if ( reservationWorkflowTrigger != null )
                {
                    if ( reservationWorkflowTrigger.TriggerType == ddlTriggerType.SelectedValueAsEnum<ReservationWorkflowTriggerType>() )
                    {
                        qualifierValues = reservationWorkflowTrigger.QualifierValue.SplitDelimitedValues();
                        if ( ddlPrimaryQualifier.Visible && qualifierValues.Length > 0 )
                        {
                            ddlPrimaryQualifier.SelectedValue = qualifierValues[0];
                        }

                        if ( ddlSecondaryQualifier.Visible && qualifierValues.Length > 1 )
                        {
                            ddlSecondaryQualifier.SelectedValue = qualifierValues[1];
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Binds the connection workflows grid.
        /// </summary>
        private void BindReservationWorkflowTriggersGrid()
        {
            SetReservationWorkflowTriggerListOrder( WorkflowTriggersState );
            gWorkflowTriggers.DataSource = WorkflowTriggersState.Select( c => new
            {
                c.Id,
                c.Guid,
                WorkflowType = c.WorkflowType.Name,
                Trigger = c.TriggerType.ConvertToString()
            } ).ToList();
            gWorkflowTriggers.DataBind();
        }

        /// <summary>
        /// Sets the connection workflow list order.
        /// </summary>
        /// <param name="reservationWorkflowTriggerList">The connection workflow list.</param>
        private void SetReservationWorkflowTriggerListOrder( List<ReservationWorkflowTrigger> reservationWorkflowTriggerList )
        {
            if ( reservationWorkflowTriggerList != null )
            {
                if ( reservationWorkflowTriggerList.Any() )
                {
                    reservationWorkflowTriggerList.OrderBy( c => c.WorkflowType.Name ).ThenBy( c => c.TriggerType.ConvertToString() ).ToList();
                }
            }
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="connectionTypeId">The Connection Type Type identifier.</param>
        public void ShowDetail()
        {
            pnlDetails.Visible = false;

            using ( var rockContext = new RockContext() )
            {
                pnlDetails.Visible = true;
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                if ( !UserCanAdministrate )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionType.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    ShowReadonlyDetails();
                }
                else
                {
                    ShowEditDetails();
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        private void ShowEditDetails()
        {
            var rockContext = new RockContext();
            SetEditMode( true );

            MinistriesState = new ReservationMinistryService( rockContext ).Queryable().ToList();
            WorkflowTriggersState = new ReservationWorkflowTriggerService( rockContext ).Queryable().ToList();

            BindReservationMinistrysGrid();
            BindReservationWorkflowTriggersGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        private void ShowReadonlyDetails()
        {
            SetEditMode( false );

            MinistriesState = null;
            WorkflowTriggersState = null;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;

            this.HideSecondaryBlocks( editable );
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
            switch ( hfActiveDialog.Value )
            {
                case "RESERVATIONMINISTRIES":
                    dlgMinistries.Show();
                    break;
                case "RESERVATIONWORKFLOWTRIGGERS":
                    dlgWorkflowTrigger.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "RESERVATIONMINISTRIES":
                    dlgMinistries.Hide();
                    break;
                case "RESERVATIONWORKFLOWTRIGGERS":
                    dlgWorkflowTrigger.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
    }
}