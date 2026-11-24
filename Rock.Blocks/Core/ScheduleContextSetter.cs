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

using Rock.Attribute;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.ViewModels.Blocks.Core.ScheduleContextSetter;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the default schedule context for the site or page.
    /// </summary>
    [DisplayName( "Schedule Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default schedule context for the site." )]
    [IconCssClass( "ti ti-calendar" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [CustomRadioListField( "Context Scope",
        Description = "The scope of context to set",
        ListSource = "Site,Page",
        IsRequired = true,
        DefaultValue = "Site",
        Order = 0,
        Key = AttributeKey.ContextScope )]

    [SchedulesField( "Schedule Group",
        Description = "Choose a schedule group to populate the dropdown",
        Order = 1,
        Key = AttributeKey.ScheduleGroup )]

    [TextField( "Current Item Template",
        Description = "Lava template for the current item. The only merge field is {{ ScheduleName }}.",
        IsRequired = true,
        DefaultValue = "{{ ScheduleName }}",
        Order = 2,
        AllowLava = true,
        Key = AttributeKey.CurrentItemTemplate )]

    [TextField( "Dropdown Item Template",
        Description = "Lava template for items in the dropdown. The only merge field is {{ ScheduleName }}.",
        IsRequired = true,
        DefaultValue = "{{ ScheduleName }}",
        Order = 3,
        AllowLava = true,
        Key = AttributeKey.DropdownItemTemplate )]

    [TextField( "No Schedule Text",
        Description = "The text to show when there is no schedule in the context.",
        IsRequired = true,
        DefaultValue = "Select Schedule",
        Order = 4,
        Key = AttributeKey.NoScheduleText )]

    [TextField( "Clear Selection Text",
        Description = "The text displayed when a schedule can be unselected. This will not display when the text is empty.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.ClearSelectionText )]

    [BooleanField( "Display Query Strings",
        Description = "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.",
        DefaultValue = "false",
        Order = 6,
        Key = AttributeKey.DisplayQueryStrings )]

    #endregion

    [Rock.Cms.DefaultBlockRole( BlockRole.System )]
    [Rock.SystemGuid.EntityTypeGuid( "ACCB4DBC-27D4-46F4-B744-B0DEAD833049" )]
    [Rock.SystemGuid.BlockTypeGuid( "6553821F-9667-4576-924F-DAF1BB3F3223" )]
    public class ScheduleContextSetter : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ContextScope = "ContextScope";
            public const string ScheduleGroup = "ScheduleGroup";
            public const string CurrentItemTemplate = "CurrentItemTemplate";
            public const string DropdownItemTemplate = "DropdownItemTemplate";
            public const string NoScheduleText = "NoScheduleText";
            public const string ClearSelectionText = "ClearSelectionText";
            public const string DisplayQueryStrings = "DisplayQueryStrings";
        }

        private static class PageParameterKey
        {
            public const string ScheduleId = "ScheduleId";
        }

        private static class MergeFieldKey
        {
            public const string ScheduleName = "ScheduleName";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The options built during this request, so the initial HTML and the
        /// initialization box share one result.
        /// </summary>
        private ScheduleContextSetterOptionsBag _options;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return GetConfigurationOptionsBag();
        }

        /// <inheritdoc/>
        protected override string GetInitialHtmlContent()
        {
            // Render the closed dropdown server-side so the navbar does not
            // shift while the block loads. The text is the admin's Lava
            // template output, which is HTML like in the WebForms block.
            var options = GetConfigurationOptionsBag();

            return $@"
<ul class=""nav navbar-nav contextsetter contextsetter-schedule"">
    <li class=""dropdown"">
        <a class=""dropdown-toggle navbar-link"" href=""#"" data-toggle=""dropdown"">
            {options.CurrentSelectionText}
            <b class=""ti ti-caret-down-filled""></b>
        </a>
    </li>
</ul>";
        }

        /// <summary>
        /// Gets the configuration options that will be sent down to the client.
        /// </summary>
        /// <returns>The configuration options.</returns>
        private ScheduleContextSetterOptionsBag GetConfigurationOptionsBag()
        {
            if ( _options != null )
            {
                return _options;
            }

            var currentSchedule = RequestContext.GetContextEntity<Schedule>();

            // A schedule passed on the query string always refreshes the
            // context cookie, even when it already matches, so the cookie
            // scope and expiration stay current.
            var scheduleKey = PageParameter( PageParameterKey.ScheduleId );
            if ( scheduleKey.IsNotNullOrWhiteSpace() )
            {
                var schedule = new ScheduleService( RockContext ).Get( scheduleKey, !PageCache.Layout.Site.DisablePredictableIds );

                currentSchedule = SetScheduleContext( schedule, false, null, out _ );
            }

            _options = new ScheduleContextSetterOptionsBag
            {
                CurrentSelectionText = GetCurrentSelectionText( currentSchedule ),
                Schedules = GetScheduleItems()
            };

            return _options;
        }

        /// <summary>
        /// Gets the text that describes the current schedule selection.
        /// </summary>
        /// <param name="currentSchedule">The schedule in context, or <c>null</c> if none.</param>
        /// <returns>The text to display in the dropdown toggle.</returns>
        private string GetCurrentSelectionText( Schedule currentSchedule )
        {
            if ( currentSchedule == null )
            {
                return GetAttributeValue( AttributeKey.NoScheduleText );
            }

            // The output is rendered as HTML, so only pass admin data as merge
            // fields. Visitor input such as page parameters is left out.
            var mergeFields = new Dictionary<string, object>
            {
                { MergeFieldKey.ScheduleName, currentSchedule.Name }
            };
            mergeFields.AddRockRequestContext( RequestContext );

            return GetAttributeValue( AttributeKey.CurrentItemTemplate ).ResolveMergeFields( mergeFields );
        }

        /// <summary>
        /// Gets the dropdown items, including the clear selection item when
        /// that option is configured.
        /// </summary>
        /// <returns>The list of dropdown items.</returns>
        private List<ListItemBag> GetScheduleItems()
        {
            var dropdownItemTemplate = GetAttributeValue( AttributeKey.DropdownItemTemplate );

            // The output is rendered as HTML, so only pass admin data as merge
            // fields. Visitor input such as page parameters is left out.
            var mergeFields = new Dictionary<string, object>();
            mergeFields.AddRockRequestContext( RequestContext );

            var items = GetAvailableSchedules()
                .Select( s =>
                {
                    mergeFields[MergeFieldKey.ScheduleName] = s.Name;

                    return new ListItemBag
                    {
                        Value = s.Guid.ToString(),
                        Text = dropdownItemTemplate.ResolveMergeFields( mergeFields )
                    };
                } )
                .ToList();

            var clearSelectionText = GetAttributeValue( AttributeKey.ClearSelectionText );
            if ( clearSelectionText.IsNotNullOrWhiteSpace() )
            {
                items.Insert( 0, new ListItemBag
                {
                    Value = Guid.Empty.ToString(),
                    Text = clearSelectionText
                } );
            }

            return items;
        }

        /// <summary>
        /// Gets the schedules configured in the block settings, ordered by
        /// name. This is also used by the block action so that only an offered
        /// schedule can be selected.
        /// </summary>
        /// <returns>The list of schedules that may be selected.</returns>
        private List<NamedScheduleCache> GetAvailableSchedules()
        {
            return GetAttributeValue( AttributeKey.ScheduleGroup )
                .SplitDelimitedValues()
                .AsGuidList()
                .Select( g => NamedScheduleCache.Get( g ) )
                .Where( s => s != null )
                .OrderBy( s => s.Name )
                .ToList();
        }

        /// <summary>
        /// Sets or clears the schedule context.
        /// </summary>
        /// <param name="schedule">The schedule to put in context, or <c>null</c> to clear the context.</param>
        /// <param name="refreshPage">If <c>true</c>, <paramref name="redirectUrl"/> will be set to the URL to redirect to.</param>
        /// <param name="originalUrl">The absolute URL the individual is on, used as the base of the redirect URL.</param>
        /// <param name="redirectUrl">On exit, contains the URL to redirect to.</param>
        /// <returns>The schedule now in context, or <c>null</c> if the context was cleared.</returns>
        private Schedule SetScheduleContext( Schedule schedule, bool refreshPage, string originalUrl, out string redirectUrl )
        {
            var pageScope = GetAttributeValue( AttributeKey.ContextScope ) == "Page";

            if ( schedule != null )
            {
                RequestContext.SetContextEntity( schedule, pageScope );
            }
            else
            {
                RequestContext.RemoveContextEntity( typeof( Schedule ), pageScope );
            }

            redirectUrl = null;

            if ( !refreshPage )
            {
                return schedule;
            }

            redirectUrl = originalUrl;

            if ( PageParameter( PageParameterKey.ScheduleId ).IsNotNullOrWhiteSpace() || GetAttributeValue( AttributeKey.DisplayQueryStrings ).AsBoolean() )
            {
                var uri = new Uri( redirectUrl, UriKind.Absolute );
                var queryString = uri.Query.ParseQueryString();

                // Drop the parameter when the selection is cleared so a stale
                // value does not put the schedule back in context.
                if ( schedule != null )
                {
                    queryString.Set( PageParameterKey.ScheduleId, schedule.IdKey );
                }
                else
                {
                    queryString.Remove( PageParameterKey.ScheduleId );
                }

                // Leave off the "?" when no parameters remain.
                var query = queryString.ToQueryString();
                redirectUrl = query.IsNotNullOrWhiteSpace()
                    ? $"{uri.AbsolutePath}?{query}"
                    : uri.AbsolutePath;
            }

            return schedule;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Sets the schedule context to the selected schedule, or clears it
        /// when <paramref name="scheduleGuid"/> is <see cref="Guid.Empty"/>.
        /// </summary>
        /// <param name="scheduleGuid">The unique identifier of the selected schedule.</param>
        /// <param name="url">The URL of the page the individual is on.</param>
        /// <returns>An object containing the URL to redirect to.</returns>
        [BlockAction]
        public BlockActionResult SetSchedule( Guid scheduleGuid, string url )
        {
            // The redirect is built from this URL, so it must be the absolute
            // URL of the page the individual is on.
            if ( !Uri.TryCreate( url, UriKind.Absolute, out _ ) )
            {
                return ActionBadRequest( "Invalid URL." );
            }

            Schedule schedule = null;

            if ( scheduleGuid == Guid.Empty )
            {
                // Clearing is only offered when Clear Selection Text has a value.
                if ( GetAttributeValue( AttributeKey.ClearSelectionText ).IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( "Schedule not found." );
                }
            }
            else
            {
                // Only accept a schedule from the configured list so other
                // schedules cannot be selected by crafting a request.
                var scheduleId = GetAvailableSchedules().FirstOrDefault( s => s.Guid == scheduleGuid )?.Id;
                schedule = scheduleId.HasValue ? new ScheduleService( RockContext ).Get( scheduleId.Value ) : null;

                if ( schedule == null )
                {
                    return ActionBadRequest( "Schedule not found." );
                }
            }

            SetScheduleContext( schedule, true, url, out var redirectUrl );

            return ActionOk( new
            {
                RedirectUrl = redirectUrl
            } );
        }

        #endregion
    }
}
