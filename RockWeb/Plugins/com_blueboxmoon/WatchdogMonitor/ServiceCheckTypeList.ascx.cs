using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using com.blueboxmoon.WatchdogMonitor;
using com.blueboxmoon.WatchdogMonitor.Model;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_blueboxmoon.WatchdogMonitor
{
    [DisplayName( "Service Check Type List" )]
    [Category( "Blue Box Moon > Watchdog Monitor" )]
    [Description( "Lists service check types in the system." )]
    public partial class ServiceCheckTypeList : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the service check type entity type identifier.
        /// </summary>
        /// <value>
        /// The service check type entity type identifier.
        /// </value>
        protected int ServiceCheckTypeEntityTypeId
        {
            get
            {
                return ViewState["ServiceCheckTypeEntityTypeId"] as int? ?? 0;
            }
            set
            {
                ViewState["ServiceCheckTypeEntityTypeId"] = value;
            }
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var serviceCheckType = new WatchdogServiceCheckType()
            {
                EntityTypeId = ServiceCheckTypeEntityTypeId
            };
            serviceCheckType.LoadAttributes();
            BuildDynamicControls( serviceCheckType, false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            var entityType = new EntityTypeService( new RockContext() ).Get( typeof( WatchdogServiceCheckType ).FullName );

            gServiceCheckType.DataKeyNames = new string[] { "Id" };
            gServiceCheckType.Actions.AddClick += gServiceCheckType_Add;
            gServiceCheckType.GridRebind += gServiceCheckType_GridRebind;
            gServiceCheckType.Actions.ShowAdd = entityType.IsAuthorized( Authorization.EDIT, CurrentPerson );

            if ( mdlEdit.Visible )
            {
                var serviceCheckType = new WatchdogServiceCheckTypeService( new RockContext() ).Get( hfEditId.ValueAsInt() );
                serviceCheckType.EntityTypeId = cpEditProvider.SelectedEntityTypeId ?? 0;

                Helper.AddEditControls( serviceCheckType, phEditAttributes, false, mdlEdit.ValidationGroup, new List<string> { "Order", "Active" }, false );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Bind the data grid to the list of project types in the system.
        /// </summary>
        private void BindGrid()
        {
            var serviceCheckTypeService = new WatchdogServiceCheckTypeService( new RockContext() );
            var sortProperty = gServiceCheckType.SortProperty;

            var types = serviceCheckTypeService.Queryable()
                .ToList()
                .Where( t => t.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                .Select( t => new
                {
                    t.Id,
                    t.Name,
                    t.IsActive,
                    Provider = t.EntityType != null ? ServiceCheckContainer.GetComponentName( t.EntityType.Name ) : string.Empty,
                    t.CheckInterval,
                    t.IsSystem,
                    CanDelete = t.IsAuthorized( Authorization.EDIT, CurrentPerson )
                } )
                .ToList();

            if ( sortProperty != null )
            {
                types = types.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                types = types.OrderBy( t => t.Name ).ThenBy( t => t.Id ).ToList();
            }

            gServiceCheckType.EntityTypeId = EntityTypeCache.Get<WatchdogServiceCheckType>().Id;
            gServiceCheckType.DataSource = types;
            gServiceCheckType.DataBind();
        }

        /// <summary>
        /// Shows the edit dialog.
        /// </summary>
        /// <param name="id">The identifier of the service check type to show or 0 to add new.</param>
        private void ShowEdit( int id )
        {
            var serviceCheckType = new WatchdogServiceCheckTypeService( new RockContext() ).Get( id );

            if ( serviceCheckType == null )
            {
                serviceCheckType = new WatchdogServiceCheckType
                {
                    IsActive = true,
                    CheckInterval = 5,
                    RecheckInterval = 1,
                    RecheckCount = 3
                };
            }

            bool canEdit = serviceCheckType.IsAuthorized( Authorization.EDIT, CurrentPerson );

            hfEditId.Value = serviceCheckType.Id.ToString();
            tbEditName.Text = serviceCheckType.Name;
            tbEditDescription.Text = serviceCheckType.Description;
            cbEditIsActive.Checked = serviceCheckType.IsActive;
            cpEditProvider.SetValue( serviceCheckType.EntityType != null ? serviceCheckType.EntityType.Guid.ToString().ToUpper() : string.Empty );
            nbEditCheckInterval.Text = serviceCheckType.CheckInterval.ToString();
            nbEditRecheckInterval.Text = serviceCheckType.RecheckInterval.ToString();
            nbEditRecheckCount.Text = serviceCheckType.RecheckCount.ToString();

            nbEditModeMessage.Text = serviceCheckType.IsSystem ? EditModeMessage.System( WatchdogServiceCheckType.FriendlyTypeName ) : string.Empty;

            tbEditName.Enabled = !serviceCheckType.IsSystem && canEdit;
            cbEditIsActive.Enabled = canEdit;
            cpEditProvider.Enabled = !serviceCheckType.IsSystem && canEdit;
            nbEditCheckInterval.Enabled = canEdit;
            nbEditRecheckInterval.Enabled = canEdit;
            nbEditRecheckCount.Enabled = canEdit;
            tbEditDescription.Enabled = !serviceCheckType.IsSystem && canEdit;


            ServiceCheckTypeEntityTypeId = serviceCheckType.EntityTypeId;
            BuildDynamicControls( serviceCheckType, true );

            mdlEdit.SaveButtonText = canEdit ? "Save" : string.Empty;

            mdlEdit.Show();
        }

        /// <summary>
        /// Build the dynamic edit controls for this service check type.
        /// </summary>
        /// <param name="serviceCheckType">The service check type whose controls need to be built.</param>
        /// <param name="setValues">Whether or not to set the initial values.</param>
        protected void BuildDynamicControls( WatchdogServiceCheckType serviceCheckType, bool setValues )
        {
            serviceCheckType.LoadAttributes();
            phEditAttributes.Controls.Clear();

            if ( serviceCheckType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                Rock.Attribute.Helper.AddEditControls( serviceCheckType, phEditAttributes, setValues, mdlEdit.ValidationGroup, new List<string> { "Active", "Order" }, false );
            }
            else
            {
                Rock.Attribute.Helper.AddDisplayControls( serviceCheckType, phEditAttributes, new List<string> { "Active", "Order" }, false, false );
            }
            foreach ( var tb in phEditAttributes.ControlsOfTypeRecursive<TextBox>() )
            {
                tb.AutoCompleteType = AutoCompleteType.Disabled;
                tb.Attributes["autocomplete"] = "off";
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gServiceCheckType_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gServiceCheckType_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var serviceCheckTypeService = new WatchdogServiceCheckTypeService( rockContext );
            var serviceCheckType = serviceCheckTypeService.Get( e.RowKeyId );

            if ( serviceCheckType != null && serviceCheckType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                serviceCheckTypeService.Delete( serviceCheckType );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        protected void gServiceCheckType_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gServiceCheckType_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gServiceCheckType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gServiceCheckType_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                int deleteIndex = gServiceCheckType.Columns.IndexOf( gServiceCheckType.Columns.OfType<DeleteField>().Single() );
                var deleteButton = e.Row.Cells[deleteIndex].ControlsOfTypeRecursive<LinkButton>().Single();

                deleteButton.Enabled = deleteButton.Enabled && ( bool ) e.Row.DataItem.GetPropertyValue( "CanDelete" );
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the mdlEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdlEdit_SaveClick( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var serviceCheckTypeService = new WatchdogServiceCheckTypeService( rockContext );
            var serviceCheckType = serviceCheckTypeService.Get( hfEditId.ValueAsInt() );

            if ( serviceCheckType == null )
            {
                serviceCheckType = new WatchdogServiceCheckType();
                serviceCheckTypeService.Add( serviceCheckType );
            }

            if ( serviceCheckType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
            {
                serviceCheckType.Name = tbEditName.Text;
                serviceCheckType.Description = tbEditDescription.Text;
                serviceCheckType.IsActive = cbEditIsActive.Checked;
                serviceCheckType.EntityTypeId = cpEditProvider.SelectedEntityTypeId.Value;
                serviceCheckType.CheckInterval = nbEditCheckInterval.Text.AsInteger();
                serviceCheckType.RecheckInterval = nbEditRecheckInterval.Text.AsInteger();
                serviceCheckType.RecheckCount = nbEditRecheckCount.Text.AsInteger();

                serviceCheckType.LoadAttributes( rockContext );
                Helper.GetEditValues( phEditAttributes, serviceCheckType );

                rockContext.SaveChanges();
                serviceCheckType.SaveAttributeValues( rockContext );
            }

            mdlEdit.Hide();
            BindGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpEditProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpEditProvider_SelectedIndexChanged( object sender, EventArgs e )
        {
            var serviceCheckType = new WatchdogServiceCheckType
            {
                EntityTypeId = cpEditProvider.SelectedEntityTypeId ?? 0
            };

            ServiceCheckTypeEntityTypeId = serviceCheckType.EntityTypeId;
            BuildDynamicControls( serviceCheckType, true );
        }

        #endregion
    }
}