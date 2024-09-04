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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
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

    [ReminderTypesField(
        "Reminder Types Include",
        Description = "Select any specific reminder types to show in this block. Leave all unchecked to show all active reminder types ( except for excluded reminder types ).",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.ReminderTypesInclude )]

    [ReminderTypesField(
        "Reminder Types Exclude",
        Description = "Select group types to exclude from this block. Note that this setting is only effective if 'Reminder Types Include' has no specific group types selected.",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.ReminderTypesExclude )]

    [BooleanField(
        "Show Filters",
        Description = "Select this option if you want the block to show filters for reminders.",
        DefaultBooleanValue = true,
        Order = 4,
        Key = AttributeKey.ShowFilters )]

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
            /// <summary>
            /// The Edit Reminder Page.
            /// </summary>
            public const string EditReminderPage = "EditReminderPage";

            /// <summary>
            /// The Reminder Types to Include.
            /// </summary>
            public const string ReminderTypesInclude = "ReminderTypesInclude";

            /// <summary>
            /// The Reminder Types to Exclude.
            /// </summary>
            public const string ReminderTypesExclude = "ReminderTypesExclude";

            /// <summary>
            /// Whether to Show Filters.
            /// </summary>
            public const string ShowFilters = "ShowFilters";
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

            /// <summary>
            /// The Completion Status.
            /// </summary>
            public const string CompletionStatus = "CompletionStatus";

            /// <summary>
            /// The Reminder Due Date.
            /// </summary>
            public const string Due = "Due";

            /// <summary>
            /// The Reminder Due Date Range (for Custom Date Range filter).
            /// </summary>
            public const string DueDateRange = "DueDateRange";
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

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Sets the allowed reminder types (from Block Attributes).
        /// </summary>
        private void SetAllowedReminderTypes()
        {
            var reminderTypeService = new ReminderTypeService( new RockContext() );

            hfReminderTypesInclude.Value = string.Empty;
            List<Guid> reminderTypeIncludeGuids = GetAttributeValue( AttributeKey.ReminderTypesInclude ).SplitDelimitedValues().AsGuidList();
            if ( reminderTypeIncludeGuids.Any() )
            {
                var reminderTypeIdIncludeList = new List<int>();
                foreach ( Guid guid in reminderTypeIncludeGuids )
                {
                    var reminderType = reminderTypeService.Get( guid );
                    if ( reminderType != null )
                    {
                        reminderTypeIdIncludeList.Add( reminderType.Id );
                    }
                }

                hfReminderTypesInclude.Value = reminderTypeIdIncludeList.AsDelimited( "," );
            }

            hfReminderTypesExclude.Value = string.Empty;
            List<Guid> reminderTypeExcludeGuids = GetAttributeValue( AttributeKey.ReminderTypesExclude ).SplitDelimitedValues().AsGuidList();
            if ( reminderTypeExcludeGuids.Any() )
            {
                var reminderTypeIdExcludeList = new List<int>();
                foreach ( Guid guid in reminderTypeExcludeGuids )
                {
                    var reminderType = reminderTypeService.Get( guid );
                    if ( reminderType != null )
                    {
                        reminderTypeIdExcludeList.Add( reminderType.Id );
                    }
                }

                hfReminderTypesExclude.Value = reminderTypeIdExcludeList.AsDelimited( "," );
            }
        }

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
        /// Gets the list of included reminder type ids.  This should only be called after setting the hidden field values (i.e., buy calling <see cref="SetAllowedReminderTypes"/>.
        /// </summary>
        /// <returns></returns>
        private List<int> GetIncludedReminderTypeIds()
        {
            var includeReminderTypes = hfReminderTypesInclude.Value.SplitDelimitedValues().AsIntegerList().Except( new List<int> { 0 } ).ToList();
            return includeReminderTypes;
        }

        /// <summary>
        /// Gets the list of excluded reminder type ids.  This should only be called after setting the hidden field values (i.e., buy calling <see cref="SetAllowedReminderTypes"/>.
        /// </summary>
        /// <returns></returns>
        private List<int> GetExcludedReminderTypeIds()
        {
            var excludeReminderTypes = hfReminderTypesExclude.Value.SplitDelimitedValues().AsIntegerList();
            return excludeReminderTypes;
        }

        /// <summary>
        /// Initialize the block.
        /// </summary>
        private void InitializeBlock()
        {
            var selectedEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();
            int? selectedEntityId = null;
            int? selectedReminderTypeId = null;

            var showFilters = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean();
            pnlFilterOptions.Visible = showFilters;
            if ( showFilters )
            {
                selectedEntityId = PageParameter( PageParameterKey.EntityId ).AsIntegerOrNull();
                selectedReminderTypeId = PageParameter( PageParameterKey.ReminderTypeId ).AsIntegerOrNull();
            }

            SetAllowedReminderTypes();
            var includedReminderTypeIds = GetIncludedReminderTypeIds();
            var excludedReminderTypeIds = GetExcludedReminderTypeIds();

            var entityTypes = new List<EntityType>();
            var reminderTypes = new List<ReminderType>();
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );

                entityTypes = reminderService.GetReminderEntityTypesByPerson( CurrentPersonId.Value, includedReminderTypeIds, excludedReminderTypeIds ).ToList();

                reminderTypes = reminderService.GetReminderTypesByPerson( selectedEntityTypeId, CurrentPerson );
                if ( includedReminderTypeIds.Any() )
                {
                    reminderTypes = reminderTypes.Where( t => includedReminderTypeIds.Contains( t.Id ) ).ToList();
                }
                else if ( excludedReminderTypeIds.Any() )
                {
                    reminderTypes = reminderTypes.Where( t => !excludedReminderTypeIds.Contains( t.Id ) ).ToList();
                }

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
                // This user only has a reminder for a single entity type, so set that as the selected entity type.
                selectedEntityTypeId = entityTypes[0].Id;
            }

            BindEntityTypeList( entityTypes );
            BindReminderList( selectedEntityTypeId, selectedEntityId, selectedReminderTypeId );
            SetReminderTypeFilter( reminderTypes, selectedReminderTypeId );

            if ( !selectedEntityTypeId.HasValue )
            {
                return;
            }

            if ( showFilters )
            {
                // Show Person Picker if EntityType is PersonAlias.
                var personAliasEntityTypeId = EntityTypeCache.GetId<PersonAlias>();
                if ( selectedEntityTypeId == personAliasEntityTypeId )
                {
                    lSelectedEntityType.Text = "People";
                    ppSelectedPerson.Visible = true;
                    if ( selectedEntityId.HasValue )
                    {
                        var person = new PersonAliasService( new RockContext() ).GetPerson( selectedEntityId.Value );
                        ppSelectedPerson.SetValue( person );
                    }
                    return;
                }

                // Show Group Picker if EntityType matches.
                var groupEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.GROUP.AsGuid() );
                if ( selectedEntityTypeId == groupEntityTypeId )
                {
                    lSelectedEntityType.Text = "Groups";
                    gpSelectedGroup.Visible = true;
                    gpSelectedGroup.SetValue( selectedEntityId );
                    return;
                }
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
            var boundEntityTypes = new List<EntityType>
            {
                new EntityType() { FriendlyName = "All Reminders", Id = 0 }
            };

            foreach ( var entityType in entityTypes )
            {
                if ( entityType.Id == EntityTypeCache.GetId<PersonAlias>() )
                {
                    // Show PersonAlias reminder filter as "People".
                    boundEntityTypes.Add( new EntityType() { FriendlyName = "People", Id = entityType.Id } );
                }
                else
                {
                    boundEntityTypes.Add( entityType );
                }
            }

            rptEntityTypeList.DataSource = boundEntityTypes;
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
            List<ReminderViewModel> reminders;
            if ( entityTypeId.HasValue && entityId.HasValue && entityTypeId.Value == EntityTypeCache.GetId<PersonAlias>() )
            {
                // Special logic is required here to ensure we get any reminders that are linked to previous PersonAliases.
                reminders = GetPersonReminders( entityTypeId.Value, entityId.Value, reminderTypeId );
            }
            else
            {
                reminders = GetReminders( entityTypeId, entityId, reminderTypeId );
            }

            if ( reminders.Count == 0 )
            {
                if ( lCompletionFilter.Text == "Complete")
                {
                    nbFilteredReminders.Text = "You have no completed reminders";
                }
                else
                {
                    nbFilteredReminders.Text = "You have no active reminders";
                }

                nbFilteredReminders.Visible = true;
            }
            else
            {
                nbFilteredReminders.Visible = true;
            }

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
        private List<ReminderViewModel> GetReminders( int? entityTypeId, int? entityId, int? reminderTypeId )
        {
            var reminderViewModels = new List<ReminderViewModel>();

            var completionFilter = PageParameter( PageParameterKey.CompletionStatus );
            var dueFilter = PageParameter( PageParameterKey.Due );

            var showFilters = GetAttributeValue( AttributeKey.ShowFilters ).AsBoolean();
            if ( !showFilters )
            {
                // If filters are not available to the user, enforce default settings.
                completionFilter = "Active";
                dueFilter = "Due";
            }

            if ( completionFilter.IsNullOrWhiteSpace() )
            {
                completionFilter = "Active";
            }

            if ( dueFilter.IsNullOrWhiteSpace() )
            {
                dueFilter = "Due";
            }

            var personAliasEntityTypeId = EntityTypeCache.GetId<PersonAlias>();

            var includedReminderTypeIds = GetIncludedReminderTypeIds();
            var excludedReminderTypeIds = GetExcludedReminderTypeIds();

            var rockContext = new RockContext();
            var reminderService = new ReminderService( rockContext );
            var reminders = reminderService.GetReminders( CurrentPersonId.Value, entityTypeId, entityId, reminderTypeId );

            // Filter by include/exclude block attribute.
            if ( includedReminderTypeIds.Any() )
            {
                reminders = reminders.Where( r => includedReminderTypeIds.Contains( r.ReminderTypeId ) );
            }
            else if ( excludedReminderTypeIds.Any() )
            {
                reminders = reminders.Where( r => !excludedReminderTypeIds.Contains( r.ReminderTypeId ) );
            }

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
                var selectedDateRange = PageParameter( PageParameterKey.DueDateRange );
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

            var invalidReminders = new List<Reminder>();
            var reminderEntities = reminderService.GetReminderEntities( reminders );

            foreach ( var reminder in reminders.ToList() )
            {
                var entity = reminderEntities.ContainsKey( reminder.Id ) ? reminderEntities[reminder.Id] : null;
                if ( entity == null )
                {
                    invalidReminders.Add( reminder );
                    continue;
                }

                string personProfilePhoto = string.Empty;
                if ( entity.TypeId == personAliasEntityTypeId )
                {
                    var person = ( entity as PersonAlias ).Person;
                    reminderViewModels.Add( new ReminderViewModel( reminder, person, Person.GetPersonPhotoUrl( person ) ) );
                }
                else
                {
                    reminderViewModels.Add( new ReminderViewModel( reminder, entity ) );
                }
            }

            if ( invalidReminders.Any() )
            {
                reminderService.DeleteRange( invalidReminders );
                rockContext.SaveChanges();
                NavigateToCurrentPageReference();
            }

            return reminderViewModels;
        }

        /// <summary>
        /// Gets the reminders for all PersonAliases belonging to the Person associated to a specific PersonAlias.
        /// </summary>
        /// <returns></returns>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="selectedPersonAliasId">The PersonAlias identifier.</param>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private List<ReminderViewModel> GetPersonReminders( int entityTypeId, int selectedPersonAliasId, int? reminderTypeId )
        {
            var reminders = new List<ReminderViewModel>();

            var person = new PersonAliasService( new RockContext() ).GetPerson( selectedPersonAliasId );
            foreach ( var personAlias in person.Aliases )
            {
                var remindersForAlias = GetReminders( entityTypeId, personAlias.Id, reminderTypeId );
                reminders.AddRange( remindersForAlias );
            }

            return reminders;
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
        private void RefreshPage( Dictionary<string, string> queryParameters )
        {
            var selectedEntityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();
            var selectedEntityId = PageParameter( PageParameterKey.EntityId ).AsIntegerOrNull();
            var selectedReminderTypeId = PageParameter( PageParameterKey.ReminderTypeId ).AsIntegerOrNull();
            RefreshPage( selectedEntityTypeId, selectedEntityId, selectedReminderTypeId, queryParameters );
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
        private void RefreshPage( int? entityTypeId, int? entityId, int? reminderTypeId, Dictionary<string, string> queryParameters = null )
        {
            queryParameters = queryParameters ?? new Dictionary<string, string>();

            if ( entityTypeId.HasValue && entityTypeId != 0 )
            {
                queryParameters.AddOrReplace( PageParameterKey.EntityTypeId, entityTypeId.ToString() );
            }

            if ( entityId.HasValue && entityId != 0 )
            {
                queryParameters.AddOrReplace( PageParameterKey.EntityId, entityId.ToString() );
            }

            if ( reminderTypeId.HasValue && reminderTypeId != 0 )
            {
                queryParameters.AddOrReplace( PageParameterKey.ReminderTypeId, reminderTypeId.ToString() );
            }

            // Keep any existing page parameters that we didn't modify.
            foreach ( var paramKey in PageParameters().Keys )
            {
                if ( paramKey != "PageId"
                    && paramKey != PageParameterKey.EntityTypeId
                    && paramKey != PageParameterKey.EntityId
                    && paramKey != PageParameterKey.ReminderTypeId
                    && !queryParameters.ContainsKey( paramKey ) )
                {
                    queryParameters.Add( paramKey, PageParameter( paramKey ) );
                }
            }

            NavigateToCurrentPage( queryParameters );
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

        /// <summary>
        /// Get the IconCss for the GroupType of a Group.
        /// </summary>
        /// <param name="groupId">The Group identifier.</param>
        /// <returns></returns>
        private string GetGroupTypeIconCss( int groupId )
        {
            string defaultGroupIconCss = "fa fa-users";

            using ( var rockContext = new RockContext() )
            {
                var groupType = GroupTypeCache.Get( groupId );

                if ( groupType == null )
                {
                    return defaultGroupIconCss;
                }

                if ( groupType.IconCssClass.IsNullOrWhiteSpace() )
                {
                    return defaultGroupIconCss;
                }

                return groupType.IconCssClass;
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
            var btnReminderTypeFilter_EntityType = e.Item.FindControl( "btnReminderTypeFilter_EntityType" ) as LinkButton;
            var reminderType = ( ReminderType ) e.Item.DataItem;
            btnReminderTypeFilter_EntityType.Text = reminderType.Name;
            btnReminderTypeFilter_EntityType.CommandArgument = reminderType.Id.ToString();
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
            var selectedEntityId = ppSelectedPerson.PersonAliasId ?? 0;
            RefreshPage( selectedEntityTypeId, selectedEntityId, selectedReminderTypeId );
        }

        /// <summary>
        /// Handles the Click event of the btnComplete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnComplete_Click( object sender, EventArgs e )
        {
            var btnComplete = sender as LinkButton;
            if (btnComplete == null )
            {
                return;
            }

            var reminderId = btnComplete.CommandArgument.AsIntegerOrNull();
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
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var btnEdit = sender as LinkButton;
            if ( btnEdit == null )
            {
                return;
            }

            var reminderId = btnEdit.CommandArgument.AsIntegerOrNull();
            if ( reminderId == null )
            {
                return;
            }

            EditReminder( reminderId.Value );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            var btnDelete = sender as LinkButton;
            if ( btnDelete == null )
            {
                return;
            }

            var reminderId = btnDelete.CommandArgument.AsIntegerOrNull();
            if ( reminderId == null )
            {
                return;
            }

            DeleteReminder( reminderId.Value );

            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the btnCompletionFilter controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCompletionFilter_Click( object sender, EventArgs e )
        {
            var btnCompletionFilter = sender as LinkButton;
            var queryParameters = new Dictionary<string, string>
            {
                { PageParameterKey.CompletionStatus, btnCompletionFilter.CommandArgument }
            };

            RefreshPage( queryParameters );
        }

        /// <summary>
        /// Handles the Click event of the btnReminderTypeFilter controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnReminderTypeFilter_Click( object sender, EventArgs e )
        {
            var btnReminderTypeFilter = sender as LinkButton;

            int? reminderTypeId = null;
            if ( btnReminderTypeFilter.CommandArgument != "All" )
            {
                reminderTypeId = int.Parse( btnReminderTypeFilter.CommandArgument );
            }

            RefreshPage( reminderTypeId );
        }

        /// <summary>
        /// Handles the Click event of the btnDueFilter controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDueFilter_Click( object sender, EventArgs e )
        {
            var btnDueFilter = sender as LinkButton;

            if ( btnDueFilter.CommandArgument == "Custom Date Range" )
            {
                drpCustomDate_SelectedDateRangeChanged( sender, e );
                return;
            }

            var queryParameters = new Dictionary<string, string>
            {
                { PageParameterKey.Due, btnDueFilter.CommandArgument }
            };

            RefreshPage( queryParameters );
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
                var queryParameters = new Dictionary<string, string>
                {
                    { PageParameterKey.Due, "Custom Date Range" },
                    { PageParameterKey.DueDateRange, drpCustomDate.DelimitedValues }
                };

                RefreshPage( queryParameters );
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptReminders controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptReminders_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var reminder = e.Item.DataItem as ReminderViewModel;

            if ( reminder.EntityUrl.IsNotNullOrWhiteSpace() )
            {
                var entityUrl = ResolveUrl( reminder.EntityUrl );
                var lEntity = e.Item.FindControl( "lEntity" ) as Literal;
                lEntity.Text = $"<a href=\"{entityUrl}\">{reminder.EntityDescription}</a>";
            }
            else
            {
                var lEntity = e.Item.FindControl( "lEntity" ) as Literal;
                lEntity.Text = $"{reminder.EntityDescription}";
            }

            if ( reminder.IsPersonReminder )
            {
                var lProfilePhoto = e.Item.FindControl( "lProfilePhoto" ) as Literal;
                lProfilePhoto.Visible = true;
                lProfilePhoto.Text = string.Format( lProfilePhoto.Text, reminder.PersonProfilePictureUrl );
            }
            else if ( reminder.IsGroupReminder )
            {
                var group = reminder.Entity as Group;
                var iconCss = GetGroupTypeIconCss( group.GroupTypeId );
                var lGroupIcon = e.Item.FindControl( "lGroupIcon" ) as Literal;
                lGroupIcon.Visible = true;
                lGroupIcon.Text = string.Format( lGroupIcon.Text, iconCss );
            }
        }

        #endregion Events
    }
}