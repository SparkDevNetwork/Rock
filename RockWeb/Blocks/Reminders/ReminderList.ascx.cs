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
using System.IO;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Reminders
{
    [DisplayName( "Reminder List" )]
    [Category( "Reminders" )]
    [Description( "Block to show a list of reminders." )]

    #region Block Attributes

    [LinkedPage(
        "Edit Reminder Page",
        Description = "The page where a person can edit a reminder.",
        DefaultValue = Rock.SystemGuid.Page.REMINDER_EDIT,
        Order = 1,
        Key = AttributeKey.EditReminderPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_LIST )]
    public partial class ReminderList : RockBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Keys for Block Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string EditReminderPage = "EditReminderPage";
        }

        /// <summary>
        /// Keys for Page Parameters.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The Entity Type Identifier.
            /// </summary>
            public const string EntityTypeId = "EntityTypeId";

            /// <summary>
            /// The Entity Identifier.
            /// </summary>
            public const string EntityId = "EntityId";

            /// <summary>
            /// The Reminder Type Identifier.
            /// </summary>
            public const string ReminderTypeId = "ReminderTypeId";

            /// <summary>
            /// The Reminder Identifier.
            /// </summary>
            public const string ReminderId = "ReminderId";
        }

        /// <summary>
        /// Keys for User Preference settings.
        /// </summary>
        private static class UserPreferenceKey
        {
            public const string CompletionFilter = "CompletionFilter";
            public const string ActiveFilter = "ActiveFilter";
            public const string StartDate = "StartDate";
            public const string EndDate = "EndDate";
        }

        #endregion Keys

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

            if ( !Page.IsPostBack )
            {
                RecalculateReminders();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentPersonId.HasValue )
            {
                if ( !Page.IsPostBack )
                {
                    InitializeBlock();
                }
            }
            else
            {
                pnlNotAuthenticated.Visible = true;
                pnlView.Visible = false;
            }
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Recalculates the reminder count for the current person.  This is done during the page init
        /// to ensure that the reminders badge is updated any time a user visits a page with this block.
        /// </summary>
        private void RecalculateReminders()
        {
            if ( !CurrentPersonId.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                reminderService.RecalculateReminderCount( CurrentPersonId.Value );
            }
        }

        /// <summary>
        /// Initialize the block.
        /// </summary>
        private void InitializeBlock()
        {
            var selectedEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();
            var selectedEntityId = PageParameter( PageParameterKey.EntityId ).AsIntegerOrNull();
            var selectedReminderTypeId = PageParameter( PageParameterKey.ReminderTypeId ).AsIntegerOrNull();

            BindEntityTypeList();
            BindReminderList( selectedEntityTypeId, selectedEntityId, selectedReminderTypeId );

            if ( !selectedEntityTypeId.HasValue )
            {
                return;
            }

            // Show Person Picker if EntityType matches.
            var entityTypeId_Person = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON.AsGuid() );
            if ( selectedEntityTypeId == entityTypeId_Person )
            {
                lSelectedEntityType.Text = "Person";
                pnlPersonPicker.Visible = true;
                if ( selectedEntityId.HasValue )
                {
                    var person = new PersonService( new RockContext() ).Get( selectedEntityId.Value );
                    ppSelectedPerson.SetValue( person );
                }
                return;
            }

            // Show Group Picker if EntityType matches.
            var entityTypeId_Group = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.GROUP.AsGuid() );
            if ( selectedEntityTypeId == entityTypeId_Group )
            {
                lSelectedEntityType.Text = "Group";
                pnlGroupPicker.Visible = true;
                gpSelectedGroup.SetValue( selectedEntityId );
                return;
            }

            var selectedEntityType = EntityTypeCache.Get( selectedEntityTypeId.Value );
            lSelectedEntityType.Text = selectedEntityType.FriendlyName;
        }

        /// <summary>
        /// Binds the entity type list.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private void BindEntityTypeList()
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var entityTypes = reminderService.GetReminderEntityTypesByPerson( CurrentPersonId.Value ).ToList();
                rptEntityTypeList.DataSource = entityTypes;
                rptEntityTypeList.DataBind();
            }
        }

        /// <summary>
        /// Binds the reminder list.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private void BindReminderList( int? entityTypeId, int? entityId, int? reminderTypeId )
        {
            rptReminders.DataSource = GetReminders( entityTypeId, entityId, reminderTypeId );
            rptReminders.DataBind();
        }

        /// <summary>
        /// Gets the reminders.
        /// </summary>
        /// <returns></returns>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private List<ReminderDTO> GetReminders( int? entityTypeId, int? entityId, int? reminderTypeId )
        {
            var reminderDTOs = new List<ReminderDTO>();

            // ToDo:  Get these from dropdowns.

            var completionFilter = GetBlockUserPreference( UserPreferenceKey.CompletionFilter );
            var activeFilter = GetBlockUserPreference( UserPreferenceKey.ActiveFilter );

            using ( var rockContext = new RockContext() )
            {
                var entityTypeService = new EntityTypeService( rockContext );
                var reminderService = new ReminderService( rockContext );
                var reminders = reminderService.GetReminders( CurrentPersonId.Value, entityTypeId, entityId, reminderTypeId );

                // Filter for completion status.
                if ( completionFilter == "Incomplete" )
                {
                    reminders = reminders.Where( r => !r.IsComplete );
                }
                else if ( completionFilter == "Complete" )
                {
                    reminders = reminders.Where( r => r.IsComplete );
                }

                // Filter for active status.
                if ( activeFilter == "Active" )
                {
                    var currentDate = RockDateTime.Now;
                    reminders = reminders.Where( r => r.ReminderDate <= currentDate );
                }
                else if ( activeFilter == "Active This Week" )
                {
                    // Check with DSD - Rock setting for start of week?
                    var nextWeekStartDate = RockDateTime.Now.EndOfWeek( DayOfWeek.Sunday ).AddDays( 1 );
                    reminders = reminders.Where( r => r.ReminderDate < nextWeekStartDate );
                }
                else if ( activeFilter == "Active This Month" )
                {
                    var nextMonthDate = RockDateTime.Now.AddMonths( 1 );
                    var nextMonthStartDate = new DateTime( nextMonthDate.Year, nextMonthDate.Month, 1 );
                    reminders = reminders.Where( r => r.ReminderDate < nextMonthStartDate );
                }
                else
                {
                    // Custom date range.
                    var startDate = GetBlockUserPreference( UserPreferenceKey.StartDate ).AsDateTime() ?? RockDateTime.Now.Date;
                    var endDate = GetBlockUserPreference( UserPreferenceKey.EndDate ).AsDateTime() ?? RockDateTime.Now.Date.AddDays( 14 );
                    reminders = reminders.Where( r => r.ReminderDate >= startDate && r.ReminderDate < endDate );
                }

                foreach ( var reminder in reminders.ToList() )
                {
                    var entity = entityTypeService.GetEntity( reminder.ReminderType.EntityTypeId, reminder.EntityId );
                    reminderDTOs.Add( new ReminderDTO( reminder, entity ) );
                }
            }

            return reminderDTOs;
        }

        /// <summary>
        /// Reload the page with appropriate page parameters.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private void RefreshPage( int? entityTypeId, int? entityId, int? reminderTypeId )
        {
            var queryParams = new Dictionary<string, string>();

            if ( entityTypeId.HasValue )
            {
                queryParams.Add( PageParameterKey.EntityTypeId, entityTypeId.ToString() );
            }

            if ( entityId.HasValue )
            {
                queryParams.Add( PageParameterKey.EntityId, entityId.ToString() );
            }

            if ( reminderTypeId.HasValue )
            {
                queryParams.Add( PageParameterKey.ReminderTypeId, reminderTypeId.ToString() );
            }

            NavigateToCurrentPage( queryParams );
        }

        /// <summary>
        /// Edit a reminder.
        /// </summary>
        /// <param name="reminderId">The reminder identifier</param>
        private void EditReminder( int reminderId )
        {
            var queryParams = new Dictionary<string, string>
            {
                { PageParameterKey.ReminderId, reminderId.ToString() }
            };

            NavigateToLinkedPage( AttributeKey.EditReminderPage, queryParams );
        }

        /// <summary>
        /// Delete a reminder.
        /// </summary>
        /// <param name="reminderId">The reminder identifier</param>
        private void DeleteReminder( int reminderId )
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var reminder = reminderService.Get( reminderId );
                reminderService.Delete( reminder );
                rockContext.SaveChanges();
            }
        }

        #endregion Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            InitializeBlock();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptEntityTypeList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptEntityTypeList_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var btnEntityType = e.Item.FindControl( "btnEntityType" ) as LinkButton;
            var entityType = ( EntityType ) e.Item.DataItem;
            btnEntityType.Text = entityType.FriendlyName;
            btnEntityType.CommandArgument = entityType.Id.ToString();
        }

        /// <summary>
        /// Handles the Click event of the btnEntityType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEntityType_Click( object sender, EventArgs e )
        {
            LinkButton btnEntityType = sender as LinkButton;
            var entityTypeId = btnEntityType.CommandArgument.AsIntegerOrNull();
            var selectedEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();

            if ( entityTypeId != selectedEntityTypeId )
            {
                RefreshPage( selectedEntityTypeId, null, null );
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the gpSelectedGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpSelectedGroup_ValueChanged( object sender, EventArgs e )
        {
            var selectedEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();
            var selectedReminderTypeId = PageParameter( PageParameterKey.ReminderTypeId ).AsIntegerOrNull();
            var selectedEntityId = gpSelectedGroup.SelectedValueAsId();
            RefreshPage( selectedEntityTypeId, selectedEntityId, selectedReminderTypeId );
        }

        /// <summary>
        /// Handles the ValueChanged event of the ppSelectedPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppSelectedPerson_ValueChanged( object sender, EventArgs e )
        {
            var selectedEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();
            var selectedReminderTypeId = PageParameter( PageParameterKey.ReminderTypeId ).AsIntegerOrNull();
            var selectedEntityId = ppSelectedPerson.SelectedValue;
            RefreshPage( selectedEntityTypeId, selectedEntityId, selectedReminderTypeId );
        }

        /// <summary>
        /// Handles the Click event of the lbComplete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbComplete_Click( object sender, EventArgs e )
        {
            var lbComplete = sender as LinkButton;
            if ( lbComplete == null )
            {
                return;
            }

            var reminderId = lbComplete.CommandArgument.AsIntegerOrNull();
            if ( reminderId == null )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var reminder = new ReminderService( rockContext ).Get( reminderId.Value );
                reminder.CompleteReminder();
                rockContext.SaveChanges();
            }

            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var lbEdit = sender as LinkButton;
            if ( lbEdit == null )
            {
                return;
            }

            var reminderId = lbEdit.CommandArgument.AsIntegerOrNull();
            if ( reminderId == null )
            {
                return;
            }

            EditReminder( reminderId.Value );
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            var lbDelete = sender as LinkButton;
            if ( lbDelete == null )
            {
                return;
            }

            var reminderId = lbDelete.CommandArgument.AsIntegerOrNull();
            if ( reminderId == null )
            {
                return;
            }

            DeleteReminder( reminderId.Value );

            NavigateToCurrentPageReference();
        }

        #endregion Events
    }
}