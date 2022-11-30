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
            public const string DueFilter = "DueFilter";
            public const string CustomDateRange = "CustomDateRange";
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
                var currentReminderCount = CurrentPerson.ReminderCount ?? 0;
                int updatedReminderCount = reminderService.RecalculateReminderCount( CurrentPersonId.Value );

                if ( updatedReminderCount != currentReminderCount )
                {
                    // The RecalculateReminderCount() service method has already updated the database record, if required, but since
                    // CurrentPerson may be cached by the RockPage, we need to be sure we display the correct Reminder count when the page loads.
                    CurrentPerson.ReminderCount = updatedReminderCount;
                }
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

            var entityTypes = new List<EntityType>();
            var reminderTypes = new List<ReminderType>();
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );

                entityTypes = reminderService.GetReminderEntityTypesByPerson( CurrentPersonId.Value ).ToList();

                reminderTypes = reminderService.GetReminderTypesByPerson( selectedEntityTypeId, CurrentPerson );
                rptReminderType.DataSource = reminderTypes;
                rptReminderType.DataBind();
            }

            if ( entityTypes.Count == 0 )
            {
                // This user doesn't have any reminders.  We can stop here.
                pnlNoReminders.Visible = true;
                pnlView.Visible = false;
                return;
            }
            else if ( entityTypes.Count == 1 )
            {
                // This user only has a reminder for a single entity type, so hide that dropdown.
                lSelectedEntityType.Text = entityTypes[0].FriendlyName.Pluralize();
            }

            BindEntityTypeList( entityTypes );
            BindReminderList( selectedEntityTypeId, selectedEntityId, selectedReminderTypeId );
            SetReminderTypeFilter( reminderTypes, selectedReminderTypeId );

            if ( !selectedEntityTypeId.HasValue )
            {
                return;
            }

            // Show Person Picker if EntityType matches.
            var entityTypeId_Person = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON.AsGuid() );
            if ( selectedEntityTypeId == entityTypeId_Person )
            {
                lSelectedEntityType.Text = "People";
                ppSelectedPerson.Visible = true;
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
                lSelectedEntityType.Text = "Groups";
                gpSelectedGroup.Visible = true;
                gpSelectedGroup.SetValue( selectedEntityId );
                return;
            }

            var selectedEntityType = EntityTypeCache.Get( selectedEntityTypeId.Value );
            if ( selectedEntityType != null )
            {
                lSelectedEntityType.Text = selectedEntityType.FriendlyName.Pluralize();
            }
        }

        /// <summary>
        /// Binds the entity type list.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private void BindEntityTypeList( List<EntityType> entityTypes )
        {
            entityTypes.Insert( 0, new EntityType() { FriendlyName = "All Reminders", Id = 0 } );
            rptEntityTypeList.DataSource = entityTypes;
            rptEntityTypeList.DataBind();
        }

        /// <summary>
        /// Binds the reminder list.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private void BindReminderList( int? entityTypeId, int? entityId, int? reminderTypeId )
        {
            var reminders = GetReminders( entityTypeId, entityId, reminderTypeId );
            rptReminders.DataSource = reminders;
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

            var completionFilter = GetBlockUserPreference( UserPreferenceKey.CompletionFilter );
            var dueFilter = GetBlockUserPreference( UserPreferenceKey.DueFilter );

            if ( completionFilter.IsNullOrWhiteSpace() )
            {
                completionFilter = "Active";
            }

            if ( dueFilter.IsNullOrWhiteSpace() )
            {
                dueFilter = "Due";
            }

            using ( var rockContext = new RockContext() )
            {
                var entityTypeService = new EntityTypeService( rockContext );
                var reminderService = new ReminderService( rockContext );
                var reminders = reminderService.GetReminders( CurrentPersonId.Value, entityTypeId, entityId, reminderTypeId );

                // Filter for completion status.
                lCompletionFilter.Text = completionFilter;
                if ( completionFilter == "Active")
                {
                    reminders = reminders.Where( r => !r.IsComplete );
                }
                else if ( completionFilter == "Complete" )
                {
                    reminders = reminders.Where( r => r.IsComplete );
                }

                // Filter for overdue timeframe.
                lDueFilter.Text = dueFilter;
                hfDueFilterSetting.Value = dueFilter;
                if ( dueFilter == "Due")
                {
                    var currentDate = RockDateTime.Now;
                    reminders = reminders.Where( r => r.ReminderDate <= currentDate );
                }
                else if ( dueFilter == "Due This Week")
                {
                    var nextWeekStartDate = RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek ).AddDays( 1 );
                    var startOfWeek = nextWeekStartDate.AddDays( -7 );
                    reminders = reminders.Where( r => r.ReminderDate >= startOfWeek && r.ReminderDate < nextWeekStartDate );
                }
                else if ( dueFilter == "Due This Month")
                {
                    var startOfMonth = RockDateTime.Now.StartOfMonth();
                    var nextMonthDate = RockDateTime.Now.AddMonths( 1 );
                    var nextMonthStartDate = new DateTime( nextMonthDate.Year, nextMonthDate.Month, 1 );
                    reminders = reminders.Where( r => r.ReminderDate >= startOfMonth && r.ReminderDate < nextMonthStartDate );
                }
                else
                {
                    // Custom date range.
                    var selectedDateRange = GetBlockUserPreference( UserPreferenceKey.CustomDateRange );
                    if ( selectedDateRange.IsNotNullOrWhiteSpace() )
                    {
                        drpCustomDate.DelimitedValues = selectedDateRange;
                        lDueFilter.Text = "Custom Date Range";
                        hfDueFilterSetting.Value = "Custom Date Range";
                        var dateRange = new TimePeriod( selectedDateRange ).GetDateRange();
                        var startDate = dateRange.Start;
                        var endDate = dateRange.End;
                        reminders = reminders.Where( r => r.ReminderDate >= startDate && r.ReminderDate < endDate );
                    }
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
        /// Sets the reminder type filter display.
        /// </summary>
        /// <param name="reminderTypes">The available Reminder Types.</param>
        /// <param name="selectedReminderType">The selected reminder type identifier.</param>
        private void SetReminderTypeFilter( List<ReminderType> reminderTypes, int? selectedReminderType )
        {
            if ( !selectedReminderType.HasValue )
            {
                return;
            }

            foreach ( var reminderType in reminderTypes )
            {
                if ( reminderType.Id == selectedReminderType )
                {
                    lReminderType.Text = reminderType.Name;
                    return;
                }
            }

            lReminderType.Text = "All Reminder Types";
        }

        /// <summary>
        /// Reload the page with appropriate page parameters.  This is useful when the filter values are updated.
        /// </summary>
        private void RefreshPage()
        {
            var selectedEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();
            var selectedEntityId = PageParameter( PageParameterKey.EntityId ).AsIntegerOrNull();
            var selectedReminderTypeId = PageParameter( PageParameterKey.ReminderTypeId ).AsIntegerOrNull();
            RefreshPage( selectedEntityTypeId, selectedEntityId, selectedReminderTypeId );
        }

        /// <summary>
        /// Reload the page with appropriate page parameters.  This is useful when the reminder type filter is updated.
        /// </summary>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private void RefreshPage( int? reminderTypeId )
        {
            var selectedEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();
            var selectedEntityId = PageParameter( PageParameterKey.EntityId ).AsIntegerOrNull();
            RefreshPage( selectedEntityTypeId, selectedEntityId, reminderTypeId );
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

            if ( entityTypeId.HasValue && entityTypeId != 0 )
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
            btnEntityType.Text = entityType.FriendlyName.Pluralize();
            btnEntityType.CommandArgument = entityType.Id.ToString();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptReminderType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptReminderType_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var btnEntityType = e.Item.FindControl( "btnEntityType" ) as LinkButton;
            var reminderType = ( ReminderType ) e.Item.DataItem;
            btnEntityType.Text = reminderType.Name;
            btnEntityType.CommandArgument = reminderType.Id.ToString();
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
            var currentEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();

            if ( entityTypeId != currentEntityTypeId )
            {
                RefreshPage( entityTypeId, null, null );
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
                if ( reminder.IsComplete )
                {
                    reminder.IsComplete = false;
                }
                else
                {
                    reminder.CompleteReminder();
                }
                rockContext.SaveChanges();
            }

            InitializeBlock();
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

        /// <summary>
        /// Handles the Click event of the btnCompletion controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCompletion_Click( object sender, EventArgs e )
        {
            var btnCompletion = sender as LinkButton;
            SetBlockUserPreference( UserPreferenceKey.CompletionFilter, btnCompletion.CommandArgument );
            RefreshPage();
        }

        /// <summary>
        /// Handles the Click event of the btnReminderType controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReminderType_Click( object sender, EventArgs e )
        {
            var btnReminderType = sender as LinkButton;

            int? reminderTypeId = null;
            if ( btnReminderType.CommandArgument != "All" )
            {
                reminderTypeId = int.Parse( btnReminderType.CommandArgument );
            }
            RefreshPage( reminderTypeId );
        }

        /// <summary>
        /// Handles the Click event of the btnActive controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDue_Click( object sender, EventArgs e )
        {
            var btnDue = sender as LinkButton;
            SetBlockUserPreference( UserPreferenceKey.DueFilter, btnDue.CommandArgument );
            RefreshPage();
        }

        /// <summary>
        /// Handles the SelectedDateRangeChanged event of the drpCustomDate controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void drpCustomDate_SelectedDateRangeChanged( object sender, EventArgs e )
        {
            if ( Page.IsPostBack )
            {
                SetBlockUserPreference( UserPreferenceKey.DueFilter, "Custom Date Range" );
                SetBlockUserPreference( UserPreferenceKey.CustomDateRange, drpCustomDate.DelimitedValues );
                RefreshPage();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptReminders controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptReminders_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var reminder = e.Item.DataItem as ReminderDTO;

            if ( reminder.EntityUrl.IsNotNullOrWhiteSpace() )
            {
                var entityUrl = ResolveUrl( reminder.EntityUrl );
                var lEntity = e.Item.FindControl( "lEntity" ) as Literal;
                lEntity.Text = $"<a href=\"{entityUrl}\">{reminder.EntityDescription}</a>";
            }

            if ( reminder.IsPersonReminder )
            {
                var photoUrl = Person.GetPersonPhotoUrl( reminder.EntityId );
                var litProfilePhoto = e.Item.FindControl( "litProfilePhoto" ) as Literal;
                litProfilePhoto.Visible = true;
                litProfilePhoto.Text = string.Format( litProfilePhoto.Text, photoUrl );
            }
        }

        #endregion Events
    }
}