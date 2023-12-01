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
using Rock.Mobile;
using Rock.Model;
using Rock.Search;
using Rock.Web.Cache;

namespace Rock.Blocks.Types.Mobile.Core
{
    /// <summary>
    /// Performs a search using one of the configured search components and displays the results.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Search" )]
    [Category( "Mobile > Core" )]
    [Description( "Performs a search using one of the configured search components and displays the results." )]
    [IconCssClass( "fa fa-search" )]
    [SupportedSiteTypes( Model.SiteType.Mobile )]

    #region Block Attributes

    [ComponentField( "Rock.Search.SearchContainer, Rock",
        Name = "Search Component",
        Description = "The search component to use when performing searches.",
        IsRequired = true,
        Key = AttributeKey.SearchComponent,
        Order = 0 )]

    [BooleanField( "Show Search Label",
        Description = "Determines if the input label for the search box should be displayed.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.ShowSearchLabel,
        Order = 1 )]

    [TextField( "Search Label Text",
        Description = "The label for the search field.",
        IsRequired = true,
        DefaultValue = "Search",
        Key = AttributeKey.SearchLabelText,
        Order = 2 )]

    [TextField( "Search Placeholder Text",
        Description = "The text to show as the placeholder text in the search box.",
        IsRequired = false,
        Key = AttributeKey.SearchPlaceholderText,
        Order = 3 )]

    [BlockTemplateField( "Result Item Template",
        Description = "Lava template for rendering each result item. The Lava merge field will be 'Item'.",
        TemplateBlockValueGuid = SystemGuid.DefinedValue.BLOCK_TEMPLATE_MOBILE_CORE_SEARCH,
        IsRequired = true,
        DefaultValue = "50FABA2A-B23C-46CD-A634-2F54BC1AE8C3",
        Key = AttributeKey.ResultItemTemplate,
        Order = 4 )]

    [CodeEditorField( "Results Separator Content",
        Description = "Content to display between the search input and the results. This content will show with the display of the results.",
        IsRequired = false,
        DefaultValue = DefaultAttributeValue.ResultsSeparatorContent,
        Key = AttributeKey.ResultsSeparatorContent,
        Order = 5 )]

    [CodeEditorField( "Historical Result Item Template",
        Description = "Lava template for rendering each historical result item. The Lava merge fields will be populated by the values provided in the 'AppendToSearchHistory' command.",
        IsRequired = true,
        DefaultValue = defaultHistoricalItemTemplate,
        Key = AttributeKey.HistoricalResultItemTemplate,
        Order = 6 )]

    [MobileNavigationActionField( "Detail Navigation Action",
        Description = "The navigation action to perform when an item is tapped. The Guid of the item will be passed as the entity name and Guid, such as PersonGuid=value.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.NoneValue,
        Key = AttributeKey.DetailNavigationAction,
        Order = 7 )]

    [IntegerField( "Max Results",
        Description = "The maximum number of results to show before displaying a 'Show More' option.",
        IsRequired = true,
        DefaultIntegerValue = 25,
        Key = AttributeKey.MaxResults,
        Order = 8 )]

    [BooleanField( "Auto Focus Keyboard",
        Description = "Determines if the keyboard should auto-focus into the search field when the page is attached.",
        IsRequired = false,
        DefaultBooleanValue = true,
        ControlType = Field.Types.BooleanFieldType.BooleanControlType.Toggle,
        Key = AttributeKey.AutoFocusKeyboard,
        Order = 9 )]

    [IntegerField( "Stopped Typing Behavior Threshold",
        Description = "Changes the amount of time (in milliseconds) that a user must stop typing for the search command to execute. Set to 0 to disable entirely.",
        IsRequired = true,
        DefaultIntegerValue = 200,
        Key = AttributeKey.StoppedTypingBehaviorThreshold,
        Order = 10 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.MOBILE_CORE_SEARCH_BLOCK_TYPE )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.MOBILE_CORE_SEARCH )]
    public class Search : RockBlockType
    {
        #region Constants

        private const string defaultHistoricalItemTemplate = @"<StackLayout
    Spacing=""0"">
    <Frame StyleClass=""search-item-container"" 
        Margin=""0""
        BackgroundColor=""White""
        HasShadow=""false""
        HeightRequest=""40"">
            <StackLayout Orientation=""Horizontal""
                Spacing=""0""
                VerticalOptions=""Center"">
                <Rock:Image Source=""{{ PhotoUrl | Escape }}""
                    StyleClass=""search-image""
                    VerticalOptions=""Start""
                    Aspect=""AspectFit""
                    Margin=""0, 4, 8, 0"">
                    <Rock:CircleTransformation />
                </Rock:Image>
                
                <StackLayout Spacing=""0"" 
                    HorizontalOptions=""FillAndExpand"">
                    <StackLayout Orientation=""Horizontal""
                    VerticalOptions=""Center"">
                        <Label StyleClass=""search-item-name""
                            Text=""{{ Name }}""
                            LineBreakMode=""TailTruncation""
                            HorizontalOptions=""FillAndExpand"" />
                    </StackLayout>
                    {% if Text == null or Text == """" %}
                        {% assign Text = ""No Email"" %}
                    {% endif %}
                        <Label StyleClass=""search-item-text""
                            Grid.Column=""0""
                            MaxLines=""2""
                            LineBreakMode=""TailTruncation"">{{ Text | XamlWrap }}</Label> 
                </StackLayout>

                <Rock:Icon IconClass=""times""
                    VerticalTextAlignment=""Center""
                    Grid.Column=""1"" 
                    StyleClass=""note-read-more-icon""
                    HorizontalOptions=""End""
                    Padding=""8, 0"">
                    <Rock:Icon.GestureRecognizers>
                        <TapGestureRecognizer
                            Command=""{Binding DeleteFromSearchHistory}""
                            CommandParameter=""{{ Guid }}"" />
                    </Rock:Icon.GestureRecognizers>
                </Rock:Icon>
            </StackLayout>
        </Frame>
    <BoxView HorizontalOptions=""FillAndExpand""
        HeightRequest=""1""
        Color=""#cccccc"" />
</StackLayout>";

        /// <summary>
        /// The default attribute values.
        /// </summary>
        private static class DefaultAttributeValue
        {
            public const string ResultsSeparatorContent = @"<StackLayout StyleClass=""search-result-header"">
    <Rock:Divider />
</StackLayout>";
        }

        /// <summary>
        /// The block setting attribute keys.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The search component attribute key.
            /// </summary>
            public const string SearchComponent = "SearchComponent"; // key for search component attribute

            /// <summary>
            /// The show search label attribute key.
            /// </summary>
            public const string ShowSearchLabel = "ShowSearchLabel";

            /// <summary>
            /// The search label text attribute key.
            /// </summary>
            public const string SearchLabelText = "SearchLabelText";

            /// <summary>
            /// The search placeholder text attribute key.
            /// </summary>
            public const string SearchPlaceholderText = "SearchPlaceholderText";

            /// <summary>
            /// The results separator content attribute key.
            /// </summary>
            public const string ResultsSeparatorContent = "ResultsSeparatorContent";

            /// <summary>
            /// The result item template attribute key.
            /// </summary>
            public const string ResultItemTemplate = "ResultItemTemplate";

            /// <summary>
            /// The historical result item template attribute key.
            /// </summary>
            public const string HistoricalResultItemTemplate = "HistoricalResultItemTemplate";

            /// <summary>
            /// The detail navigation action attribute key.
            /// </summary>
            public const string DetailNavigationAction = "DetailNavigationAction";

            /// <summary>
            /// The max results attribute key.
            /// </summary>
            public const string MaxResults = "MaxResults";

            /// <summary>
            /// The autofocus keyboard attribute key.
            /// </summary>
            public const string AutoFocusKeyboard = "AutoFocusKeyboard";

            /// <summary>
            /// The stopped typing behavior threshold attribute key.
            /// </summary>
            public const string StoppedTypingBehaviorThreshold = "StoppedTypingBehaviorThreshold";
        }


        /// <summary>
        /// Gets the search component to use for searches.
        /// </summary>
        /// <value>The search component to use for searches.</value>
        protected SearchComponent SearchComponent
        {
            get
            {
                var entityTypeGuid = GetAttributeValue( AttributeKey.SearchComponent ).AsGuid();

                return SearchContainer.Instance.Components
                    .Select( c => c.Value.Value )
                    .FirstOrDefault( c => c.TypeGuid == entityTypeGuid );
            }
        }

        /// <summary>
        /// Gets a value indicating whether the search label should be shown.
        /// </summary>
        /// <value><c>true</c> if the search label should be shown; otherwise, <c>false</c>.</value>
        protected bool ShowSearchLabel => GetAttributeValue( AttributeKey.ShowSearchLabel ).AsBoolean();

        /// <summary>
        /// Gets the search label text.
        /// </summary>
        /// <value>The search label text.</value>
        protected string SearchLabelText => GetAttributeValue( AttributeKey.SearchLabelText );

        /// <summary>
        /// Gets the search placeholder text.
        /// </summary>
        /// <value>The search placeholder text.</value>
        protected string SearchPlaceholderText => GetAttributeValue( AttributeKey.SearchPlaceholderText );

        /// <summary>
        /// Gets the content to be displayed just above the results.
        /// </summary>
        /// <value>The content to be displayed just above the results.</value>
        protected string ResultsSeparatorContent => GetAttributeValue( AttributeKey.ResultsSeparatorContent );

        /// <summary>
        /// Gets the result item template.
        /// </summary>
        /// <value>The result item template.</value>
        protected string ResultItemTemplate => Rock.Field.Types.BlockTemplateFieldType.GetTemplateContent( GetAttributeValue( AttributeKey.ResultItemTemplate ) );

        /// <summary>
        /// Gets the detail navigation action.
        /// </summary>
        /// <value>The detail navigation action.</value>
        internal MobileNavigationAction DetailNavigationAction => GetAttributeValue( AttributeKey.DetailNavigationAction ).FromJsonOrNull<MobileNavigationAction>() ?? new MobileNavigationAction();

        /// <summary>
        /// Gets the maximum results to be returned in each request.
        /// </summary>
        /// <value>The maximum results to be returned in each request.</value>
        protected int MaxResults => GetAttributeValue( AttributeKey.MaxResults ).AsIntegerOrNull() ?? 25;

        /// <summary>
        /// Gets a value indicating whether we should automatically focus the keyboard.
        /// </summary>
        /// <value><c>true</c> if [automatic focus keyboard]; otherwise, <c>false</c>.</value>
        public bool AutoFocusKeyboard => GetAttributeValue( AttributeKey.AutoFocusKeyboard ).AsBoolean();

        /// <summary>
        /// Gets the historical result item template.
        /// </summary>
        /// <value>The historical result item template.</value>
        public string HistoricalResultItemTemplate => GetAttributeValue( AttributeKey.HistoricalResultItemTemplate );

        /// <summary>
        /// Gets the stopped typing behavior threshold, which is a value used in the shell
        /// to set how long (in milliseconds) to wait before a search is executed.
        /// </summary>
        public int StoppedTypingBehaviorThreshold => GetAttributeValue( AttributeKey.StoppedTypingBehaviorThreshold ).AsInteger();

        #endregion

        #region IRockMobileBlockType Implementation

        /// <inheritdoc/>
        public override Version RequiredMobileVersion => new Version( 1, 3 );

        /// <summary>
        /// Gets the property values that will be sent to the device in the application bundle.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetMobileConfigurationValues()
        {
            return new
            {
                ShowSearchLabel,
                SearchLabelText,
                SearchPlaceholderText,
                ResultsSeparatorContent,
                DetailNavigationAction,
                AutoFocusKeyboard,
                HistoricalResultItemTemplate,
                StoppedTypingBehaviorThreshold,
                PreferredKeyboardMode = SearchComponent?.PreferredKeyboardMode.ToMobile() ?? Rock.Common.Mobile.Enums.KeyboardInputMode.Default
            };
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the search result item view model for the original item.
        /// </summary>
        /// <param name="item">The item to be represented by the view model.</param>
        /// <param name="content">The content to be displayed for this item.</param>
        /// <returns>A new <see cref="SearchResultItemViewModel"/> instance that represents the item.</returns>
        private SearchResultItemViewModel GetSearchResultItemViewModel( object item, string content )
        {
            var viewModel = new SearchResultItemViewModel
            {
                Content = content
            };

            if ( item is IEntity entity )
            {
                var type = entity.GetType();

                if ( type.IsDynamicProxyType() )
                {
                    type = type.BaseType;
                }

                viewModel.Guid = entity.Guid;
                viewModel.DetailKey = $"{type.Name}Guid";

                // If this entity is a person, we also want to pass the ViewedCount to the shell.
                var personEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PERSON ).Id;
                if ( entity.TypeId == personEntityTypeId )
                {
                    viewModel.ViewedCount = new PersonService( new RockContext() ).Get( entity.Id )?.ViewedCount;
                }
            }

            return viewModel;
        }

        /// <summary>
        /// Gets the search result item view models from the result set.
        /// </summary>
        /// <param name="results">The results to be converted into view models.</param>
        /// <returns>A list of <see cref="SearchResultItemViewModel"/> objects that represent the results.</returns>
        private List<SearchResultItemViewModel> GetSearchResultItems( List<object> results )
        {
            var itemTemplate = ResultItemTemplate;
            var mergeFields = RequestContext.GetCommonMergeFields();

            // Convert the results into item view models that will be understood
            // by the client.
            var resultItems = results
                .Select( r =>
                {
                    // Get the item type so the lava template can do conditional
                    // rendering based on the type.
                    var type = r.GetType();
                    if ( type.IsDynamicProxyType() )
                    {
                        type = type.BaseType;
                    }

                    mergeFields.AddOrReplace( "Item", r );
                    mergeFields.AddOrReplace( "ItemType", type.Name );
                    mergeFields.AddOrReplace( "DetailNavigationActionType", DetailNavigationAction.Type );
                    mergeFields.AddOrReplace( "DetailNavigationActionPage", DetailNavigationAction.PageGuid );

                    var content = itemTemplate.ResolveMergeFields( mergeFields );

                    return GetSearchResultItemViewModel( r, content );
                } )
                .ToList();
                
            return resultItems;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the search results that match the term.
        /// </summary>
        /// <param name="searchTerm">The search term to use for matching.</param>
        /// <param name="offset">The number of results to skip.</param>
        /// <returns>A view model that represents the results of the search.</returns>
        [BlockAction( "Search" )]
        public BlockActionResult GetSearchResults( string searchTerm, int offset = 0 )
        {
            var searchComponent = SearchComponent;
            var hasMore = false;

            if ( searchComponent == null || !searchComponent.IsActive )
            {
                return ActionInternalServerError( "Search component is not configured or not active." );
            }

            // Perform the search, take one more than configured so we can
            // determine if there are more items.
            var results = searchComponent.SearchQuery( searchTerm )
                .Skip( offset )
                .Take( MaxResults + 1 )
                .ToList();

            // Check if we have more results than we will send, if so then set
            // the flag to tell the client there are more results available.
            if ( results.Count > MaxResults )
            {
                hasMore = true;
                results = results.Take( MaxResults ).ToList();
            }

            // Convert the results into view models.
            var resultItems = GetSearchResultItems( results );

            return ActionOk( new
            {
                HasMore = hasMore,
                Results = resultItems
            } );
        }

        #endregion

        #region View Models

        /// <summary>
        /// A view model that represents a single search result item to be
        /// displayed by the client.
        /// </summary>
        private class SearchResultItemViewModel
        {
            /// <summary>
            /// Gets or sets the unique identifier of the search result item.
            /// </summary>
            /// <value>The unique identifier of the search result item.</value>
            public Guid? Guid { get; set; }

            /// <summary>
            /// Gets or sets the detail query string parameter key to be used
            /// when performing the detail navigation action.
            /// </summary>
            /// <value>The detail key query string parameter key.</value>
            public string DetailKey { get; set; }

            /// <summary>
            /// Gets or sets the content to be displayed for this item.
            /// </summary>
            /// <value>The content to be displayed for this item.</value>
            public string Content { get; set; }

            /// <summary>
            /// Gets or sets the view count.
            /// </summary>
            /// <value>The view count.</value>
            public int? ViewedCount { get; set; }
        }

        #endregion
    }
}
