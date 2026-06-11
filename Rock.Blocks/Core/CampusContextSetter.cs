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
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Model;
using Rock.ViewModels.Blocks.Core.CampusContextSetter;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// Block that can be used to set the default campus context for the site or page
    /// </summary>
    [DisplayName( "Campus Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default campus context for the site or page." )]
    [IconCssClass( "ti ti-building" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CustomRadioListField( "Context Scope",
        Description = "The scope of context to set",
        ListSource = "Site,Page",
        IsRequired = true,
        DefaultValue = "Site",
        Order = 0,
        Key = AttributeKey.ContextScope )]

    [TextField( "Current Item Template",
        Description = "Lava template for the current item. The only merge field is {{ CampusName }}.",
        IsRequired = true,
        DefaultValue = "{{ CampusName }}",
        Order = 1,
        Key = AttributeKey.CurrentItemTemplate )]

    [TextField( "Dropdown Item Template",
        Description = "Lava template for items in the dropdown. The only merge field is {{ CampusName }}.",
        IsRequired = true,
        DefaultValue = "{{ CampusName }}",
        Order = 2,
        Key = AttributeKey.DropdownItemTemplate )]

    [TextField( "No Campus Text",
        Description = "The text displayed when no campus context is selected.",
        IsRequired = true,
        DefaultValue = "Select Campus",
        Order = 3,
        Key = AttributeKey.NoCampusText )]

    [TextField( "Clear Selection Text",
        Description = "The text displayed when a campus can be unselected. This will not display when the text is empty.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.ClearSelectionText )]

    [BooleanField( "Display Query Strings",
        Description = "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.",
        DefaultValue = "false",
        Order = 5,
        Key = AttributeKey.DisplayQueryStrings )]

    [BooleanField( "Include Inactive Campuses",
        Description = "Should inactive campuses be listed as well?",
        DefaultValue = "false",
        Order = 6,
        Key = AttributeKey.IncludeInactiveCampuses )]

    [BooleanField( "Default To Current User's Campus",
        Description = "Will use the campus of the current user if no context is provided.",
        Order = 7,
        Key = AttributeKey.DefaultToCurrentUser )]

    [CustomDropdownListField( "Alignment",
        Description = "Determines the alignment of the dropdown.",
        ListSource = "1^Left,2^Right",
        IsRequired = true,
        DefaultValue = "1",
        Order = 8,
        Key = AttributeKey.Alignment )]

    [CampusField( name: "Default Campus",
        description: "When there is no campus value, what campus should be displayed?",
        required: false,
        includeInactive: true,
        order: 9,
        key: AttributeKey.DefaultCampus )]

    [BooleanField( "Update Family Campus on Change",
        Description = "When the individual changes the selected campus, should their family's campus (primary family) be updated?",
        DefaultBooleanValue = false,
        Order = 10,
        Key = AttributeKey.UpdateFamilyCampusOnChange )]

    [DefinedValueField( "Campus Types",
        Description = "This setting filters the list of campuses by type that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        AllowMultiple = true,
        Order = 11,
        Key = AttributeKey.CampusTypes )]

    [DefinedValueField( "Campus Statuses",
        Description = "This setting filters the list of campuses by statuses that are displayed in the campus drop-down.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        AllowMultiple = true,
        Order = 12,
        Key = AttributeKey.CampusStatuses )]

    #endregion

    [ConfigurationChangedReload( BlockReloadMode.Block )]
    [Rock.Cms.DefaultBlockRole( BlockRole.System )]
    [SystemGuid.EntityTypeGuid( "22d6fd92-efd2-4f88-8355-792a459e453c" )]
    [Rock.SystemGuid.BlockTypeGuid( "4A5AAFFC-B1C7-4EFD-A9E4-84363242EA85" )]
    public class CampusContextSetter : RockBlockType
    {
        #region Keys

        public static class AttributeKey
        {
            public const string ContextScope = "ContextScope";
            public const string CurrentItemTemplate = "CurrentItemTemplate";
            public const string DropdownItemTemplate = "DropdownItemTemplate";
            public const string NoCampusText = "NoCampusText";
            public const string ClearSelectionText = "ClearSelectionText";
            public const string DisplayQueryStrings = "DisplayQueryStrings";
            public const string IncludeInactiveCampuses = "IncludeInactiveCampuses";
            public const string DefaultToCurrentUser = "DefaultToCurrentUser";
            public const string Alignment = "Alignment";
            public const string DefaultCampus = "DefaultCampus";
            public const string UpdateFamilyCampusOnChange = "UpdateFamilyCampusOnChange";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
        }

        #endregion

        #region Fields

        /// <summary>
        /// This is a private variable used by <see cref="GetConfigurationOptionsBag"/>
        /// to return a cached version of the options during startup.
        /// </summary>
        private CampusContextSetterOptionsBag _options;

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
<ul class=""nav navbar-nav contextsetter contextsetter-campus"">
    <li class=""dropdown"">
        <a class=""dropdown-toggle navbar-link"" href=""#"" data-toggle=""dropdown"">
            {options.CurrentSelectionText}
            <b class=""ti ti-caret-down-filled""></b>
        </a>
    </li>
</ul>";
        }

        /// <summary>
        /// Get the configuration options that will be sent down to the client.
        /// </summary>
        /// <returns>The configuration options.</returns>
        private CampusContextSetterOptionsBag GetConfigurationOptionsBag()
        {
            if ( _options != null )
            {
                return _options;
            }

            var options = new CampusContextSetterOptionsBag
            {
                Alignment = GetAttributeValue( AttributeKey.Alignment ).AsIntegerOrNull() ?? 1, // 2=Right [dropdown-menu-right]
            };

            InitializeCampusSelections( options );

            _options = options;

            return options;
        }

        /// <summary>
        /// Initializes the campus selections in <paramref name="options"/>.
        /// </summary>
        /// <param name="options">The options to be updated.</param>
        private void InitializeCampusSelections( CampusContextSetterOptionsBag options )
        {
            var campusEntityType = EntityTypeCache.Get( typeof( Campus ) );
            var currentCampus = RequestContext.GetContextEntity<Campus>();

            var campusIdString = RequestContext.QueryString["CampusId"];
            if ( campusIdString != null )
            {
                var campusId = campusIdString.AsInteger();

                // If there is a query parameter, ensure that the Campus
                // Context cookie is set (and has an updated expiration).
                //
                // Note: The Campus Context might already match due to the query
                // parameter, but has a different cookie context, so we still
                // need to ensure the cookie context is updated
                currentCampus = SetCampusContext( campusId, false, null, out _ );
            }

            // If the currentCampus isn't determined yet, and
            // DefaultToCurrentUser, and the person has a CampusId, use that
            // as the campus context.
            if ( currentCampus == null && GetAttributeValue( AttributeKey.DefaultToCurrentUser ).AsBoolean() && RequestContext.CurrentPerson != null )
            {
                var campusId = RequestContext.CurrentPerson.GetFamily().CampusId;
                if ( campusId.HasValue )
                {
                    currentCampus = SetCampusContext( campusId.Value, false, null, out _ );
                }
            }

            // If currentCampus still isn't determined, and DefaultCampus is
            // defined, use that as the campus context.
            var defaultCampusGuid = GetAttributeValue( AttributeKey.DefaultCampus ).AsGuidOrNull();
            if ( currentCampus == null && defaultCampusGuid.HasValue )
            {
                var defaultCampusId = CampusCache.GetId( defaultCampusGuid.Value );
                if ( defaultCampusId.HasValue )
                {
                    currentCampus = SetCampusContext( defaultCampusId.Value, true, null, out var redirectUrl );

                    RequestContext.Response.RedirectToUrl( redirectUrl );
                }
            }

            if ( currentCampus != null )
            {
                var mergeObjects = new Dictionary<string, object>
                {
                    { "CampusName", currentCampus.Name }
                };

                options.CurrentSelectionText = GetAttributeValue( AttributeKey.CurrentItemTemplate ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                options.CurrentSelectionText = GetAttributeValue( AttributeKey.NoCampusText );
            }

            var includeInactive = GetAttributeValue( AttributeKey.IncludeInactiveCampuses ).AsBoolean();

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

            var campusList = CampusCache.All( includeInactive )
                .Where( c => !campusTypeIds.Any() || ( c.CampusTypeValueId.HasValue && campusTypeIds.Contains( c.CampusTypeValueId.Value ) ) )
                .Where( c => !campusStatusIds.Any() || ( c.CampusStatusValueId.HasValue && campusStatusIds.Contains( c.CampusStatusValueId.Value ) ) )
                .ToListItemBagList();

            // Run lava on each campus.
            var dropdownItemTemplate = GetAttributeValue( AttributeKey.DropdownItemTemplate );
            if ( dropdownItemTemplate.IsNotNullOrWhiteSpace() )
            {
                foreach ( var campus in campusList )
                {
                    var mergeObjects = new Dictionary<string, object>
                    {
                        { "CampusName", campus.Text }
                    };
                    campus.Text = dropdownItemTemplate.ResolveMergeFields( mergeObjects );
                }
            }

            // Check if the campus can be unselected.
            if ( !string.IsNullOrEmpty( GetAttributeValue( AttributeKey.ClearSelectionText ) ) )
            {
                var blankCampus = new ListItemBag
                {
                    Text = GetAttributeValue( AttributeKey.ClearSelectionText ),
                    Value = Guid.Empty.ToString()
                };

                campusList.Insert( 0, blankCampus );
            }

            options.Campuses = campusList;
        }

        /// <summary>
        /// Sets the campus context.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="refreshPage">If true, then the <paramref name="redirectUrl"/> will be set with the URL to redirect to.</param>
        /// <param name="redirectUrl">On exit, contains the URL to redirect to.</param>
        /// <returns>The campus object.</returns>
        private Campus SetCampusContext( int campusId, bool refreshPage, string originalUrl, out string redirectUrl )
        {
            var pageScope = GetAttributeValue( AttributeKey.ContextScope ) == "Page";
            var campus = new CampusService( new RockContext() ).Get( campusId );

            if ( campus != null )
            {
                RequestContext.SetContextEntity( campus, pageScope );
            }
            else
            {
                RequestContext.RemoveContextEntity( typeof( Campus ), pageScope );
            }

            // Only redirect if refreshPage is true
            if ( refreshPage )
            {
                redirectUrl = originalUrl ?? RequestContext.RequestUri.ToString();

                if ( PageParameter( "CampusId" ).IsNotNullOrWhiteSpace() || GetAttributeValue( AttributeKey.DisplayQueryStrings ).AsBoolean() )
                {
                    var uri = new Uri( redirectUrl, UriKind.Absolute );
                    var queryString = uri.Query.ParseQueryString();

                    /*
                        11/28/2023 - JPH

                        Only set the "CampusId" page parameter if the current campusId value is > 0.

                        Otherwise, the old behavior of setting "CampusId=-1" can wreak havoc with the newly-added
                        "Default Campus" block setting when the individual clears out the campus selection. In that
                        scenario, the default campus would not be auto-selected because it would be overridden by the
                        "-1" in the query string. Furthermore, if the current campusId value is <= 0, let's go so far
                        as to try amd remove it from the query string to ensure a stale value doesn't stick around.

                        Reason: Added "Default Campus" block setting.
                     */
                    if ( campusId > 0 )
                    {
                        queryString.Set( "CampusId", campusId.ToString() );
                    }
                    else
                    {
                        queryString.Remove( "CampusId" );
                    }

                    redirectUrl = $"{uri.AbsolutePath}?{queryString.ToQueryString()}";
                }
            }
            else
            {
                redirectUrl = null;
            }

            return campus;
        }

        /// <summary>
        /// Updates the individual's primary family campus if enabled in
        /// block settings.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        private void UpdateFamilyCampusIfRequired( int campusId )
        {
            if ( !GetAttributeValue( AttributeKey.UpdateFamilyCampusOnChange ).AsBoolean() )
            {
                return;
            }

            var primaryFamilyId = RequestContext.CurrentPerson?.PrimaryFamilyId;
            if ( !primaryFamilyId.HasValue )
            {
                return;
            }

            var primaryFamily = new GroupService( RockContext ).Get( primaryFamilyId.Value );
            if ( primaryFamily == null )
            {
                return;
            }

            primaryFamily.CampusId = campusId;
            RockContext.SaveChanges();
        }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult SetCampus( Guid campusGuid, string url )
        {
            var campusId = campusGuid != Guid.Empty
                ? CampusCache.Get( campusGuid, RockContext )?.Id
                : -1;

            if ( !campusId.HasValue )
            {
                return ActionBadRequest( "Campus not found." );
            }

            var campus = SetCampusContext( campusId.Value, true, url, out var redirectUrl );

            // If the campus context was set with the selected value, try to
            // update the individual's primary family campus Id.
            if ( campus != null && campus.Id == campusId.Value )
            {
                UpdateFamilyCampusIfRequired( campusId.Value );
            }

            var result = new
            {
                RedirectUrl = redirectUrl
            };

            return ActionOk( result );
        }

        #endregion
    }
}
