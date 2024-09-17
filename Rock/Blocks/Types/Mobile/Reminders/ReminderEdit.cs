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

    [BooleanField( "Show Assigned To",
        Description = "Whether to show the assigned to field. Otherwise defaults to the Current Person.",
        IsRequired = false,
        DefaultBooleanValue = false,
        Key = AttributeKey.ShowAssignedTo,
        Order = 2 )]

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

            /// <summary>
            /// Whether to show the assigned to field.
            /// </summary>
            public const string ShowAssignedTo = "ShowAssignedTo";
        }

        /// <summary>
        /// The page parameter keys for this block.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The linked note guid page parameter key.
            /// </summary>
            public const string LinkedNoteGuid = "LinkedNoteGuid";

            /// <summary>
            /// The linked note type guid page parameter key.
            /// </summary>
            public const string LinkedNoteTypeGuid = "LinkedNoteTypeGuid";
        }

        #endregion

        #region Constants

        /// <summary>
        /// The default header XAML.
        /// </summary>
        private const string _defaultHeaderXaml = @"<Rock:StyledBorder StyleClass=""p-16, rounded, border, border-interface-soft, bg-interface-softest"">
    <StackLayout>
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
            <Rock:StyledBorder WidthRequest=""48""
                HeightRequest=""48""
                CornerRadius=""24""
                StyleClass=""bg-light, bg-primary-soft""
                Grid.Column=""0"">
                <Rock:Icon IconClass=""fa-plus""
                    IconFamily=""FontAwesomeSolid""
                    FontSize=""24""
                    StyleClass=""text-primary-strong""
                    VerticalTextAlignment=""Center""
                    HorizontalTextAlignment=""Center"" />
            </Rock:StyledBorder>
    
            <!-- Our select individual label -->
            <Label Text=""Select Individual""
                Grid.Column=""1""
                VerticalOptions=""Center""
                StyleClass=""title, text-interface-stronger, body, bold"" />
    
            <Grid.GestureRecognizers>
                <TapGestureRecognizer Command=""{Binding ShowPersonSearch}"" />
            </Grid.GestureRecognizers>
        </Grid>
    
        <!-- If an entity is selected -->
        <Grid RowDefinitions=""*""
            ColumnDefinitions=""64, *""
            IsVisible=""{Binding Reminder.IsEntitySelected}"">
    
            <!-- The bell icon -->
            <Rock:StyledBorder WidthRequest=""48""
                HeightRequest=""48""
                CornerRadius=""24""
                StyleClass=""bg-light, bg-primary-soft""
                Grid.Column=""0"">
                <Rock:Icon IconClass=""fa-bell""
                    IconFamily=""FontAwesomeSolid""
                    FontSize=""24""
                    StyleClass=""text-primary-strong""
                    VerticalTextAlignment=""Center""
                    HorizontalTextAlignment=""Center"" />
            </Rock:StyledBorder>
    
            <!-- The person information -->
            <StackLayout Grid.Column=""1"" 
                VerticalOptions=""Center"">
                <Label Text=""New Reminder""
                    StyleClass=""text-gray-600, text-sm, text-interface-strong, callout"" />
                <Label Text=""{Binding Reminder.Name}""
                    StyleClass=""title, text-interface-stronger, body, bold"" />
            </StackLayout>
        </Grid>
    </StackLayout>
</Rock:StyledBorder>";

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
                SaveNavigationAction = GetAttributeValue( AttributeKey.SaveNavigationAction ).FromJsonOrNull<MobileNavigationActionViewModel>() ?? new MobileNavigationActionViewModel(),
                ShowAssignTo = GetAttributeValue( AttributeKey.ShowAssignedTo ).AsBoolean()
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
        /// <param name="assignedToPrimaryAliasGuid">The person this reminder should be assigned to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>This method should only be called when there is a Current Person in the request.</remarks>
        private int? CreateReminder( Guid reminderTypeGuid, Guid entityGuid, DateTime reminderDate, string note, int? renewPeriodDays, int? renewMaxCount, Guid? assignedToPrimaryAliasGuid, RockContext rockContext )
        {
            var reminderService = new ReminderService( rockContext );
            var reminderType = new ReminderTypeService( rockContext ).Get( reminderTypeGuid );

            if ( reminderType == null )
            {
                return null;
            }

            var entityId = Reflection.GetEntityIdForEntityType( reminderType.EntityType.Id, entityGuid, rockContext );
            if ( entityId == null )
            {
                return null;
            }

            // Create a new reminder.
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

            if ( GetAttributeValue( AttributeKey.ShowAssignedTo ).AsBoolean() )
            {
                var assignedToPersonAlias = new PersonAliasService( rockContext ).GetId( assignedToPrimaryAliasGuid.Value );

                if ( assignedToPersonAlias.HasValue )
                {
                    reminder.PersonAliasId = assignedToPersonAlias.Value;
                }
            }
            else
            {
                var person = RequestContext.CurrentPerson;
                reminder.PersonAliasId = person.PrimaryAliasId.Value;
            }

            reminderService.Add( reminder );
            rockContext.SaveChanges();

            return reminder.Id;
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
        /// <param name="assignedToPersonAliasGuid">The person this reminder should be assigned to.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <remarks>This method should only be called when there is a Current Person in the request.</remarks>
        private int? UpdateReminder( Guid reminderGuid, Guid reminderTypeGuid, DateTime reminderDate, string note, int? renewPeriodDays, int? renewMaxCount, Guid? assignedToPersonAliasGuid, RockContext rockContext )
        {
            var reminderService = new ReminderService( rockContext );
            var reminder = reminderService.Get( reminderGuid );

            if ( reminder == null )
            {
                return null;
            }

            var reminderTypeId = new ReminderTypeService( rockContext ).GetId( reminderTypeGuid );
            reminder.ReminderTypeId = reminderTypeId.Value;
            reminder.ReminderDate = reminderDate;
            reminder.Note = note;
            reminder.RenewPeriodDays = renewPeriodDays;
            reminder.RenewMaxCount = renewMaxCount;

            if ( GetAttributeValue( AttributeKey.ShowAssignedTo ).AsBoolean() )
            {
                var assignedToPersonAlias = new PersonAliasService( rockContext ).GetId( assignedToPersonAliasGuid.Value );

                if ( assignedToPersonAlias.HasValue )
                {
                    reminder.PersonAliasId = assignedToPersonAlias.Value;
                }
            }
            else
            {
                var person = RequestContext.CurrentPerson;
                reminder.PersonAliasId = person.PrimaryAliasId.Value;
            }

            rockContext.SaveChanges();
            return reminder.Id;
        }

        /// <summary>
        /// Links a reminder to a note.
        /// </summary>
        /// <param name="linkedNoteGuid">The GUID of the note to link the reminder to.</param>
        /// <param name="linkedNoteTypeGuid">The Note Type to update the note to.</param>
        /// <param name="reminderId">The reminder to link the note to.</param>
        /// <param name="rockContext">The Rock Context.</param>
        private void LinkReminderToNote( Guid linkedNoteGuid, Guid linkedNoteTypeGuid, int reminderId, RockContext rockContext )
        {
            var noteService = new NoteService( rockContext );
            var note = noteService.Get( linkedNoteGuid );
            var noteType = NoteTypeCache.Get( linkedNoteTypeGuid );

            if ( note == null || noteType == null )
            {
                return;
            }

            note.EntityId = reminderId;
            note.NoteTypeId = noteType.Id;

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Populates the additional properties for reminder information bag.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="personAliasService">The person service.</param>
        private void PopulateAdditionalPropertiesForReminderInfoBag( ReminderInfoBag bag, PersonAliasService personAliasService )
        {
            if ( bag == null )
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
            else if ( entityType != null && entityType.Guid == Rock.SystemGuid.EntityType.CONNECTION_REQUEST.AsGuid() )
            {
                var connectionRequest = new ConnectionRequestService( new RockContext() ).Get( bag.EntityGuid );

                var connectionRequestText = connectionRequest.ConnectionOpportunity?.Name ?? string.Empty;

                if ( connectionRequest.PersonAlias.Person.FullName.IsNotNullOrWhiteSpace() )
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
                var linkedNoteGuid = RequestContext.GetPageParameter( PageParameterKey.LinkedNoteGuid ).AsGuidOrNull();
                var linkedNoteTypeGuid = RequestContext.GetPageParameter( PageParameterKey.LinkedNoteTypeGuid ).AsGuidOrNull();
                int? reminderId;

                // If we have an existing reminder, update that.
                if ( reminderGuid.HasValue )
                {
                    reminderId = UpdateReminder( reminderGuid.Value, reminderBag.ReminderTypeGuid, reminderBag.ReminderDate.DateTime, reminderBag.Note, reminderBag.RenewPeriodDays, reminderBag.RenewMaxCount, reminderBag.AssignedToPrimaryAliasGuid, rockContext );
                }
                // Otherwise, create a new reminder.
                else
                {
                    reminderId = CreateReminder( reminderBag.ReminderTypeGuid, reminderBag.EntityGuid, reminderBag.ReminderDate.DateTime, reminderBag.Note, reminderBag.RenewPeriodDays, reminderBag.RenewMaxCount, reminderBag.AssignedToPrimaryAliasGuid, rockContext );
                }

                if ( reminderId == null )
                {
                    return ActionBadRequest( "Failed to create or update your reminder." );
                }

                // If we have a linked note, link it to the reminder.
                if ( linkedNoteGuid.HasValue && linkedNoteTypeGuid.HasValue )
                {
                    LinkReminderToNote( linkedNoteGuid.Value, linkedNoteTypeGuid.Value, reminderId.Value, rockContext );
                }

                return ActionOk();
            }
        }

        #endregion
    }
}