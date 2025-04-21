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
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;

namespace RockWeb.Blocks.Reminders
{
    [DisplayName( "Reminder Edit" )]
    [Category( "Reminders" )]
    [Description( "Block for editing reminders." )]

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_EDIT )]
    public partial class ReminderEdit : Rock.Web.UI.RockBlock
    {
        #region Page Parameter Keys

        private static class PageParameterKey
        {
            public const string EntityTypeId = "EntityTypeId";
            public const string EntityId = "EntityId";
            public const string ReminderId = "ReminderId";
        }

        #endregion Page Parameter Keys

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                InitializeBlock();
            }

            base.OnLoad( e );
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Initialize the block.
        /// </summary>
        private void InitializeBlock()
        {
            var reminderId = PageParameter( PageParameterKey.ReminderId ).AsInteger();
            var entityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsInteger();
            var entityId = PageParameter( PageParameterKey.EntityId ).AsInteger();
            if ( reminderId == 0 && ( entityTypeId == 0 || entityId == 0 ) )
            {
                NavigateToParentPage();
                return;
            }

            if ( reminderId != 0 )
            {
                LoadExistingReminder( reminderId );
                return;
            }

            // This is a new reminder.
            cbComplete.Visible = false;
            using ( var rockContext = new RockContext() )
            {
                IEntity entity = new EntityTypeService( rockContext ).GetEntity( entityTypeId, entityId );
                lEntity.Text = entity.ToString();

                var reminderTypeService = new ReminderTypeService( rockContext );
                var reminderTypes = reminderTypeService.GetReminderTypesForEntityType( entityTypeId, CurrentPerson );
                BindReminderTypes( reminderTypes );
            }

            ppPerson.SetValue( CurrentPerson );
        }

        /// <summary>
        /// Display the values of an existing reminder (edit mode).
        /// </summary>
        /// <param name="reminderId">The reminder identifier.</param>
        private void LoadExistingReminder( int reminderId )
        {
            hfReminderId.Value = reminderId.ToString();
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var reminder = reminderService.Get( reminderId );

                IEntity entity = new EntityTypeService( rockContext ).GetEntity( reminder.ReminderType.EntityTypeId, reminder.EntityId );
                lEntity.Text = entity.ToString();

                var reminderTypeService = new ReminderTypeService( rockContext );
                var reminderTypes = reminderTypeService.GetReminderTypesForEntityType( reminder.ReminderType.EntityTypeId, CurrentPerson );
                BindReminderTypes( reminderTypes );

                ddlReminderType.SelectedValue = reminder.ReminderTypeId.ToString();
                dpReminderDate.SelectedDate = reminder.ReminderDate;
                cbComplete.Checked = reminder.IsComplete;
                tbNote.Text = reminder.Note;

                numbRepeatDays.IntegerValue = reminder.RenewPeriodDays;
                numbRepeatTimes.IntegerValue = reminder.RenewMaxCount;

                ppPerson.SetValue( reminder.PersonAlias.Person );
            }

        }

        /// <summary>
        /// Bind the reminder types dropdown list.
        /// </summary>
        /// <param name="reminderTypes">The reminder types.</param>
        private void BindReminderTypes( List<ReminderType> reminderTypes )
        {
            ddlReminderType.DataSource = reminderTypes;
            ddlReminderType.DataTextField = "Name";
            ddlReminderType.DataValueField = "Id";
            ddlReminderType.DataBind();
        }

        /// <summary>
        /// Create a new reminder.
        /// </summary>
        private void CreateNewReminder()
        {
            var reminder = new Reminder
            {
                EntityId = PageParameter( PageParameterKey.EntityId ).AsInteger(),
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
                var person = new PersonService( rockContext ).Get( ppPerson.SelectedValue.Value );
                reminder.PersonAliasId = person.PrimaryAliasId.Value;

                var reminderService = new ReminderService( rockContext );
                reminderService.Add( reminder );
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Update and save an existing reminder.
        /// </summary>
        /// <param name="reminderId">The reminder identifier.</param>
        private void UpdateExistingReminder( int reminderId )
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var reminder = reminderService.Get( reminderId );

                IEntity entity = new EntityTypeService( rockContext ).GetEntity( reminder.ReminderType.EntityTypeId, reminder.EntityId );

                reminder.ReminderTypeId = ddlReminderType.SelectedValue.AsInteger();
                reminder.ReminderDate = dpReminderDate.SelectedDate.Value;
                reminder.Note = tbNote.Text;
                reminder.RenewPeriodDays = numbRepeatDays.IntegerValue;
                reminder.RenewMaxCount = numbRepeatTimes.IntegerValue;

                if ( reminder.IsComplete && !cbComplete.Checked )
                {
                    reminder.ResetCompletedReminder();
                }
                else if ( !reminder.IsComplete && cbComplete.Checked )
                {
                    reminder.CompleteReminder();
                }

                var person = new PersonService( rockContext ).Get( ppPerson.SelectedValue.Value );
                reminder.PersonAliasId = person.PrimaryAliasId.Value;

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
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var reminderId = hfReminderId.Value.AsInteger();
            if ( reminderId == 0 )
            {
                CreateNewReminder();
            }
            else
            {
                UpdateExistingReminder( reminderId );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion Events
    }
}