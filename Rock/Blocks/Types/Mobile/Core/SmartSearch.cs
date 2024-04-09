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
using Rock.Common.Mobile.Blocks.Core.SmartSearch;
using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Search;
using Rock.Web.Cache;
using Rock.Web.UI;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Rock.Blocks.Types.Mobile.Core
{
    /// <summary>
    /// Performs a search using any of the configured search components and displays the results.
    /// </summary>
    /// <remarks>
    ///     <para>This block only supports some search components. It heavily relies on templates stored on the mobile shell.</para>
    ///     <para>Supported person entity search components: <see cref="SmartSearchConstants.SupportedPersonSearchComponents"/></para>
    ///     <para>Supported group entity search components: <see cref="SmartSearchConstants.SupportedGroupSearchComponents"/></para>
    /// </remarks>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Smart Search" )]
    [Category( "Mobile > Core" )]
    [Description( "Performs a search using the configured search components and displays the results." )]
    [IconCssClass( "fa fa-search" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CORE_SMART_SEARCH_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CORE_SMART_SEARCH )]

    #region Block Attributes

    [ComponentsField( "Rock.Search.SearchContainer, Rock",
        Name = "Search Component(s)",
        Description = "The search components to offer for searches.",
        IsRequired = true,
        Key = AttributeKey.SearchComponents,
        Order = 0 )]

    [CodeEditorField( "Header Content",
        Key = AttributeKey.HeaderContent,
        Description = "The content to display for the header.",
        IsRequired = false,
        DefaultValue = "",
        Order = 1 )]

    [CodeEditorField( "Footer Content",
        Key = AttributeKey.FooterContent,
        Description = "The content to display for the header.",
        IsRequired = false,
        DefaultValue = "",
        Order = 2 )]

    [IntegerField( "Result Size",
        Key = AttributeKey.ResultSize,
        Description = "The amount of results to initially return and with each sequential load (as you scroll down).",
        IsRequired = true,
        DefaultIntegerValue = 20,
        Order = 3 )]

    [BooleanField( "Auto Focus Keyboard",
        Description = "Determines if the keyboard should auto-focus into the search field when the page is attached.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.AutoFocusKeyboard,
        Order = 4 )]

    [IntegerField( "Stopped Typing Behavior Threshold",
        Description = "Changes the amount of time (in milliseconds) that a user must stop typing for the search command to execute. Set to 0 to disable entirely.",
        IsRequired = true,
        DefaultIntegerValue = 200,
        Key = AttributeKey.StoppedTypingBehaviorThreshold,
        Order = 5 )]

    //
    // Person-specific attributes
    //

    [BooleanField( "Show Birthdate",
        Description = "Determines if the person's birthdate should be displayed in the search results.",
        IsRequired = false,
        DefaultBooleanValue = false,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.ShowBirthdate,
        Category = AttributeCategory.PersonSearch,
        Order = 6 )]

    [BooleanField( "Show Age",
        Description = "Determines if the person's age should be displayed in the search results.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.ShowAge,
        Category = AttributeCategory.PersonSearch,
        Order = 7 )]

    [BooleanField( "Show Spouse",
        Description = "Determines if the person's spouse should be displayed in the search results.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.ShowSpouse,
        Category = AttributeCategory.PersonSearch,
        Order = 8 )]

    [BooleanField( "Show Phone Number",
        Description = "Determines if the person's phone number should be displayed in the search results.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.ShowPhoneNumber,
        Category = AttributeCategory.PersonSearch,
        Order = 9 )]

    [BooleanField( "Show Address",
        Description = "Determines if the person's address should be displayed in the search results.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.ShowAddress,
        Category = AttributeCategory.PersonSearch,
        Order = 10 )]

    [BooleanField( "Show Age",
        Description = "Determines if the person's age should be displayed in the search results.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.ShowAge,
        Category = AttributeCategory.PersonSearch,
        Order = 11 )]

    [LinkedPage(
        "Person Detail Page",
        Description = "Page to link to when a person taps on a Person search result. 'PersonGuid' is passed as the query string.",
        IsRequired = false,
        Key = AttributeKey.PersonDetailPage,
        Category = AttributeCategory.PersonSearch,
        Order = 12 )]

    [DataViewsField(
        "Person Highlight Indicators",
        Key = AttributeKey.PersonDataViewIcons,
        Category = AttributeCategory.PersonSearch,
        Description = "Select one or more Data Views for Person search result icons. Note: More selections increase processing time.",
        EntityTypeName = "Rock.Model.Person",
        DisplayPersistedOnly = true,
        IsRequired = false,
        Order = 13 )]

    //
    // Group-specific attributes
    //

    [LinkedPage(
        "Group Detail Page",
        Description = "Page to link to when a person taps on a Group search result. 'GroupGuid' is passed as the query string.",
        IsRequired = false,
        Key = AttributeKey.GroupDetailPage,
        Category = AttributeCategory.GroupSearch,
        Order = 14 )]

    [DataViewsField(
        "Group Highlight Indicators",
        Key = AttributeKey.GroupDataViewIcons,
        Category = AttributeCategory.GroupSearch,
        Description = "Select one or more Data Views for Group search result icons. Note: More selections increase processing time.",
        EntityTypeName = "Rock.Model.Group",
        DisplayPersistedOnly = true,
        IsRequired = false,
        Order = 15 )]

    #endregion

    public class SmartSearch : RockBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the search components configured for this block.
        /// </summary>
        protected List<Guid> SearchComponents => GetAttributeValue( AttributeKey.SearchComponents )?.SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the content to display above the search results.
        /// </summary>
        protected string HeaderContent => GetAttributeValue( AttributeKey.HeaderContent );

        /// <summary>
        /// Gets the content to display below the search results.
        /// </summary>
        protected string FooterContent => GetAttributeValue( AttributeKey.FooterContent );

        /// <summary>
        /// Gets the result size from the block attribute.
        /// </summary>
        protected int ResultSize => GetAttributeValue( AttributeKey.ResultSize ).AsInteger();

        /// <summary>
        /// Gets whether or not the keyboard should auto-focus into the search field when the page is attached.
        /// </summary>
        protected bool AutoFocusKeyboard => GetAttributeValue( AttributeKey.AutoFocusKeyboard ).AsBoolean();

        /// <summary>
        /// Gets the amount of time (in milliseconds) that a user must stop typing for the search command to execute.
        /// </summary>
        protected int StoppedTypingBehaviorThreshold => GetAttributeValue( AttributeKey.StoppedTypingBehaviorThreshold ).AsInteger();

        /// <summary>
        /// Gets whether or not the person's birthdate should be displayed in the search results for a Person search.
        /// </summary>
        protected bool ShowBirthdate => GetAttributeValue( AttributeKey.ShowBirthdate ).AsBoolean();

        /// <summary>
        /// Gets whether or not the person's age should be displayed in the search results for a Person search.
        /// </summary>
        protected bool ShowAge => GetAttributeValue( AttributeKey.ShowAge ).AsBoolean();

        /// <summary>
        /// Gets whether or not the person's spouse should be displayed in the search results for a Person search.
        /// </summary>
        protected bool ShowSpouse => GetAttributeValue( AttributeKey.ShowSpouse ).AsBoolean();

        /// <summary>
        /// Gets whether or not the person's phone number should be displayed in the search results for a Person search.
        /// </summary>
        protected bool ShowPhoneNumber => GetAttributeValue( AttributeKey.ShowPhoneNumber ).AsBoolean();

        /// <summary>
        /// Gets whether or not the person's address should be displayed in the search results for a Person search.
        /// </summary>
        protected bool ShowAddress => GetAttributeValue( AttributeKey.ShowAddress ).AsBoolean();

        /// <summary>
        /// The data views to use for person search result icons.
        /// </summary>
        protected List<Guid> PersonDataViewIcons => GetAttributeValue( AttributeKey.PersonDataViewIcons )?.SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// The data views to use for group search result icons.
        /// </summary>
        protected List<Guid> GroupDataViewIcons => GetAttributeValue( AttributeKey.GroupDataViewIcons )?.SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the page to link to when a person taps on a Person search result.
        /// </summary>
        protected Guid? PersonDetailPage => GetAttributeValue( AttributeKey.PersonDetailPage ).AsGuidOrNull();

        /// <summary>
        /// Gets the page to link to when a person taps on a Group search result.
        /// </summary>
        protected Guid? GroupDetailPage => GetAttributeValue( AttributeKey.GroupDetailPage ).AsGuidOrNull();

        #endregion

        #region Constants/Keys

        /// <summary>
        /// A class used to hold attribute category keys for this block.
        /// </summary>
        private static class AttributeCategory
        {
            /// <summary>
            /// The person search attribute category.
            /// </summary>
            public const string PersonSearch = "Person Search";

            /// <summary>
            /// The group search attribute category.
            /// </summary>
            public const string GroupSearch = "Group Search";
        }

        /// <summary>
        /// A class used to hold attribute keys for this block.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The search component attribute key.
            /// </summary>
            public const string SearchComponents = "SearchComponents";

            /// <summary>
            /// The header content attribute key.
            /// </summary>
            public const string HeaderContent = "HeaderContent";

            /// <summary>
            /// The footer content attribute key.
            /// </summary>
            public const string FooterContent = "FooterContent";

            /// <summary>
            /// The result size attribute key.
            /// </summary>
            public const string ResultSize = "ResultSize";

            /// <summary>
            /// The auto focus keyboard attribute key.
            /// </summary>
            public const string AutoFocusKeyboard = "AutoFocusKeyboard";

            /// <summary>
            /// The stopped typing behavior threshold attribute key.
            /// </summary>
            public const string StoppedTypingBehaviorThreshold = "StoppedTypingBehaviorThreshold";

            /// <summary>
            /// The show birthdate attribute key.
            /// </summary>
            public const string ShowBirthdate = "ShowBirthdate";

            /// <summary>
            /// The show age attribute key.
            /// </summary>
            public const string ShowAge = "ShowAge";

            /// <summary>
            /// The show spouse attribute key.
            /// </summary>
            public const string ShowSpouse = "ShowSpouse";

            /// <summary>
            /// The show phone number attribute key.
            /// </summary>
            public const string ShowPhoneNumber = "ShowPhoneNumber";

            /// <summary>
            /// The show address attribute key.
            /// </summary>
            public const string ShowAddress = "ShowAddress";

            /// <summary>
            /// The person detail page attribute key.
            /// </summary>
            public const string PersonDetailPage = "PersonDetailPage";

            /// <summary>
            /// The person data view icons attribute key.
            /// </summary>
            public const string PersonDataViewIcons = "PersonDataViewIcons";

            /// <summary>
            /// The group data view icons attribute key.
            /// </summary>
            public const string GroupDataViewIcons = "GroupDataViewIcons";

            /// <summary>
            /// The group detail page attribute key.=
            /// </summary>
            public const string GroupDetailPage = "GroupDetailPage";
        }

        /// <summary>
        /// A class used to hold template keys that are available on the mobile shell for this block.
        /// </summary>
        private static class TemplateKey
        {
            /// <summary>
            /// The mobile template key for a person search result item.
            /// </summary>
            public const string PersonSearchResultItem = "PersonSearchResultItem";

            /// <summary>
            /// The mobile template key for a group search result item.
            /// </summary>
            public const string GroupSearchResultItem = "GroupSearchResultItem";
        }

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc />
        public override object GetMobileConfigurationValues()
        {
            return new Rock.Common.Mobile.Blocks.Core.SmartSearch.Configuration
            {
                HeaderContent = HeaderContent,
                FooterContent = FooterContent,
                ResultSize = ResultSize,
                AutoFocusKeyboard = AutoFocusKeyboard,
                StoppedTypingBehaviorThreshold = StoppedTypingBehaviorThreshold,
                ShowBirthdate = ShowBirthdate,
                ShowAge = ShowAge,
                ShowAddress = ShowAddress,
                ShowPhoneNumber = ShowPhoneNumber,
                ShowSpouseName = ShowSpouse,
                PersonDetailPage = PersonDetailPage,
                GroupDetailPage = GroupDetailPage,
                SearchComponents = GetComponents()
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the configured search components for this block.
        /// </summary>
        /// <returns>A list of search components that the block should display.</returns>
        private IEnumerable<SearchComponentBag> GetComponents()
        {
            //
            // This block only supports certain search components (listed below).
            // The template key is used to determine which template to use for the search results.
            // The templates for this block are stored within the mobile shell.
            //

            var components = new List<SearchComponentBag>();

            foreach ( var guid in SearchComponents )
            {
                var templateKey = GetTemplateKeyForComponent( guid );

                if ( templateKey.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                var componentEntityType = EntityTypeCache.Get( guid );

                if ( componentEntityType == null )
                {
                    continue;
                }

                var searchComponent = SearchContainer.Instance.Components
                    .Select( c => c.Value.Value )
                    .FirstOrDefault( c => c.TypeGuid == guid );

                var componentBag = new SearchComponentBag
                {
                    Guid = guid,
                    TemplateKey = templateKey,
                    Name = componentEntityType.FriendlyName,
                    PreferredKeyboardMode = ( searchComponent?.PreferredKeyboardMode ?? Enums.Core.KeyboardInputMode.Default ).ToMobile()
                };

                components.Add( componentBag );
            }

            return components;
        }

        /// <summary>
        /// Gets the template key for a search component.
        /// </summary>
        /// <param name="guid">The search component guid.</param>
        /// <returns>A key used to identify a template on the mobile shell.</returns>
        private string GetTemplateKeyForComponent( Guid guid )
        {
            var templateKey = string.Empty;

            if ( SmartSearchConstants.SupportedPersonSearchComponents.Contains( guid ) )
            {
                templateKey = TemplateKey.PersonSearchResultItem;
            }
            else if ( SmartSearchConstants.SupportedGroupSearchComponents.Contains( guid ) )
            {
                templateKey = TemplateKey.GroupSearchResultItem;
            }

            return templateKey;
        }

        /// <summary>
        /// Gets the results as a <see cref="SearchResultUnionItemBag"/> object.
        /// </summary>
        /// <param name="results">A list of search results.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>A bag containing general information about the search, and search results.</returns>
        private SearchResultUnionItemBag GetSearchResultItems( List<object> results, RockContext rockContext )
        {
            var unionBag = new SearchResultUnionItemBag();
            var dataViewService = new DataViewService( rockContext );

            results.ForEach( o =>
            {
                if ( o is IEntity entity )
                {
                    var type = entity.GetType();

                    if ( type.IsDynamicProxyType() )
                    {
                        type = type.BaseType;
                    }

                    if ( entity is Person personEntity )
                    {
                        unionBag.DataViewIcons = GetDataViewIcons( dataViewService, PersonDataViewIcons );

                        if ( unionBag.PersonResults == null )
                        {
                            unionBag.PersonResults = new List<PersonSearchItemResultBag>();
                        }

                        var personBag = GetPersonSearchResultBag( personEntity, unionBag.DataViewIcons?.Select( dv => dv.DataViewGuid ).ToList() );
                        personBag.DetailKey = $"{type.Name}Guid";
                        personBag.Guid = personEntity.Guid;

                        unionBag.PersonResults.Add( personBag );
                    }
                    else if ( entity is Group groupEntity )
                    {
                        unionBag.DataViewIcons = GetDataViewIcons( dataViewService, GroupDataViewIcons );

                        if ( unionBag.GroupResults == null )
                        {
                            unionBag.GroupResults = new List<GroupSearchItemResultBag>();
                        }

                        var groupBag = GetGroupSearchItemResultBag( groupEntity, unionBag.DataViewIcons?.Select( dv => dv.DataViewGuid ).ToList() );
                        groupBag.DetailKey = $"{type.Name}Guid";
                        groupBag.Guid = groupEntity.Guid;

                        unionBag.GroupResults.Add( groupBag );
                    }
                }
            } );

            return unionBag;
        }

        /// <summary>
        /// Populates the data view icons for the search results.
        /// </summary>
        /// <param name="dataViewService">The data view service.</param>
        /// <param name="dataViewGuids">The data views to get the information from.</param>
        private List<DataViewIconBag> GetDataViewIcons( DataViewService dataViewService, IEnumerable<Guid> dataViewGuids )
        {
            if ( dataViewGuids?.Any() ?? false )
            {
                var dataViews = dataViewService.Queryable()
                    .Where( d => dataViewGuids.Contains( d.Guid ) )
                    .ToList()
                    .Where( d => d.IsAuthorized( Rock.Security.Authorization.VIEW, this.RequestContext.CurrentPerson ) )
                    .Select( d => new DataViewIconBag
                    {
                        DataViewGuid = d.Guid,
                        IconCssClass = d.IconCssClass,
                        HighlightColor = d.HighlightColor
                    } )
                    .ToList();

                return dataViews;
            }

            return null;
        }

        /// <summary>
        /// Gets the search result bag for a person.
        /// </summary>
        /// <param name="person">The person to return the bag for.</param>
        /// <param name="dataViewGuids">The data views to check if the person is in.</param>
        /// <returns></returns>
        private PersonSearchItemResultBag GetPersonSearchResultBag( Person person, List<Guid> dataViewGuids )
        {
            var itemBag = new PersonSearchItemResultBag();
            itemBag.NickName = person.NickName;
            itemBag.LastName = person.LastName;
            itemBag.PhotoUrl = MobileHelper.BuildPublicApplicationRootUrl( person.PhotoUrl );
            itemBag.Email = person.Email;
            itemBag.Age = person.Age;

            var spouse = person.GetSpouse();
            if( spouse != null )
            {
                itemBag.SpouseName = spouse.FullName;
            }

            itemBag.BirthDate = person.BirthDate;

            var address = person.GetHomeLocation()?.GetFullStreetAddress();

            if ( address.IsNotNullOrWhiteSpace() )
            {
                // Replace line breaks with commas.
                itemBag.Address = address.Replace( Environment.NewLine, ", " );
            }

            itemBag.PhoneNumberFormatted = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.NumberFormatted;
            var connectionStatus = person.ConnectionStatusValueId.HasValue ? DefinedValueCache.Get( person.ConnectionStatusValueId.Value ) : null;

            if( connectionStatus != null )
            {
                itemBag.ConnectionStatus = connectionStatus.Value;

                var connectionStatusColor = connectionStatus.GetAttributeValue( "Color" );
                itemBag.ConnectionStatusColor = connectionStatusColor.IsNotNullOrWhiteSpace() ? connectionStatusColor : "#f5f5f5";
            }

            var personDataViews = new List<Guid>();

            // Check if the person is in any of the provided data views.
            if( dataViewGuids != null )
            {
                foreach ( var dataViewGuid in dataViewGuids )
                {
                    var dataViewCache = DataViewCache.Get( dataViewGuid );
                    var entityIds = dataViewCache.GetEntityIds();
                    var recordExists = entityIds.Contains( person.Id );

                    if ( recordExists )
                    {
                        personDataViews.Add( dataViewGuid );
                    }
                }
            }

            itemBag.MatchingDataViews = personDataViews;

            return itemBag;
        }

        /// <summary>
        /// Gets the search result bag for a group.
        /// </summary>
        /// <param name="group">The group to retun the bag for.</param>
        /// <param name="dataViewGuids">The data view guids.</param>
        /// <returns></returns>
        private GroupSearchItemResultBag GetGroupSearchItemResultBag( Group group, List<Guid> dataViewGuids )
        {
            var itemBag = new GroupSearchItemResultBag();
            itemBag.Name = group.Name;
            itemBag.ParentGroupStructure = GetGroupParentStructure( group );
            itemBag.GroupTypeName = group.GroupType.Name;
            itemBag.GroupTypeColor = group.GroupType.GroupTypeColor;

            var groupDataViews = new List<Guid>();

            // Check if the person is in any of the provided data views.
            if ( dataViewGuids != null )
            {
                foreach ( var dataViewGuid in dataViewGuids )
                {
                    var dataViewCache = DataViewCache.Get( dataViewGuid );
                    var entityIds = dataViewCache.GetEntityIds();
                    var recordExists = entityIds.Contains( group.Id );

                    if ( recordExists )
                    {
                        groupDataViews.Add( dataViewGuid );
                    }
                }
            }

            itemBag.MatchingDataViews = groupDataViews;

            return itemBag;
        }

        /// <summary>
        /// Gets the structured group name for a group.
        /// Ex: Group Type > Sub Group > Group
        /// </summary>
        /// <returns></returns>
        private string GetGroupParentStructure( Group group )
        {
            if ( group == null )
            {
                return string.Empty;
            }

            var groupNames = new List<string>();
            var groupIds = new List<int>();
            var parentGroup = group.ParentGroup;

            while ( parentGroup != null && !groupIds.Contains( parentGroup.Id ) )
            {
                groupNames.Add( parentGroup.Name );
                groupIds.Add( parentGroup.Id );
                parentGroup = parentGroup.ParentGroup;
            }

            groupNames.Reverse();

            return string.Join( " > ", groupNames );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the search results that match the term.
        /// </summary>
        /// <returns>A view model that represents the results of the search.</returns>
        [BlockAction( "Search" )]
        public BlockActionResult GetSearchResults( SearchRequestBag requestBag )
        {
            using( var rockContext = new RockContext() )
            {
                var searchComponent = SearchContainer.Instance.Components
                                   .Select( c => c.Value.Value )
                                   .FirstOrDefault( c => c.TypeGuid == requestBag.SearchComponentGuid );

                var hasMore = false;

                if ( searchComponent == null || !searchComponent.IsActive )
                {
                    return ActionInternalServerError( "Search component is not configured or not active." );
                }

                // Perform the search, take one more than configured so we can
                // determine if there are more items.
                var results = searchComponent.SearchQuery( requestBag.SearchTerm )
                    .Skip( requestBag.Offset )
                    .Take( ResultSize + 1 )
                    .ToList();

                // Check if we have more results than we will send, if so then set
                // the flag to tell the client there are more results available.
                if ( results.Count > ResultSize )
                {
                    hasMore = true;
                    results = results.Take( ResultSize ).ToList();
                }

                // Convert the results into view models.
                var result = GetSearchResultItems( results, rockContext );

                return ActionOk( new SearchResponseBag
                {
                    Result = result,
                    HasMore = hasMore
                } );
            }
        }

        #endregion
    }
}