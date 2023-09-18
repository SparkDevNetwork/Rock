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
using System.ComponentModel;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.Linq;
using System.Collections.Generic;
using Rock.Blocks.Types.Mobile.Connection;
using Rock.Common.Mobile.Blocks.Reminders.ReminderDashboard;
using Rock.Common.Mobile.Blocks.Reminders;
using Rock.Common.Mobile.Blocks.Reminders.ReminderList;
using System;

namespace Rock.Blocks.Types.Mobile.Reminders
{
    /// <summary>
    /// A mobile block used to display information about
    /// existing reminders for a person.
    /// </summary>
    [DisplayName( "Reminder Dashboard" )]
    [Category( "Reminders" )]
    [Description( "Allows management of the current person's reminders." )]
    [IconCssClass( "fa fa-bell" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [LinkedPage(
        "Reminder List Page",
        Description = "Page to link to when user taps on a reminder type or reminder filter card. PersonGuid is passed in the query string, as well as corresponding filter parameters.",
        IsRequired = false,
        Key = AttributeKey.ReminderListPage,
        Order = 0 )]

    [LinkedPage(
        "Reminder Edit Page",
        Description = "The page where a person can edit a reminder.",
        Order = 1,
        IsRequired = false,
        Key = AttributeKey.ReminderEditPage )]

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
        "Enable Color Pair Calculation",
        Description = "If enabled, the associated foreground and background color of the reminder type will undergo calculations to ensure readability. This is initially based on the 'HighlightColor' of the reminder type.",
        DefaultBooleanValue = true,
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.EnableColorPairCalculation )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_REMINDERS_REMINDER_DASHBOARD )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_REMINDERS_REMINDER_DASHBOARD )]
    public class ReminderDashboard : RockBlockType
    {
        #region Keys

        /// <summary>
        /// The block setting attribute keys for the <see cref="ConnectionTypeList"/> block.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The reminder list page attribute key.
            /// </summary>
            public const string ReminderListPage = "ReminderListPage";

            /// <summary>
            /// The reminder types include attribute key.
            /// </summary>
            public const string ReminderTypesInclude = "ReminderTypesInclude";

            /// <summary>
            /// The reminder types exclude attribute key.
            /// </summary>
            public const string ReminderTypesExclude = "ReminderTypesExclude";

            /// <summary>
            /// The edit reminder page attribute key.
            /// </summary>
            public const string ReminderEditPage = "EditReminderPage";

            /// <summary>
            /// The enable color pair calculation attribute key.
            /// </summary>
            public const string EnableColorPairCalculation = "EnableColorPairCalculation";
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 5 );

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Reminders.ReminderDashboard.Configuration
            {
                ListPageGuid = GetAttributeValue( AttributeKey.ReminderListPage ).AsGuidOrNull(),
                EditPageGuid = GetAttributeValue( AttributeKey.ReminderEditPage ).AsGuidOrNull(),
                EnableColorPairCalculation = GetAttributeValue( AttributeKey.EnableColorPairCalculation ).AsBoolean()
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a list of <see cref="ReminderTypeInfoBag" /> objects that
        /// we use to display information in the mobile shell.
        /// </summary>
        /// <returns></returns>
        private List<ReminderTypeInfoBag> GetReminderTypes( RockContext rockContext )
        {
            if ( RequestContext?.CurrentPerson?.PrimaryAliasId == null )
            {
                return null;
            }

            var reminderTypeService = new ReminderTypeService( rockContext );
            var personAliasId = RequestContext.CurrentPerson.PrimaryAliasId.Value;

            //
            // Get the specific reminder type information (and the count of total reminders).
            //
            var reminderTypesAndRemindersGrouping = reminderTypeService.GetTypesAndRemindersAssignedToPerson( personAliasId );

            // If this block was specified to only include certain reminder types, limit to those.
            var reminderTypeIncludeGuids = GetAttributeValue( AttributeKey.ReminderTypesInclude ).SplitDelimitedValues().AsGuidList();
            if ( reminderTypeIncludeGuids.Any() )
            {
                reminderTypesAndRemindersGrouping = reminderTypesAndRemindersGrouping.Where( r => reminderTypeIncludeGuids.Contains( r.Key.Guid ) );
            }

            // If this block was specified to exclude certain reminder types, exclude those.
            var reminderTypeExcludeGuids = GetAttributeValue( AttributeKey.ReminderTypesExclude ).SplitDelimitedValues().AsGuidList();
            if ( reminderTypeExcludeGuids.Any() )
            {
                reminderTypesAndRemindersGrouping = reminderTypesAndRemindersGrouping.Where( r => !reminderTypeExcludeGuids.Contains( r.Key.Guid ) );
            }

            // Convert into bags.
            var reminderTypeBags = reminderTypesAndRemindersGrouping.Select( group => new ReminderTypeInfoBag
            {
                TotalReminderCount = group.Where( r => !r.IsComplete ).Count(), // The count of total reminders in the group.
                Guid = group.Key.Guid,
                HighlightColor = group.Key.HighlightColor,
                Name = group.Key.Name,
                EntityTypeName = group.Key.EntityType.FriendlyName,
            } ).ToList();

            return reminderTypeBags;
        }

        /// <summary>
        /// Gets information about filtering options for reminder types.
        /// This is a pretty specific use case where we want to display the filter option
        /// and then # of reminders that the filtered type would provide
        /// </summary>
        /// <param name="rockContext"></param>
        /// <returns></returns>
        private List<FilteredReminderOptionBag> GetFilteredReminderOptionBags( RockContext rockContext )
        {
            var reminders = new ReminderService( rockContext )
                .GetReminders( RequestContext.CurrentPerson.Id, null, null, null );

            var filteredReminderOptionBag = new List<FilteredReminderOptionBag>
            {
                //
                // The 'Due' filtered reminder option.
                //
                new FilteredReminderOptionBag
                {
                    Name = "Due",
                    CssClass = "reminders-due",
                    IconClass = "fa fa-bell",
                    TotalReminderCount = GetTotalRemindersForFilteredType( "due", reminders ),
                    Parameters = new Dictionary<string, string>
                    {
                        ["CompletionFilter"] = FilterBag.CompletionFilterValue.Active.ToString(),
                        ["DueFilter"] = FilterBag.DueFilterValue.Due.ToString()
                    },
                    Order = 1
                },

                //
                // The 'Future' filtered reminder option.
                //
                new FilteredReminderOptionBag
                {
                    Name = "Future",
                    CssClass = "reminders-future",
                    IconClass = "fa fa-calendar",
                    TotalReminderCount = GetTotalRemindersForFilteredType( "future", reminders ),
                    Parameters = new Dictionary<string, string>
                    {
                        ["CompletionFilter"] = FilterBag.CompletionFilterValue.Incomplete.ToString(),
                        ["StartDateFilter"] = RockDateTime.Now.ToString()
                    },
                    Order = 2
                },

                //
                // The 'All' filtered reminder option.
                //
                new FilteredReminderOptionBag
                {
                    Name = "All",
                    CssClass = "reminders-all",
                    IconClass = "fa fa-inbox",
                    TotalReminderCount = GetTotalRemindersForFilteredType( "all", reminders ),
                    Parameters = new Dictionary<string, string>
                    {
                        ["GroupByType"] = true.ToString(),
                        ["CompletionFilter"] = FilterBag.CompletionFilterValue.Incomplete.ToString(),
                        ["CollectionHeader"] = "All"
                    },
                    Order = 3
                },

                //
                // The 'Completed' filtered reminder option.
                //
                new FilteredReminderOptionBag
                {
                    Name = "Completed",
                    CssClass = "reminders-completed",
                    IconClass = "fa fa-check",
                    TotalReminderCount = GetTotalRemindersForFilteredType( "completed", reminders ),
                    Parameters = new Dictionary<string, string>
                    {
                        ["CompletionFilter"] = FilterBag.CompletionFilterValue.Complete.ToString(),
                    },
                    Order = 4
                }
            };

            return filteredReminderOptionBag.OrderBy( x => x.Order ).ToList();
        }

        /// <summary>
        /// Gets the total number of reminders depending on the filter passed in.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="reminders"></param>
        /// <returns>The # of reminders.</returns>
        private int GetTotalRemindersForFilteredType( string filter, IQueryable<Reminder> reminders )
        {
            // Get the reminders that are past due.
            if ( filter == "due" )
            {
                var currentDate = RockDateTime.Now;
                reminders = reminders.Where( r => r.ReminderDate <= currentDate && !r.IsComplete );
            }
            // Get the reminders that are upcoming.
            else if ( filter == "future" )
            {
                reminders = reminders.Where( r => r.ReminderDate > RockDateTime.Now && !r.IsComplete );
            }
            // Get the reminders that are completed.
            else if ( filter == "completed" )
            {
                reminders = reminders.Where( r => r.IsComplete );
            }
            else if ( filter == "all" )
            {
                reminders = reminders.Where( r => !r.IsComplete );
            }

            return reminders.Count();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the data to present on the dashboard.
        /// </summary>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetReminderDashboardData()
        {
            // We need a person to view this block.
            if ( RequestContext.CurrentPerson == null )
            {
                return ActionUnauthorized();
            }

            using ( var rockContext = new RockContext() )
            {
                // Get the list of filtered reminder types and the count associated with them.
                var filteredReminderOptions = GetFilteredReminderOptionBags( rockContext );

                // Get the list of associated reminder types and the count of reminders associated with them.
                var reminderTypes = GetReminderTypes( rockContext );

                return ActionOk( new ReminderDashboardInfoBag
                {
                    FilteredReminderOptionBags = filteredReminderOptions,
                    ReminderTypeInfoBags = reminderTypes,
                } );
            }
        }

        #endregion
    }
}
