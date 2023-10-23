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

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reminders
{
    [DisplayName( "Reminder Types" )]
    [Category( "Reminders" )]
    [Description( "Block for editing reminder types." )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_TYPES )]
    public partial class ReminderTypes : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            gReminderTypes.DataKeyNames = new string[] { "Id" };
            gReminderTypes.GridRebind += gReminderTypes_GridRebind;
            gReminderTypes.GridReorder += gReminderTypes_GridReorder;
            gReminderTypes.RowItemText = "Reminder Type";

            var securityField = gReminderTypes.ColumnsOfType<SecurityField>().FirstOrDefault();
            if ( securityField != null )
            {
                securityField.EntityTypeId = EntityTypeCache.GetId<ReminderType>().Value;
            }

            var isUserAuthorized = IsUserAuthorized( Authorization.EDIT );
            gReminderTypes.Actions.ShowAdd = isUserAuthorized;
            gReminderTypes.IsDeleteEnabled = isUserAuthorized;

            if ( isUserAuthorized )
            {
                gReminderTypes.Actions.AddClick += gReminderTypes_AddClick;
                gReminderTypes.RowSelected += gReminderTypes_RowSelected;
            }

            ddlNotificationType.BindToEnum<Rock.Model.ReminderNotificationType>( true );

            BindEntityTypes();
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
                InitializeBlock();
            }
        }

        #endregion Base Control Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the GridRebind event of the gReminderTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gReminderTypes_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gReminderTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gReminderTypes_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var reminderTypeService = new ReminderTypeService( rockContext );

            var reminderTypes = reminderTypeService.Queryable().OrderBy( rt => rt.Order ).ToList();
            reminderTypeService.Reorder( reminderTypes, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gReminderTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gReminderTypes_AddClick( object sender, EventArgs e )
        {
            LoadReminderType( 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gReminderTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gReminderTypes_RowSelected( object sender, RowEventArgs e )
        {
            var reminderTypeId = e.RowKeyId;
            LoadReminderType( reminderTypeId );
        }

        /// <summary>
        /// Handles the Delete event of the gReminderTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gReminderTypes_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var reminderTypeService = new ReminderTypeService( rockContext );

            var reminderTypeId = e.RowKeyId;
            var reminderType = reminderTypeService.Get( reminderTypeId );

            if ( reminderType == null )
            {
                mdGridWarning.Show( "The reminder type could not be found.", ModalAlertType.Information );
                return;
            }

            var errorMessage = string.Empty;
            if ( !reminderTypeService.CanDelete( reminderType, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            reminderTypeService.Delete( reminderType );
            rockContext.SaveChanges();
            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdEditReminderType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdEditReminderType_SaveClick( object sender, EventArgs e )
        {
            int reminderTypeId = hfReminderTypeId.ValueAsInt();

            var rockContext = new RockContext();
            var reminderTypeService = new ReminderTypeService( rockContext );
            ReminderType reminderType;

            if ( reminderTypeId == 0 )
            {
                reminderType = new ReminderType();
                reminderTypeService.Add( reminderType );

                var orders = reminderTypeService.Queryable()
                    .Select( d => d.Order )
                    .ToList();

                reminderType.Order = orders.Any() ? orders.Max() + 1 : 0;
            }
            else
            {
                reminderType = reminderTypeService.Get( reminderTypeId );
            }

            // If user selects Person, use PersonAlias.
            var entityTypeId = etpEntityType.SelectedValueAsId().Value;
            if ( entityTypeId == EntityTypeCache.GetId<Person>() )
            {
                entityTypeId = EntityTypeCache.GetId<PersonAlias>().Value;
            }

            reminderType.Name = tbName.Text;
            reminderType.Description = tbDescription.Text;
            reminderType.IsActive = cbActive.Checked;
            reminderType.NotificationType = ddlNotificationType.SelectedValueAsEnum<ReminderNotificationType>();
            reminderType.NotificationWorkflowTypeId = wtpWorkflowType.SelectedValueAsId();
            reminderType.ShouldShowNote = cbShouldShowNote.Checked;
            reminderType.EntityTypeId = entityTypeId;
            reminderType.ShouldAutoCompleteWhenNotified = cbShouldAutoComplete.Checked;
            reminderType.HighlightColor = cpHighlightColor.Text;

            rockContext.SaveChanges();
            mdEditReminderType.Hide();
            BindGrid();
        }

        protected void ddlNotificationType_SelectedIndexChanged( object sender, EventArgs e )
        {
            wtpWorkflowType.Required = ( ddlNotificationType.SelectedValue == "1" );
        }

        #endregion Events

        #region Methods

        private void InitializeBlock()
        {
            BindGrid();
        }

        /// <summary>
        /// Provide the options for the entity type picker
        /// </summary>
        private void BindEntityTypes()
        {
            var rockContext = new RockContext();
            var entityTypes = new EntityTypeService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( e => e.IsEntity )
                .OrderBy( t => t.FriendlyName )
                .ToList();

            etpEntityType.EntityTypes = entityTypes;
        }

        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderTypeService = new ReminderTypeService( rockContext );
                var reminderTypes = reminderTypeService.Queryable()
                    .OrderBy( t => t.Order )
                    .ThenBy( t => t.Name )
                    .ThenBy( t => t.Id )
                    .ToList();
                gReminderTypes.DataSource = reminderTypes;
                gReminderTypes.DataBind();
            }
        }

        private void LoadReminderType( int reminderTypeId )
        {
            hfReminderTypeId.Value = reminderTypeId.ToString();

            if ( reminderTypeId == 0 )
            {
                tbName.Text = string.Empty;
                tbDescription.Text = string.Empty;
                cbActive.Checked = true;
                ddlNotificationType.SelectedValue = string.Empty;
                wtpWorkflowType.SetValue( null );
                cbShouldShowNote.Checked = true;
                etpEntityType.SetValue( "0" );
                cbShouldAutoComplete.Checked = false;
                cpHighlightColor.Text = string.Empty;
            }
            else
            {
                var rockContext = new RockContext();
                var reminderType = new ReminderTypeService( new RockContext() ).Get( reminderTypeId );
                if ( reminderType == null )
                {
                    mdGridWarning.Show( "The reminder type could not be found.", ModalAlertType.Information );
                    return;
                }

                tbName.Text = reminderType.Name;
                tbDescription.Text = reminderType.Description;
                cbActive.Checked = reminderType.IsActive;
                ddlNotificationType.SelectedValue = ( ( int ) reminderType.NotificationType ).ToString();
                wtpWorkflowType.SetValue( reminderType.NotificationWorkflowTypeId );
                cbShouldShowNote.Checked = reminderType.ShouldShowNote;
                etpEntityType.SetValue( reminderType.EntityType );
                cbShouldAutoComplete.Checked = reminderType.ShouldAutoCompleteWhenNotified;
                cpHighlightColor.Text = reminderType.HighlightColor;
            }

            mdEditReminderType.Show();
        }

        #endregion Methods
    }
}