﻿// <copyright>
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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;

using System;
using System.Linq;
using System.ComponentModel;
using Rock.Common.Mobile.Blocks.Reminders;
using Rock.Common.Mobile.Blocks.Reminders.ReminderEdit;
using Rock.Web.Cache;
using Rock.Common.Mobile.ViewModel;

namespace Rock.Blocks.Types.Mobile.Reminders
{
    /// <summary>
    /// A block used to add/edit reminders.
    /// </summary>
    [DisplayName( "Reminder Edit" )]
    [Category( "Reminders" )]
    [Description( "Allows adding/editing of reminders." )]
    [IconCssClass( "fa fa-edit" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [CodeEditorField( "Header Template",
        Description = "Lava template used to render the header above the reminder edit fields.",
        IsRequired = false,
        EditorMode = Rock.Web.UI.Controls.CodeEditorMode.Xml,
        Key = AttributeKey.HeaderTemplate,
        DefaultValue = _defaultHeaderXaml,
        Order = 0 )]

    [MobileNavigationActionField( "Save Navigation Action",
        Description = "The action to perform after the reminder is saved.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.PopSinglePageValue,
        Key = AttributeKey.SaveNavigationAction,
        Order = 1 )]

    #endregion

    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.MOBILE_REMINDERS_REMINDER_EDIT )]
    [SystemGuid.BlockTypeGuid( SystemGuid.BlockType.MOBILE_REMINDERS_REMINDER_EDIT )]
    public class ReminderEdit : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The header template attribute key.
            /// </summary>
            public const string HeaderTemplate = "HeaderTemplate";

            /// <summary>
            /// The save navigation action.
            /// </summary>
            public const string SaveNavigationAction = "SaveNavigationAction";
        }

        #endregion

        #region Constants

        /// <summary>
        /// The default header XAML.
        /// </summary>
        private const string _defaultHeaderXaml = @"<StackLayout>
    <StackLayout.Resources>
        <Rock:AllTrueMultiValueConverter x:Key=""AllTrueConverter"" /> 
    </StackLayout.Resources>

    <!-- If this is a person entity and no entity is selected -->
    <Grid RowDefinitions=""*""
        ColumnDefinitions=""64, *"">
        <Grid.IsVisible>
            <MultiBinding Converter=""{StaticResource AllTrueConverter}"">
                <Binding Path=""Reminder.IsPersonEntityType"" />
                <Binding Path=""Reminder.IsEntitySelected""
                    Converter=""{Rock:InverseBooleanConverter}"" />
            </MultiBinding>
        </Grid.IsVisible>

        <!-- The add icon -->
        <Frame WidthRequest=""64""
            HeightRequest=""64""
            CornerRadius=""32""
            HasShadow=""false""
            StyleClass=""bg-light, p-0""
            Grid.Column=""0"">
            <Rock:Icon IconClass=""fa-plus""
                IconFamily=""FontAwesomeSolid""
                FontSize=""24""
                VerticalTextAlignment=""Center""
                HorizontalTextAlignment=""Center"" />
        </Frame>

        <!-- Our select individual label -->
        <Label Text=""Select Individual""
            Grid.Column=""1""
            VerticalOptions=""Center""
            StyleClass=""title"" />

        <Grid.GestureRecognizers>
            <TapGestureRecognizer Command=""{Binding ShowPersonSearch}"" />
        </Grid.GestureRecognizers>
    </Grid>

    <!-- If an entity is selected -->
    <Grid RowDefinitions=""*""
        ColumnDefinitions=""64, *""
        IsVisible=""{Binding Reminder.IsEntitySelected}"">

        <!-- The bell icon -->
        <Frame WidthRequest=""64""
            HeightRequest=""64""
            CornerRadius=""32""
            HasShadow=""false""
            StyleClass=""bg-light, p-0""
            Grid.Column=""0"">
            <Rock:Icon IconClass=""fa-bell""
                IconFamily=""FontAwesomeSolid""
                FontSize=""24""
                VerticalTextAlignment=""Center""
                HorizontalTextAlignment=""Center"" />
        </Frame>

        <!-- The person information -->
        <StackLayout Grid.Column=""1"" 
            Spacing=""0""
            VerticalOptions=""Center"">
            <Label Text=""New Reminder""
                StyleClass=""text-gray-600, text-sm"" />
            <Label Text=""{Binding Reminder.Name}""
                StyleClass=""title"" />
        </StackLayout>
    </Grid>
</StackLayout>";

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        /// <summary>
        /// Gets the mobile configuration values.
        /// </summary>
        /// <returns></returns>
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Reminders.ReminderEdit.Configuration
            {
                HeaderTemplate = GetAttributeValue( AttributeKey.HeaderTemplate ),
                SaveNavigationAction = GetAttributeValue( AttributeKey.SaveNavigationAction ).FromJsonOrNull<MobileNavigationActionViewModel>() ?? new MobileNavigationActionViewModel()
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a new reminder.
        /// </summary>
        /// <param name="reminderTypeGuid">The reminder type unique identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="reminderDate">The reminder date.</param>
        /// <param name="note">The note.</param>
        /// <param name="renewPeriodDays">The renew period days.</param>
        /// <param name="renewMaxCount">The renew maximum count.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>This method should only be called when there is a Current Person in the request.</remarks>
        private void CreateReminder( Guid reminderTypeGuid, Guid entityGuid, DateTime reminderDate, string note, int? renewPeriodDays, int? renewMaxCount, RockContext rockContext )
        {
            var reminderService = new ReminderService( rockContext );
            var reminderType = new ReminderTypeService( rockContext ).Get( reminderTypeGuid );

            if ( reminderType == null )
            {
                return;
            }

            var entityId = Reflection.GetEntityIdForEntityType( reminderType.EntityType.Id, entityGuid, rockContext );
            if ( entityId == null )
            {
                return;
            }

            //
            // Create a new reminder.
            //
            var reminder = new Reminder
            {
                EntityId = entityId.Value,
                ReminderTypeId = reminderType.Id,
                ReminderDate = reminderDate,
                Note = note,
                IsComplete = false,
                RenewPeriodDays = renewPeriodDays,
                RenewMaxCount = renewMaxCount,
                RenewCurrentCount = 0
            };

            var person = RequestContext.CurrentPerson;
            reminder.PersonAliasId = person.PrimaryAliasId.Value;
            reminderService.Add( reminder );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Updates an existing reminder.
        /// </summary>
        /// <param name="reminderGuid">The reminder unique identifier.</param>
        /// <param name="reminderTypeGuid">The reminder type unique identifier.</param>
        /// <param name="reminderDate">The reminder date.</param>
        /// <param name="note">The note.</param>
        /// <param name="renewPeriodDays">The renew period days.</param>
        /// <param name="renewMaxCount">The renew maximum count.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>This method should only be called when there is a Current Person in the request.</remarks>
        private void UpdateReminder( Guid reminderGuid, Guid reminderTypeGuid, DateTime reminderDate, string note, int? renewPeriodDays, int? renewMaxCount, RockContext rockContext )
        {
            var reminderService = new ReminderService( rockContext );
            var reminder = reminderService.Get( reminderGuid );

            var reminderTypeId = new ReminderTypeService( rockContext ).GetId( reminderTypeGuid );
            reminder.ReminderTypeId = reminderTypeId.Value;
            reminder.ReminderDate = reminderDate;
            reminder.Note = note;
            reminder.RenewPeriodDays = renewPeriodDays;
            reminder.RenewMaxCount = renewMaxCount;

            var person = RequestContext.CurrentPerson;
            reminder.PersonAliasId = person.PrimaryAliasId.Value;

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Populates the additional properties for reminder information bag.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="personAliasService">The person service.</param>
        private void PopulateAdditionalPropertiesForReminderInfoBag( ReminderInfoBag bag, PersonAliasService personAliasService )
        {
            if( bag == null )
            {
                return;
            }

            var entityType = EntityTypeCache.Get( bag.EntityTypeGuid );

            string name = "";

            // If this is a Person, use the Person properties.
            if ( entityType != null && entityType.Guid == Rock.SystemGuid.EntityType.PERSON_ALIAS.AsGuid() )
            {
                var personAlias = personAliasService.Get( bag.EntityGuid );
                name = personAlias.Person.FullName;
            }
            else if( entityType != null && entityType.Guid == Rock.SystemGuid.EntityType.CONNECTION_REQUEST.AsGuid() )
            {
                var connectionRequest = new ConnectionRequestService( new RockContext() ).Get( bag.EntityGuid );

                var connectionRequestText = connectionRequest.ConnectionOpportunity?.Name ?? string.Empty;

                if( connectionRequest.PersonAlias.Person.FullName.IsNotNullOrWhiteSpace() )
                {
                    connectionRequestText += $" - {connectionRequest.PersonAlias.Person.FullName}";
                }

                name = connectionRequestText;
            }

            // Otherwise, use the first letter of the entity type.
            else
            {

                if ( bag.EntityGuid != null )
                {
                    name = Reflection.GetIEntityForEntityType( entityType.GetEntityType(), bag.EntityGuid ).ToStringSafe();
                }
            }

            bag.Name = name;
        }

        /// <summary>
        /// Returns a list of reminder types and (optionally) pre-existing reminder data.
        /// </summary>
        /// <param name="reminderGuid"></param>
        /// <param name="entityTypeGuid"></param>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        private ResponseBag GetReminderEditBag( Guid? reminderGuid, Guid? entityTypeGuid, Guid? entityGuid = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var personAliasService = new PersonAliasService( rockContext );
                var reminderTypeService = new ReminderTypeService( rockContext );

                // If a reminder guid was provided, load our reminder data.
                ReminderInfoBag reminderBag = null;

                if ( reminderGuid.HasValue )
                {
                    var reminderService = new ReminderService( rockContext );
                    var reminder = reminderService.Get( reminderGuid.Value );

                    // Get the EntityTypeGuid and EntityGuid for this reminder.
                    var reminderEntityType = EntityTypeCache.Get( reminder.ReminderType.EntityTypeId );
                    var reminderEntityGuid = Reflection.GetEntityGuidForEntityType( reminderEntityType.Id, reminder.EntityId.ToStringSafe() );

                    reminderBag = new ReminderInfoBag
                    {
                        ReminderDate = reminder.ReminderDate,
                        Note = reminder.Note,
                        ReminderTypeGuid = reminder.ReminderType.Guid,
                        RenewMaxCount = reminder.RenewMaxCount,
                        RenewPeriodDays = reminder.RenewPeriodDays,
                        EntityGuid = reminderEntityGuid.Value,
                        EntityTypeGuid = reminderEntityType.Guid
                    };

                    // We can assume the entity type guid from the reminder type at this point.
                    if ( !entityTypeGuid.HasValue )
                    {
                        entityTypeGuid = EntityTypeCache.GetGuid( reminder.ReminderType.EntityTypeId );
                    }
                }
                else if ( entityTypeGuid.HasValue && entityGuid.HasValue )
                {
                    reminderBag = new ReminderInfoBag
                    {
                        EntityGuid = entityGuid.Value,
                        EntityTypeGuid = entityTypeGuid.Value
                    };
                }

                PopulateAdditionalPropertiesForReminderInfoBag( reminderBag, personAliasService );

                //
                // Load the applicable reminder types for this entity type and person.
                //
                var reminderTypes = reminderTypeService.GetReminderTypesForEntityType( entityTypeGuid.Value, RequestContext.CurrentPerson )
                    .Select( rt => new ReminderTypeInfoBag
                    {
                        Guid = rt.Guid,
                        EntityTypeGuid = rt.EntityType.Guid,
                        Name = rt.Name
                    } ).ToList();

                // Return a list of reminder types and (optionally) the reminder
                // data of the reminder we're editing.
                return new ResponseBag
                {
                    ReminderTypes = reminderTypes,
                    Reminder = reminderBag
                };
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the reminder edit data.
        /// </summary>
        /// <param name="reminderGuid">The reminder unique identifier.</param>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetReminderEditData( Guid? reminderGuid, Guid? entityTypeGuid, Guid? entityGuid = null )
        {

            // If there wasn't a provided reminder (that we're editing) or an entity type (for a new reminder)
            // this is incorrect.
            if ( reminderGuid == null && !( entityTypeGuid.HasValue ) )
            {
                return ActionBadRequest();
            }

            // We need a Person to add a reminder.
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized();
            }

            return ActionOk( GetReminderEditBag( reminderGuid, entityTypeGuid, entityGuid ) );
        }

        /// <summary>
        /// Creates or updates a reminder based on the request.
        /// </summary>
        /// <param name="reminderGuid">The optional Guid to pass in of the reminder to edit.</param>
        /// <param name="reminderBag"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult CreateOrUpdateReminder( Guid? reminderGuid, ReminderInfoBag reminderBag )
        {
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionForbidden();
            }

            using ( var rockContext = new RockContext() )
            {
                // If we have an existing reminder, update that.
                if ( reminderGuid.HasValue )
                {
                    UpdateReminder( reminderGuid.Value, reminderBag.ReminderTypeGuid, reminderBag.ReminderDate.DateTime, reminderBag.Note, reminderBag.RenewPeriodDays, reminderBag.RenewMaxCount, rockContext );
                }
                // Otherwise, create a new reminder.
                else
                {
                    CreateReminder( reminderBag.ReminderTypeGuid, reminderBag.EntityGuid, reminderBag.ReminderDate.DateTime, reminderBag.Note, reminderBag.RenewPeriodDays, reminderBag.RenewMaxCount, rockContext );
                }

                return ActionOk();
            }
        }

        #endregion
    }
}
