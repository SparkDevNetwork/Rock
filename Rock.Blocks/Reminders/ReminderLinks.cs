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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks.Reminders.ReminderLinks;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Reminders
{
    /// <summary>
    /// Renders the reminder/notification bell icon in the page chrome, plus the
    /// dropdown menu (Add Reminder / View Reminders / View Notifications) and
    /// the Add Reminder modal. Loaded on every internal page.
    /// </summary>
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
        Order = 2,
        Key = AttributeKey.ViewNotificationsPage )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.System )]
    [Rock.SystemGuid.EntityTypeGuid( "27551A62-9C3A-44FD-A5A3-404D338A0323" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "56F1AB7D-F1A4-4EBB-A2E2-0055E89AB899" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.REMINDER_LINKS )]
    public class ReminderLinks : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ViewRemindersPage = "ViewRemindersPage";
            public const string EditReminderPage = "EditReminderPage";
            public const string ViewNotificationsPage = "ViewNotificationsPage";
        }

        private static class NavigationUrlKey
        {
            public const string ViewRemindersPage = "ViewRemindersPage";
            public const string EditReminderPage = "EditReminderPage";
            public const string ViewNotificationsPage = "ViewNotificationsPage";
        }

        private static class PageParameterKey
        {
            public const string ReminderId = "ReminderId";
        }

        #endregion Keys

        #region Constants

        /// <summary>
        /// localStorage key the Vue component uses to cache the bell counts
        /// across page navigations. Matches the legacy key exactly so cached
        /// state survives the cutover deploy.
        /// </summary>
        private const string CountsLocalStorageKey = "Rock.Core.ReminderLinks.Counts";

        /// <summary>
        /// Number of existing reminders to render above the Add Reminder form.
        /// The query itself pulls one extra so the client can decide whether to
        /// show the "see all" link.
        /// </summary>
        private const int ExistingRemindersDisplayCount = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Cached initialization box so <see cref="GetObsidianBlockInitialization"/>
        /// and <see cref="GetInitialHtmlContent"/> observe identical data when
        /// both are invoked during the same request.
        /// </summary>
        private ReminderLinksInitializationBox _initBox;

        #endregion Fields

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetInitializationBox();
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            /*
                4/17/2026 - MSE

                Server-renders the bell icon shell so it's present in the initial
                HTML before Vue mounts. Reason: this block lives in page chrome and
                re-renders on every navigation; if the bell were produced only by
                the Vue component, every click would show a visible flicker in the
                header while Vue booted.
            */

            var box = GetInitializationBox();

            if ( !box.IsBlockVisible )
            {
                return string.Empty;
            }

            return "<div class=\"dropdown js-rock-reminders\"><a class=\"rock-bookmark\" href=\"#\"><i class=\"ti ti-bell\"></i></a></div>";
        }

        /// <summary>
        /// Builds the initialization box, caching the result for reuse within
        /// the same request.
        /// </summary>
        /// <returns>The initialization box.</returns>
        private ReminderLinksInitializationBox GetInitializationBox()
        {
            if ( _initBox != null )
            {
                return _initBox;
            }

            var box = new ReminderLinksInitializationBox
            {
                CountsLocalStorageKey = CountsLocalStorageKey,
                NavigationUrls = new Dictionary<string, string>
                {
                    [NavigationUrlKey.ViewRemindersPage] = this.GetLinkedPageUrl( AttributeKey.ViewRemindersPage ),
                    [NavigationUrlKey.EditReminderPage] = this.GetLinkedPageUrl( AttributeKey.EditReminderPage ),
                    [NavigationUrlKey.ViewNotificationsPage] = this.GetLinkedPageUrl( AttributeKey.ViewNotificationsPage )
                }
            };

            var currentPerson = GetCurrentPerson();

            if ( currentPerson?.PrimaryAliasId == null )
            {
                _initBox = box;
                return _initBox;
            }

            box.IsBlockVisible = true;

            var contextEntity = ResolveContextEntity();
            if ( contextEntity != null )
            {
                box.ContextEntityTypeId = contextEntity.TypeId;

                // Pre-computed so the Add Reminder menu item renders correctly
                // on the first dropdown open without an extra round trip.
                box.CanAddReminder = new ReminderTypeService( RockContext )
                    .GetReminderTypesForEntityType( contextEntity.TypeId, currentPerson )
                    .Any();
            }

            _initBox = box;
            return _initBox;
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Returns the current reminder and notification counts. Called on a
        /// polling interval by the Vue component and written back to localStorage
        /// so the bell indicator stays warm across page navigations.
        /// </summary>
        /// <returns>A <see cref="ReminderLinksCountsBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetNotificationCounts()
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            return ActionOk( BuildCounts( currentPerson ) );
        }

        /// <summary>
        /// Returns everything the Add Reminder modal needs to render: reminder
        /// types for the current context entity, top existing reminders,
        /// pre-substituted header text, and refreshed counts.
        /// </summary>
        /// <returns>A <see cref="ReminderLinksContextDataBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetReminderLinksData()
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            return ActionOk( BuildContextData( currentPerson ) );
        }

        /// <summary>
        /// Creates a new reminder for the current context entity.
        /// </summary>
        /// <param name="bag">The submitted form values.</param>
        /// <returns>A refreshed <see cref="ReminderLinksContextDataBag"/>.</returns>
        [BlockAction]
        public BlockActionResult SaveReminder( SaveReminderRequestBag bag )
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson?.PrimaryAliasId == null )
            {
                return ActionUnauthorized();
            }

            if ( bag == null )
            {
                return ActionBadRequest( "Missing request body." );
            }

            if ( !bag.ReminderDate.HasValue )
            {
                return ActionBadRequest( "Reminder date is required." );
            }

            var contextEntity = ResolveContextEntity();
            if ( contextEntity == null )
            {
                return ActionBadRequest( "Context entity is no longer available." );
            }

            // Guard against a forged ReminderTypeId — validate against the set
            // of reminder types the current person is actually authorized to use
            // for this entity type.
            var allowedReminderTypeIds = new ReminderTypeService( RockContext )
                .GetReminderTypesForEntityType( contextEntity.TypeId, currentPerson )
                .Select( t => t.Id )
                .ToList();

            if ( !allowedReminderTypeIds.Contains( bag.ReminderTypeId ) )
            {
                return ActionBadRequest( "Invalid reminder type." );
            }

            var assigneePersonAliasId = currentPerson.PrimaryAliasId.Value;
            if ( bag.PersonAliasGuid.HasValue && bag.PersonAliasGuid.Value != Guid.Empty )
            {
                var assigneeAlias = new PersonAliasService( RockContext ).Get( bag.PersonAliasGuid.Value );
                if ( assigneeAlias == null )
                {
                    return ActionBadRequest( "Invalid person." );
                }
                assigneePersonAliasId = assigneeAlias.Id;
            }

            var reminder = new Reminder
            {
                EntityId = contextEntity.Id,
                ReminderTypeId = bag.ReminderTypeId,
                ReminderDate = bag.ReminderDate.Value,
                Note = bag.Note,
                IsComplete = false,
                RenewPeriodDays = bag.RepeatDays,
                RenewMaxCount = bag.RepeatTimes,
                RenewCurrentCount = 0,
                PersonAliasId = assigneePersonAliasId
            };

            var reminderService = new ReminderService( RockContext );
            reminderService.Add( reminder );
            RockContext.SaveChanges();

            return ActionOk( BuildContextData( currentPerson ) );
        }

        /// <summary>
        /// Marks the specified reminder complete and returns refreshed context data.
        /// </summary>
        /// <param name="reminderIdKey">The hashed reminder identifier.</param>
        /// <returns>A refreshed <see cref="ReminderLinksContextDataBag"/>.</returns>
        [BlockAction]
        public BlockActionResult MarkReminderComplete( string reminderIdKey )
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            var reminder = GetOwnedReminder( reminderIdKey, currentPerson );
            if ( reminder == null )
            {
                return ActionBadRequest( "Invalid reminder." );
            }

            reminder.CompleteReminder();
            RockContext.SaveChanges();

            return ActionOk( BuildContextData( currentPerson ) );
        }

        /// <summary>
        /// Cancels reoccurrence on the specified reminder and returns refreshed context data.
        /// </summary>
        /// <param name="reminderIdKey">The hashed reminder identifier.</param>
        /// <returns>A refreshed <see cref="ReminderLinksContextDataBag"/>.</returns>
        [BlockAction]
        public BlockActionResult CancelReminderReoccurrence( string reminderIdKey )
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            var reminder = GetOwnedReminder( reminderIdKey, currentPerson );
            if ( reminder == null )
            {
                return ActionBadRequest( "Invalid reminder." );
            }

            reminder.CancelReoccurrence();
            RockContext.SaveChanges();

            return ActionOk( BuildContextData( currentPerson ) );
        }

        /// <summary>
        /// Deletes the specified reminder and returns refreshed context data.
        /// </summary>
        /// <param name="reminderIdKey">The hashed reminder identifier.</param>
        /// <returns>A refreshed <see cref="ReminderLinksContextDataBag"/>.</returns>
        [BlockAction]
        public BlockActionResult DeleteReminder( string reminderIdKey )
        {
            var currentPerson = GetCurrentPerson();

            if ( currentPerson == null )
            {
                return ActionUnauthorized();
            }

            var reminder = GetOwnedReminder( reminderIdKey, currentPerson );
            if ( reminder == null )
            {
                return ActionBadRequest( "Invalid reminder." );
            }

            var reminderService = new ReminderService( RockContext );
            reminderService.Delete( reminder );
            RockContext.SaveChanges();

            return ActionOk( BuildContextData( currentPerson ) );
        }

        #endregion Block Actions

        #region Private Helpers

        /// <summary>
        /// Returns the first scoped context entity for the current page, swapping
        /// a Person for its PrimaryAlias so reminders are always anchored to an alias.
        /// </summary>
        /// <returns>The context entity, or null when none is set.</returns>
        private IEntity ResolveContextEntity()
        {
            var resolvedContextTypes = RequestContext.GetContextEntityTypes();

            foreach ( var contextTypeName in PageCache.PageContexts.Keys )
            {
                var contextType = resolvedContextTypes.FirstOrDefault( t => t.FullName == contextTypeName );
                if ( contextType == null )
                {
                    continue;
                }

                var entity = RequestContext.GetContextEntity( contextType );
                if ( entity == null )
                {
                    continue;
                }

                if ( entity is Person person )
                {
                    return person.PrimaryAlias ?? person.Aliases?.FirstOrDefault();
                }

                return entity;
            }

            return null;
        }

        /// <summary>
        /// Gets the <see cref="Reminder"/> with the given hashed id and verifies
        /// it belongs to the current person. Returns null for any failure so the
        /// caller can respond with a generic "invalid reminder" error regardless
        /// of whether the id was missing, unhashable, or not owned — avoids
        /// leaking which reminders exist.
        /// </summary>
        /// <param name="reminderIdKey">The hashed reminder identifier.</param>
        /// <param name="currentPerson">The logged-in person.</param>
        /// <returns>The reminder or null.</returns>
        private Reminder GetOwnedReminder( string reminderIdKey, Person currentPerson )
        {
            if ( reminderIdKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var reminderId = IdHasher.Instance.GetId( reminderIdKey );
            if ( !reminderId.HasValue )
            {
                return null;
            }

            var reminder = new ReminderService( RockContext )
                .Queryable()
                .Include( r => r.PersonAlias )
                .FirstOrDefault( r => r.Id == reminderId.Value );

            if ( reminder == null || reminder.PersonAlias?.PersonId != currentPerson.Id )
            {
                return null;
            }

            return reminder;
        }

        /// <summary>
        /// Gets the current counts for the bell indicator and dropdown badges.
        /// </summary>
        /// <param name="currentPerson">The logged-in person.</param>
        /// <returns>A <see cref="ReminderLinksCountsBag"/>.</returns>
        private ReminderLinksCountsBag BuildCounts( Person currentPerson )
        {
            var remindersCount = new PersonService( RockContext ).Queryable()
                .AsNoTracking()
                .Where( p => p.Id == currentPerson.Id )
                .Select( p => p.ReminderCount )
                .FirstOrDefault() ?? 0;

            var notificationCount = new NotificationMessageService( RockContext )
                .GetUnreadMessagesForPerson( currentPerson.Id, PageCache.Layout.Site )
                .Count();

            return new ReminderLinksCountsBag
            {
                Reminders = remindersCount,
                Notifications = notificationCount
            };
        }

        /// <summary>
        /// Builds the full context-data payload (reminder types, existing reminders,
        /// header text, counts, modal title) for the Add Reminder modal.
        /// </summary>
        /// <param name="currentPerson">The logged-in person.</param>
        /// <returns>A <see cref="ReminderLinksContextDataBag"/>.</returns>
        private ReminderLinksContextDataBag BuildContextData( Person currentPerson )
        {
            var currentPersonPrimaryAliasGuid = new PersonAliasService( RockContext ).GetPrimaryAliasGuid( currentPerson.Id );

            var bag = new ReminderLinksContextDataBag
            {
                Counts = BuildCounts( currentPerson ),
                CurrentPerson = new ListItemBag
                {
                    Value = currentPersonPrimaryAliasGuid?.ToString(),
                    Text = currentPerson.FullName
                },
                ReminderTypes = new List<ListItemBag>(),
                ExistingReminders = new List<ExistingReminderBag>(),
                ExistingReminderText = string.Empty
            };

            var contextEntity = ResolveContextEntity();
            if ( contextEntity == null )
            {
                return bag;
            }

            var contextEntityType = EntityTypeCache.Get( contextEntity.TypeId );
            var displayEntityType = contextEntityType?.Id == EntityTypeCache.GetId<PersonAlias>()
                ? EntityTypeCache.Get<Person>()
                : contextEntityType;
            var entityTypeFriendlyName = displayEntityType?.FriendlyName;

            bag.EntityDescription = contextEntity.ToString();

            var reminderTypes = new ReminderTypeService( RockContext )
                .GetReminderTypesForEntityType( contextEntity.TypeId, currentPerson );

            bag.ReminderTypes = reminderTypes
                .Select( t => new ListItemBag
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                } )
                .ToList();

            bag.CanAddReminder = reminderTypes.Any();

            var existingReminders = GetExistingReminders( contextEntity, currentPerson );

            bag.ExistingReminders = existingReminders;
            bag.ExistingReminderText = BuildExistingReminderText( existingReminders.Count, entityTypeFriendlyName );

            return bag;
        }

        /// <summary>
        /// Returns the current person's active (past-due, incomplete) reminders
        /// for the given context entity, ordered newest first and capped at three
        /// — two for display plus one extra the client uses to decide whether to
        /// render the "see all" link.
        /// </summary>
        /// <param name="contextEntity">The context entity.</param>
        /// <param name="currentPerson">The logged-in person.</param>
        /// <returns>The existing reminders.</returns>
        private List<ExistingReminderBag> GetExistingReminders( IEntity contextEntity, Person currentPerson )
        {
            var reminderService = new ReminderService( RockContext );

            IQueryable<Reminder> query;

            if ( contextEntity is PersonAlias personAlias )
            {
                // Get all of the current person's alias ids so we catch reminders
                // attached to any of them.
                var personAliasIds = new PersonAliasService( RockContext ).Queryable()
                    .Where( a => a.PersonId == personAlias.PersonId )
                    .Select( a => a.Id )
                    .ToList();

                query = reminderService
                    .GetReminders( currentPerson.Id, contextEntity.TypeId, null, null )
                    .Where( r => personAliasIds.Contains( r.EntityId ) );
            }
            else
            {
                query = reminderService
                    .GetReminders( currentPerson.Id, contextEntity.TypeId, contextEntity.Id, null );
            }

            var now = RockDateTime.Now;

            return query
                .Include( r => r.ReminderType )
                .Where( r => !r.IsComplete && r.ReminderDate < now )
                .OrderByDescending( r => r.ReminderDate )
                .Take( ExistingRemindersDisplayCount + 1 )
                .AsNoTracking()
                .ToList()
                .Select( r => new ExistingReminderBag
                {
                    IdKey = r.IdKey,
                    ReminderDate = r.ReminderDate.ToShortDateString(),
                    Note = r.Note,
                    ReminderTypeName = r.ReminderType?.Name,
                    HighlightColor = r.ReminderType?.HighlightColor,
                    IsRenewing = r.IsRenewing
                } )
                .ToList();
        }

        /// <summary>
        /// Builds the Existing Reminders header sentence. Empty string when
        /// there are no existing reminders.
        /// </summary>
        /// <param name="totalActive">The total active reminder count.</param>
        /// <param name="entityTypeName">The entity type friendly name.</param>
        /// <returns>The fully-substituted sentence.</returns>
        private static string BuildExistingReminderText( int totalActive, string entityTypeName )
        {
            if ( totalActive <= 0 )
            {
                return string.Empty;
            }

            var quantityPhrase = totalActive == 1 ? "a reminder" : "reminders";
            var recentPhrase = totalActive == 1 ? "recent is" : "recent 2 are";
            var safeTypeName = entityTypeName.IsNullOrWhiteSpace() ? "item" : entityTypeName;

            return $"You currently have {quantityPhrase} for this {safeTypeName}. The most {recentPhrase} listed below.";
        }

        #endregion Private Helpers
    }
}
