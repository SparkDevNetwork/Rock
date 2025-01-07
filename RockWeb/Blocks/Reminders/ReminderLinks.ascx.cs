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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

using Rock.Web.UI;

namespace RockWeb.Blocks.Reminders
{
    [DisplayName( "Reminder Links" )]
    [Category( "Reminders" )]
    [Description( "This block is used to show reminder links." )]

    #region Block Attributes

    [LinkedPage(
        "View Reminders Page",
        Description = "The page where a person can view their reminders.",
        DefaultValue = Rock.SystemGuid.Page.REMINDER_LIST,
        Order = 0,
        Key = AttributeKey.ViewRemindersPage )]

    [LinkedPage(
        "Edit Reminder Page",
        Description = "The page where a person can edit a reminder.",
        DefaultValue = Rock.SystemGuid.Page.REMINDER_EDIT,
        Order = 1,
        Key = AttributeKey.EditReminderPage )]

    [LinkedPage(
        "View Notifications Page",
        Description = "The page where a person can view their notifications.",
        DefaultValue = Rock.SystemGuid.Page.NOTIFICATION_LIST,
        Order = 0,
        Key = AttributeKey.ViewNotificationsPage )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_LINKS )]
    public partial class ReminderLinks : RockBlock, IRockBlockType
    {
        #region Attribute Keys

        /// <summary>
        /// Keys for Block Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string ViewRemindersPage = "ViewRemindersPage";
            public const string EditReminderPage = "EditReminderPage";
            public const string ViewNotificationsPage = "ViewNotificationsPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string EntityTypeId = "EntityTypeId";
            public const string EntityId = "EntityId";
            public const string ReminderId = "ReminderId";
        }

        #endregion Page Parameter Keys

        #region IRockBlockType

        /// <inheritdoc/>
        int IRockBlockType.BlockId => ( ( IRockBlockType ) this ).BlockCache.Id;

        /// <inheritdoc/>
        BlockCache IRockBlockType.BlockCache { get; set; }

        /// <inheritdoc/>
        PageCache IRockBlockType.PageCache { get; set; }

        /// <inheritdoc/>
        RockRequestContext IRockBlockType.RequestContext { get; set; }

        /// <inheritdoc/>
        System.Threading.Tasks.Task<object> IRockBlockType.GetBlockInitializationAsync( RockClientType clientType )
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Base Control Methods

        /*
            10/20/2022 - SMC

            WARNING:  This block is loaded on every page of the internal site and any processing done in these
            methods should have minimal impact on the page load!  Do not include database calls, here.

            Database calls necessary to set up the page should be triggered only from interactive events (e.g.,
            on click) from specific controls.
        */

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlReminders );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( !CurrentPersonAliasId.HasValue )
                {
                    // If user is not logged in, do nothing.
                    base.OnLoad( e );
                    return;
                }

                btnReminders.Visible = true;
                ppPerson.SetValue( CurrentPerson );

                hfActionUrl.Value = $"/api/v2/BlockActions/{PageCache.Guid}/{BlockCache.Guid}/GetNotificationCounts";

                SetContextEntityType();
            }
            else
            {
                ShowDialog();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Sets the context entity type hidden field value so that the AJAX call from the front end
        /// can determine if the user has reminder types available for the entity type.
        /// </summary>
        private void SetContextEntityType()
        {
            hfContextEntityTypeId.Value = "0";

            var contextTypes = RockPage.GetScopedContextEntityTypes( ContextEntityScope.Page );
            if ( contextTypes.Any() )
            {
                var firstKey = contextTypes.Keys.First();
                var contextType = contextTypes[firstKey];
                if ( contextType == EntityTypeCache.Get<Person>() )
                {
                    // if the context entity is a Person, use PersonAlias instead.
                    hfContextEntityTypeId.Value = EntityTypeCache.GetId<PersonAlias>().ToString();
                }
                else
                {
                    hfContextEntityTypeId.Value = contextType.Id.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the first context entity for the page (excluding context from cookies).
        /// </summary>
        /// <returns></returns>
        private IEntity GetFirstContextEntity()
        {
            var contextEntities = RockPage.GetScopedContextEntities( ContextEntityScope.Page );
            if ( contextEntities.Any() )
            {
                var firstKey = contextEntities.Keys.First();
                var contextEntity = contextEntities[firstKey];

                if ( contextEntity is Person )
                {
                    // if the context entity is a Person, use PersonAlias instead.
                    var person = contextEntity as Person;
                    var personAlias = person.PrimaryAlias;
                    if ( personAlias == null )
                    {
                        personAlias = person.Aliases.FirstOrDefault();
                    }

                    contextEntity = personAlias;
                }

                return contextEntity;
            }

            return null;
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveReminderDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            switch ( hfActiveReminderDialog.Value )
            {
                case "ADDREMINDER":
                    mdAddReminder.Show();
                    break;
            }
        }

        /// <summary>
        /// Shows existing reminders for the current context entity when the Add Reminders modal is displayed.
        /// </summary>
        /// <param name="contextEntity">The context entity.</param>
        private void ShowExistingReminders( IEntity contextEntity )
        {
            btnViewReminders2.Visible = false;

            var reminders = GetReminders( contextEntity );
            if ( reminders.Count == 0 )
            {
                // No reminders to show.  Hide the panel and bail.
                pnlExistingReminders.Visible = false;
                return;
            }

            pnlExistingReminders.Visible = true;

            rptReminders.DataSource = reminders;
            rptReminders.DataBind();

            var entityTypeName = EntityTypeCache.Get( contextEntity.TypeId ).FriendlyName;
            if ( contextEntity.TypeId == EntityTypeCache.GetId<PersonAlias>() )
            {
                // Show "Person" instead of "Person Alias".
                entityTypeName = EntityTypeCache.Get<Person>().FriendlyName;
            }

            var reminderText = lExistingReminderTextTemplate.Text.Replace( "{ENTITY_TYPE}", entityTypeName );

            if ( reminders.Count == 1 )
            {
                reminderText = reminderText.Replace( "{REMINDER_QUANTITY_TEXT_1}", "a reminder" );
                reminderText = reminderText.Replace( "{REMINDER_QUANTITY_TEXT_2}", "recent is" );
                lExistingReminderText.Text = reminderText;
                return;
            }

            if ( reminders.Count >= 2 )
            {
                reminderText = reminderText.Replace( "{REMINDER_QUANTITY_TEXT_1}", "reminders" );
                reminderText = reminderText.Replace( "{REMINDER_QUANTITY_TEXT_2}", "recent 2 are" );
                lExistingReminderText.Text = reminderText;
                btnViewReminders2.Visible = ( reminders.Count > 2 );
                return;
            }
        }

        /// <summary>
        /// Gets the reminders.
        /// </summary>
        /// <returns></returns>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="reminderTypeId">The reminder type identifier.</param>
        private List<ReminderViewModel> GetReminders( IEntity contextEntity )
        {
            var reminderViewModels = new List<ReminderViewModel>();

            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );

                if ( contextEntity is PersonAlias personAlias )
                {
                    var personAliasService = new PersonAliasService( rockContext );
                    var personAliasIds = personAlias.Person.Aliases.Select( a => a.Id ).ToList();

                    var reminders = reminderService
                        .GetReminders( CurrentPersonId.Value, contextEntity.TypeId, null, null )
                        .Where( r => personAliasIds.Contains( r.EntityId ) && !r.IsComplete && r.ReminderDate < RockDateTime.Now ) // only get active reminders for this person.
                        .OrderByDescending( r => r.ReminderDate )
                        .Take( 2 ); // We're only interested in two reminders for this block.

                    foreach ( var reminder in reminders.ToList() )
                    {
                        var entity = personAliasService.Get( contextEntity.Id );
                        reminderViewModels.Add( new ReminderViewModel( reminder, entity ) );
                    }
                }
                else
                {
                    var entityTypeService = new EntityTypeService( rockContext );
                    var reminders = reminderService
                        .GetReminders( CurrentPersonId.Value, contextEntity.TypeId, contextEntity.Id, null )
                        .Where( r => !r.IsComplete && r.ReminderDate < RockDateTime.Now ) // only get active reminders.
                        .OrderByDescending( r => r.ReminderDate )
                        .Take( 2 ); // We're only interested in two reminders for this block.

                    foreach ( var reminder in reminders.ToList() )
                    {
                        var entity = entityTypeService.GetEntity( reminder.ReminderType.EntityTypeId, reminder.EntityId );
                        reminderViewModels.Add( new ReminderViewModel( reminder, entity ) );
                    }
                }
            }

            return reminderViewModels;
        }

        /// <summary>
        /// Mark a reminder complete.
        /// </summary>
        /// <param name="reminderId">The reminder identifier</param>
        private void MarkComplete( int reminderId )
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var reminder = reminderService.Get( reminderId );
                reminder.CompleteReminder();
                rockContext.SaveChanges();
            }

            UpdateExistingReminders();
        }

        /// <summary>
        /// Cancel reoccurrence for a reminder.
        /// </summary>
        /// <param name="reminderId">The reminder identifier</param>
        private void CancelReoccurrence( int reminderId )
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var reminder = reminderService.Get( reminderId );
                reminder.CancelReoccurrence();
                rockContext.SaveChanges();
            }

            UpdateExistingReminders();
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

            UpdateExistingReminders();
        }

        /// <summary>
        /// Resets the add reminder form to prepare it for a new use.
        /// </summary>
        /// <param name="contextEntity">The context entity.</param>
        private void ResetAddReminderForm( IEntity contextEntity )
        {
            using ( var rockContext = new RockContext() )
            {
                // Load context entity from the database to get the name.
                IEntity entity = new EntityTypeService( rockContext ).GetEntity( contextEntity.TypeId, contextEntity.Id );
                lEntity.Text = entity.ToString();

                // Load reminder types for this context entity.
                var reminderTypeService = new ReminderTypeService( rockContext );
                var reminderTypes = reminderTypeService.GetReminderTypesForEntityType( contextEntity.TypeId, CurrentPerson );
                ddlReminderType.DataSource = reminderTypes;
                ddlReminderType.DataTextField = "Name";
                ddlReminderType.DataValueField = "Id";
                ddlReminderType.DataBind();
            }

            // Reset form values.
            ddlReminderType.SelectedIndex = 0;
            dpReminderDate.SelectedDate = null;
            tbNote.Text = string.Empty;
            ppPerson.SetValue( CurrentPerson );
            numbRepeatDays.Text = string.Empty;
            numbRepeatTimes.Text = string.Empty;
        }

        /// <summary>
        /// Updates the existing reminder panel of the "add reminders" modal dialog.
        /// </summary>
        private void UpdateExistingReminders()
        {
            var contextEntity = GetFirstContextEntity();
            if ( contextEntity == null )
            {
                // This shouldn't be possible, since the button is only visible when the page has a context entity.
                NavigateToCurrentPageReference();
            }

            ShowExistingReminders( contextEntity );
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
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the btnAddReminder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddReminder_Click( object sender, EventArgs e )
        {
            var contextEntity = GetFirstContextEntity();
            if ( contextEntity == null )
            {
                // This shouldn't be possible, since the button is only visible when the page has a context entity.
                return;
            }

            ResetAddReminderForm( contextEntity );
            ShowExistingReminders( contextEntity );
            mdAddReminder.Title = $"Reminder For {contextEntity}";
            ShowDialog( "AddReminder" );
        }

        /// <summary>
        /// Handles the Click event of the btnViewReminders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnViewReminders_Click( object sender, EventArgs e )
        {
            var contextEntity = GetFirstContextEntity();
            if ( contextEntity == null )
            {
                NavigateToLinkedPage( AttributeKey.ViewRemindersPage );
                return;
            }

            NavigateToLinkedPage( AttributeKey.ViewRemindersPage );
        }

        /// <summary>
        /// Handles the Click event of the btnViewNotifications control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnViewNotifications_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.ViewNotificationsPage );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdAddReminder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdAddReminder_SaveClick( object sender, EventArgs e )
        {
            var contextEntity = GetFirstContextEntity();

            var reminder = new Reminder
            {
                EntityId = contextEntity.Id,
                ReminderTypeId = ddlReminderType.SelectedValue.AsInteger(),
                ReminderDate = dpReminderDate.SelectedDate.Value,
                Note = tbNote.Text,
                IsComplete = false,
                RenewPeriodDays = numbRepeatDays.IntegerValue,
                RenewMaxCount = numbRepeatTimes.IntegerValue,
                RenewCurrentCount = 0
            };

            using ( var rockContext = new RockContext() )
            {
                var person = CurrentPerson;
                if ( ppPerson.SelectedValue.HasValue )
                {
                    person = new PersonService( rockContext ).Get( ppPerson.SelectedValue.Value );
                }
                reminder.PersonAliasId = person.PrimaryAliasId.Value;

                var reminderService = new ReminderService( rockContext );
                reminderService.Add( reminder );
                rockContext.SaveChanges();
            }

            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the ItemCommand event for elements in the rptReminders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rptReminders_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var hfReminderId = e.Item.FindControl( "hfReminderId" ) as HiddenField;
            var reminderId = hfReminderId.ValueAsInt();
            if ( reminderId == 0 )
            {
                throw new Exception( "Unable to identify selected reminder." );
            }

            switch ( e.CommandName )
            {
                case "MarkComplete":
                    MarkComplete( reminderId );
                    break;
                case "CancelReoccurrence":
                    CancelReoccurrence( reminderId );
                    break;
                case "EditReminder":
                    EditReminder( reminderId );
                    break;
                case "DeleteReminder":
                    DeleteReminder( reminderId );
                    break;
            }
        }

        #endregion Events

        #region Block Actions

        /// <summary>
        /// Get the current notification counts for this person.
        /// </summary>
        /// <returns>An object that represents the counts.</returns>
        [BlockAction]
        public BlockActionResult GetNotificationCounts()
        {
            var rockBlock = ( IRockBlockType ) this;

            using ( var rockContext = new RockContext() )
            {
                var notificationMessageService = new NotificationMessageService( rockContext );
                var notificationCount = 0;

                if ( rockBlock.RequestContext.CurrentPerson != null )
                {
                    notificationCount = notificationMessageService.GetUnreadMessagesForPerson( rockBlock.RequestContext.CurrentPerson.Id, rockBlock.PageCache.Layout.Site ).Count();
                }

                return new BlockActionResult( System.Net.HttpStatusCode.OK, new
                {
                    Reminders = rockBlock.RequestContext.CurrentPerson?.ReminderCount ?? 0,
                    Notifications = notificationCount
                } );
            }
        }

        #endregion
    }
}