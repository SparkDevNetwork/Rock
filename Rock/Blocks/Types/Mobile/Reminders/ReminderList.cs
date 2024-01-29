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
using Rock.Common.Mobile.Blocks.Reminders;
using Rock.Common.Mobile.Blocks.Reminders.ReminderList;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

namespace Rock.Blocks.Types.Mobile.Reminders
{
    /// <summary>
    /// A block used to show existing reminders for a current person.
    /// </summary>
    [DisplayName( "Reminder List" )]
    [Category( "Reminders" )]
    [Description( "Allows management of the current person's reminders." )]
    [IconCssClass( "fa fa-list-alt" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    [LinkedPage(
        "Reminder Edit Page",
        Description = "The page where a person can edit a reminder.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.ReminderEditPage )]

    [ReminderTypesField(
        "Reminder Types Include",
        Description = "Select any specific reminder types to show in this block. Leave all unchecked to show all active reminder types ( except for excluded reminder types ).",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.ReminderTypesInclude )]

    [ReminderTypesField(
        "Reminder Types Exclude",
        Description = "Select group types to exclude from this block. Note that this setting is only effective if 'Reminder Types Include' has no specific group types selected.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.ReminderTypesExclude )]

    [IntegerField(
        "Completion Display Delay",
        Description = "The amount of time after a reminder is marked complete to delay before removing it from the UI (in MS).",
        IsRequired = false,
        Order = 3,
        Key = AttributeKey.CompletionDisplayDelay,
        DefaultIntegerValue = 5000 )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_REMINDERS_REMINDER_LIST )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_REMINDERS_REMINDER_LIST )]
    public class ReminderList : RockBlockType
    {
        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Reminders.ReminderList.Configuration
            {
                EditPageGuid = GetAttributeValue( AttributeKey.ReminderEditPage ).AsGuidOrNull(),
                CompletionDisplayDelay = GetAttributeValue( AttributeKey.CompletionDisplayDelay ).AsIntegerOrNull(),
            };
        }

        #endregion

        #region Constants

        /// <summary>
        /// Keys for Block Attributes.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The Edit Reminder Page.
            /// </summary>
            public const string ReminderEditPage = "ReminderEditPage";

            /// <summary>
            /// The Reminder Types to Include.
            /// </summary>
            public const string ReminderTypesInclude = "ReminderTypesInclude";

            /// <summary>
            /// The Reminder Types to Exclude.
            /// </summary>
            public const string ReminderTypesExclude = "ReminderTypesExclude";

            /// <summary>
            /// The completion display delay attribute key.
            /// </summary>
            public const string CompletionDisplayDelay = "CompletionDisplayDelay";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a number of reminder bags based on the start index and count.
        /// </summary>
        /// <param name="personGuid">The person unique identifier.</param>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="entityGuid">The entity unique identifier.</param>
        /// <param name="reminderTypeGuid">The reminder type unique identifier.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="count">The count.</param>
        /// <param name="excludedReminderTypes"></param>
        /// <param name="filter">The filter.</param>
        /// <returns>List&lt;ReminderInfoBag&gt;.</returns>
        private List<ReminderInfoBag> GetReminderBags( Guid personGuid, Guid? entityTypeGuid, Guid? entityGuid, Guid? reminderTypeGuid, int startIndex, int count, List<Guid> excludedReminderTypes, FilterBag filter = null )
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var personAliasService = new PersonAliasService( rockContext );
                var reminders = reminderService.GetReminders( personGuid, entityTypeGuid, entityGuid, reminderTypeGuid );

                // If this block was specified to only include certain reminder types, limit to those.
                var reminderTypeIncludeGuids = GetAttributeValue( AttributeKey.ReminderTypesInclude ).SplitDelimitedValues().AsGuidList();
                if ( reminderTypeIncludeGuids.Any() )
                {
                    reminders = reminders.Where( r => reminderTypeIncludeGuids.Contains( r.ReminderType.Guid ) );
                }

                // If this block was specified to exclude certain reminder types, exclude those.
                var reminderTypeExcludeGuids = GetAttributeValue( AttributeKey.ReminderTypesExclude ).SplitDelimitedValues().AsGuidList();
                reminderTypeExcludeGuids.AddRange( excludedReminderTypes );

                if ( reminderTypeExcludeGuids.Any() )
                {
                    reminders = reminders.Where( r => !reminderTypeExcludeGuids.Contains( r.ReminderType.Guid ) );
                }

                // Filter our reminders based on the Filter Bag provided.
                var filteredReminders = FilterReminders( filter, reminders )
                    .ToList();


                // Order by modified date time if we're limiting to complete reminders.
                if ( filter.CompletionFilter == FilterBag.CompletionFilterValue.Complete )
                {
                    filteredReminders = filteredReminders.OrderByDescending( r => r.ModifiedDateTime )
                        .ToList();
                }
                else
                {
                    filteredReminders = filteredReminders.OrderByDescending( r => r.Id )
                        .ToList();
                }

                // Convert reminders into reminder bags.
                var reminderBags = filteredReminders.Select( r => new ReminderInfoBag
                {
                    Guid = r.Guid,
                    Note = r.Note,
                    ReminderTypeName = r.ReminderType.Name,
                    ReminderTypeGuid = r.ReminderType.Guid,
                    ReminderDate = r.ReminderDate,
                    EntityTypeName = r.ReminderType.EntityType.FriendlyName,
                    IsComplete = r.IsComplete,
                    EntityTypeGuid = EntityTypeCache.GetGuid( r.ReminderType.EntityTypeId ).Value,
                    EntityGuid = Reflection.GetEntityGuidForEntityType( r.ReminderType.EntityType.Id, r.EntityId.ToStringSafe(), true, rockContext ).Value
                } );

                List<ReminderInfoBag> reminderBagList = new List<ReminderInfoBag>();

                // We do this so we can support querying sequential data.
                if ( startIndex >= 0 && count > 0 )
                {
                    reminderBags = reminderBags
                    .Skip( startIndex )
                    .Take( count );
                }

                reminderBagList = reminderBags.ToList();

                // We need some more data post query (such as a specific photo url generated based on the entity)
                // so let's loop over our bags and populate those properties.
                reminderBagList.ForEach( ( bag ) => LoadAdditionalReminderData( bag, personAliasService ) );

                return reminderBagList;
            }
        }

        /// <summary>
        /// Populates the additional properties for reminder information bag.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="personAliasService">The person service.</param>
        private void LoadAdditionalReminderData( ReminderInfoBag bag, PersonAliasService personAliasService )
        {
            var entityType = EntityTypeCache.Get( bag.EntityTypeGuid );

            string name = "", path;

            // If this is a Person, use the Person properties.
            if ( entityType != null && entityType.Guid == Rock.SystemGuid.EntityType.PERSON_ALIAS.AsGuid() )
            {
                var personAlias = personAliasService.Get( bag.EntityGuid );
                path = personAlias.Person.PhotoUrl;
                name = personAlias.Person.FullName;

                bag.EntityDetailParameters = new Dictionary<string, object>
                {
                    { "PersonGuid", personAlias.Person.Guid }
                };
            }
            // Otherwise, use the first letter of the entity type.
            else
            {
                path = $"/GetAvatar.ashx?text={bag.EntityTypeName.SubstringSafe( 0, 1 )}";

                if ( bag.EntityGuid != null )
                {
                    name = Reflection.GetIEntityForEntityType( entityType.GetEntityType(), bag.EntityGuid ).ToStringSafe();
                }
            }

            bag.PhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( path );
            bag.Name = name;
        }

        /// <summary>
        /// Filters the reminders from the filter bag values.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="reminders">The reminders.</param>
        /// <returns>IQueryable&lt;Reminder&gt;.</returns>
        private IQueryable<Reminder> FilterReminders( FilterBag filter, IQueryable<Reminder> reminders )
        {
            if ( filter == null )
            {
                return reminders;
            }

            //
            // The 'Completion' Filter.
            //
            if ( filter.CompletionFilter != FilterBag.CompletionFilterValue.None )
            {
                // Filter by active reminders.
                if ( filter.CompletionFilter == FilterBag.CompletionFilterValue.Active )
                {
                    reminders = reminders.Where( r => !r.IsComplete && r.ReminderDate <= RockDateTime.Now );
                }
                // Filter by complete reminders.
                else if ( filter.CompletionFilter == FilterBag.CompletionFilterValue.Complete )
                {
                    reminders = reminders.Where( r => r.IsComplete );
                }
                else if ( filter.CompletionFilter == FilterBag.CompletionFilterValue.Incomplete )
                {
                    reminders = reminders.Where( r => !r.IsComplete );
                }
            }

            //
            // The 'Due' Filter.
            //
            if ( filter.DueFilter != FilterBag.DueFilterValue.None )
            {
                // Reminders that are due this month.
                if ( filter.DueFilter == FilterBag.DueFilterValue.DueThisMonth )
                {
                    var startOfMonth = RockDateTime.Now.StartOfMonth();
                    var nextMonthDate = RockDateTime.Now.AddMonths( 1 );
                    var nextMonthStartDate = new DateTime( nextMonthDate.Year, nextMonthDate.Month, 1 );
                    reminders = reminders.Where( r => r.ReminderDate >= startOfMonth && r.ReminderDate < nextMonthStartDate );
                }
                // Reminders that are due this week.
                else if ( filter.DueFilter == FilterBag.DueFilterValue.DueThisWeek )
                {
                    var nextWeekStartDate = RockDateTime.Now.EndOfWeek( RockDateTime.FirstDayOfWeek ).AddDays( 1 );
                    var startOfWeek = nextWeekStartDate.AddDays( -7 );
                    reminders = reminders.Where( r => r.ReminderDate >= startOfWeek && r.ReminderDate < nextWeekStartDate );
                }
                // Reminders that are past due.
                else if ( filter.DueFilter == FilterBag.DueFilterValue.Due )
                {
                    var currentDate = RockDateTime.Now;
                    reminders = reminders.Where( r => r.ReminderDate <= currentDate );
                }
            }

            //
            // The 'Start Date' filter.
            //
            if ( filter.StartDate.HasValue )
            {
                var startDate = filter.StartDate.Value.DateTime;
                reminders = reminders.Where( r => r.ReminderDate >= startDate );
            }

            //
            // The 'End Date' filter.
            //
            if ( filter.EndDate.HasValue )
            {
                var endDate = filter.EndDate.Value.DateTime;
                reminders = reminders.Where( r => r.ReminderDate < endDate );
            }

            //
            // The 'Reminder Type' filter.
            //
            if ( filter.ReminderType.HasValue )
            {
                var reminderTypeId = new ReminderTypeService( new RockContext() ).GetId( filter.ReminderType.Value );
                reminders = reminders.Where( r => r.ReminderTypeId == reminderTypeId );
            }

            return reminders;
        }

        /// <summary>
        /// Delete a reminder.
        /// </summary>
        /// <param name="reminderGuid">The reminder GUID.</param>
        private static void DeleteReminderInternal( Guid reminderGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var reminder = reminderService.Get( reminderGuid );
                reminderService.Delete( reminder );
                rockContext.SaveChanges();
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the reminders.
        /// </summary>
        /// <param name="requestBag"></param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult GetReminders( RequestBag requestBag )
        {
            var reminders = GetReminderBags( requestBag.PersonGuid, requestBag.EntityTypeGuid, requestBag.EntityGuid, requestBag.ReminderTypeGuid, requestBag.StartIndex, requestBag.Count, requestBag.ExcludedReminderTypes, requestBag.Filter );

            return ActionOk( reminders );
        }

        /// <summary>
        /// Sets the reminder as complete or incomplete based on the parameters provided.
        /// </summary>
        /// <param name="reminderGuid">The reminder unique identifier.</param>
        /// <param name="complete">if set to <c>true</c> [complete].</param>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public BlockActionResult SetReminderCompletion( Guid reminderGuid, bool complete )
        {
            using ( var rockContext = new RockContext() )
            {
                var reminderService = new ReminderService( rockContext );
                var reminder = reminderService.Get( reminderGuid );

                if ( reminder == null )
                {
                    return ActionNotFound();
                }

                if ( !reminder.IsComplete && complete )
                {
                    reminder.CompleteReminder();
                }
                else if ( reminder.IsComplete && !complete )
                {
                    reminder.ResetCompletedReminder();
                }

                rockContext.SaveChanges();
            }

            return ActionOk();
        }

        /// <summary>
        /// Deletes a specific reminder.
        /// </summary>
        /// <param name="reminderGuid"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult DeleteReminder( Guid reminderGuid )
        {
            DeleteReminderInternal( reminderGuid );

            return ActionOk();
        }

        #endregion
    }
}
