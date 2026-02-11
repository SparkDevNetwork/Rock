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
using Rock.ViewModels.Blocks.CheckIn.Manager.CheckInContextSetter;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Manager
{
    /// <summary>
    /// Block that can be used to set the various context values for the check-in manager pages.
    /// </summary>
    [DisplayName( "Check-in Context Setter" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block that can be used to set the various context values for the check-in manager pages." )]
    [IconCssClass( "ti ti-building" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #region Campus Block Attributes

    [BooleanField( "Include Inactive Campuses",
        Description = "Should inactive campuses be listed as well?",
        DefaultValue = "false",
        Order = 4,
        Category = AttributeCategoryKey.Campus,
        Key = AttributeKey.IncludeInactiveCampuses )]

    [CampusField( "Default Campus", includeInactive: true,
        Description = "When there is no campus value, what campus should be displayed?",
        IsRequired = false,
        Order = 5,
        Category = AttributeCategoryKey.Campus,
        Key = AttributeKey.DefaultCampus )]

    [DefinedValueField( "Campus Types",
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 6,
        Category = AttributeCategoryKey.Campus,
        Key = AttributeKey.CampusTypes )]

    [DefinedValueField( "Campus Statuses",
        Description = "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 7,
        Category = AttributeCategoryKey.Campus,
        Key = AttributeKey.CampusStatuses )]

    #endregion

    #endregion

    [ConfigurationChangedReload( BlockReloadMode.Block )]
    [Rock.Cms.DefaultBlockRole( BlockRole.System )]
    [SystemGuid.EntityTypeGuid( "a8256bb8-66c8-4038-ad0c-041678ba7278" )]
    [Rock.SystemGuid.BlockTypeGuid( "3364aabf-0c5b-4bfb-8cf3-b1a80fd3ed10" )]
    public class CheckInContextSetter : RockBlockType
    {
        #region Keys

        private static class AttributeCategoryKey
        {
            public const string Campus = "Campus";
        }

        private static class AttributeKey
        {
            public const string IncludeInactiveCampuses = "IncludeInactiveCampuses";
            public const string DefaultCampus = "DefaultCampus";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
        }

        #endregion

        #region Fields

        /// <summary>
        /// This is a private variable used by <see cref="GetConfigurationOptionsBag"/>
        /// to return a cached version of the options during startup.
        /// </summary>
        private CheckInContextSetterOptionsBag _options;

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
            // Since this block is expected to be placed on a public facing
            // site, we want to avoid any potential snapping or loading
            // issues by rendering the initial HTML server-side.
            var options = GetConfigurationOptionsBag();

            return $@"
<div class=""context-setters-container"">
    <ul class=""nav navbar-nav contextsetter contextsetter-campus"">
        <li class=""dropdown"">
            <a class=""dropdown-toggle navbar-link"" href=""#"" data-toggle=""dropdown"">
                {options.SelectedCampus?.Text}
                <b class=""ti ti-caret-down-filled""></b>
            </a>
        </li>
    </ul>

    <ul class=""nav navbar-nav contextsetter contextsetter-location"">
        <div class=""control-wrapper"">
            <div>
                <div class=""picker picker-obsidian picker-select rollover-container picker-show-clear"">
                    <a class=""picker-label"" href=""#"">
                        <span class=""selected-names"">{options.SelectedLocation?.Text ?? "Select Location"}</span>
                        <b class=""ti ti-caret-down-filled""></b>
                    </a>
                </div>
            </div>
        </div>
    </ul>

    <ul class=""nav navbar-nav contextsetter contextsetter-schedule"">
        <li class=""dropdown"">
            <a class=""dropdown-toggle navbar-link"" href=""#"" data-toggle=""dropdown"">
                {options.SelectedSchedule?.Text ?? "All Schedules"}
                <b class=""ti ti-caret-down-filled""></b>
            </a>
        </li>
    </ul>
</div>";
        }

        /// <summary>
        /// Get the configuration options that will be sent down to the client.
        /// </summary>
        /// <returns>The configuration options.</returns>
        private CheckInContextSetterOptionsBag GetConfigurationOptionsBag()
        {
            if ( _options != null )
            {
                return _options;
            }

            var options = new CheckInContextSetterOptionsBag();

            InitializeCampusOptions( options );

            var location = RequestContext.GetContextEntity<Location>();
            options.SelectedLocation = RequestContext.GetContextEntity<Location>()?.ToListItemBag();
            options.SelectedSchedule = RequestContext.GetContextEntity<Schedule>()?.ToListItemBag();
            options.Schedules = location != null ? GetScheduleBagsByLocation( location.Guid ) : new List<ListItemBag>();

            _options = options;

            return options;
        }

        /// <summary>
        /// Initializes the campus selections in <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options to be updated.</param>
        private void InitializeCampusOptions( CheckInContextSetterOptionsBag options )
        {
            var includeInactive = GetAttributeValue( AttributeKey.IncludeInactiveCampuses ).AsBoolean();
            var defaultCampusGuid = GetAttributeValue( AttributeKey.DefaultCampus ).AsGuidOrNull();

            var campusTypeIds = GetAttributeValues( AttributeKey.CampusTypes )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var campusStatusIds = GetAttributeValues( AttributeKey.CampusStatuses )
                .AsGuidOrNullList()
                .Where( g => g.HasValue )
                .Select( g => DefinedValueCache.GetId( g.Value ) )
                .Where( id => id.HasValue )
                .Select( id => id.Value )
                .ToList();

            var currentCampus = RequestContext.GetContextEntity<Campus>();

            var campusList = CampusCache.All( includeInactive )
                .Where( c => !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                .Where( c => !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) )
                .ToList();

            if ( currentCampus != null )
            {
                // If the current campus is not in the available list, then unset it.
                if ( !campusList.Any( c => c.Guid == currentCampus.Guid ) )
                {
                    currentCampus = null;
                }
            }

            // If currentCampus still isn't already set, and DefaultCampus is
            // defined, use that as the campus context.
            if ( currentCampus == null && defaultCampusGuid.HasValue )
            {
                var defaultCampus = new CampusService( RockContext ).Get( defaultCampusGuid.Value );
                if ( defaultCampus != null )
                {
                    SetCampusContext( defaultCampus );
                    currentCampus = defaultCampus;

                    RequestContext.Response.RedirectToUrl( RequestContext.RequestUri.ToString() );
                }
            }

            options.Campuses = campusList.ToListItemBagList();
            options.SelectedCampus = currentCampus?.ToListItemBag();
            options.RootLocations = CampusCache.All( RockContext )
                .Where( c => c.LocationId.HasValue )
                .Select( c => new
                {
                    CampusGuid = c.Guid,
                    LocationGuid = NamedLocationCache.Get( c.LocationId.Value, RockContext )?.Guid
                } )
                .Where( c => c.LocationGuid.HasValue )
                .ToDictionary( c => c.CampusGuid, c => c.LocationGuid.Value );
        }

        /// <summary>
        /// Sets the campus context.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="refreshPage">If true, then the <paramref name="redirectUrl"/> will be set with the URL to redirect to.</param>
        /// <param name="redirectUrl">On exit, contains the URL to redirect to.</param>
        /// <returns>The campus object.</returns>
        private void SetCampusContext( Campus campus )
        {
            if ( campus != null )
            {
                RequestContext.SetContextEntity( campus );
            }
            else
            {
                RequestContext.RemoveContextEntity( typeof( Campus ) );
            }
        }

        private List<ListItemBag> GetScheduleBagsByLocation( Guid locationGuid )
        {
            return new GroupLocationService( RockContext )
                .Queryable()
                .Where( gl => gl.Location.Guid == locationGuid && gl.Schedules.Any() )
                .SelectMany( gl => gl.Schedules )
                .Select( s => new
                {
                    s.Guid,
                    s.Name
                } )
                .Where( s => !string.IsNullOrEmpty( s.Name ) )
                .Distinct()
                .Select( s => new ListItemBag
                {
                    Value = s.Guid.ToString(),
                    Text = s.Name
                } )
                .ToList();
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetSchedulesByLocation( Guid locationGuid )
        {
            return ActionOk( GetScheduleBagsByLocation( locationGuid ) );
        }

        #endregion
    }
}
