using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Mobile;
using Rock.Search;

namespace Rock.Blocks.Types.Mobile.Core
{
    /// <summary>
    /// Performs a search using one of the configured search components and displays the results.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockMobileBlockType" />

    [DisplayName( "Search" )]
    [Category( "Mobile > Core" )]
    [Description( "Performs a search using one of the configured search components and displays the results." )]
    [IconCssClass( "fa fa-search" )]

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

    [MobileNavigationActionField( "Detail Navigation Action",
        Description = "The navigation action to perform when an item is tapped. The Guid of the item will be passed as the entity name and Guid, such as PersonGuid=value.",
        IsRequired = false,
        DefaultValue = MobileNavigationActionFieldAttribute.NoneValue,
        Key = AttributeKey.DetailNavigationAction,
        Order = 6 )]

    [IntegerField( "Max Results",
        Description = "The maximum number of results to show before displaying a 'Show More' option.",
        IsRequired = true,
        DefaultIntegerValue = 25,
        Key = AttributeKey.MaxResults,
        Order = 7 )]

    #endregion

    public class Search : RockMobileBlockType
    {
        #region Block Attributes

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
            public const string SearchComponent = "SearchComponent";

            public const string ShowSearchLabel = "ShowSearchLabel";

            public const string SearchLabelText = "SearchLabelText";

            public const string SearchPlaceholderText = "SearchPlaceholderText";

            public const string ResultsSeparatorContent = "ResultsSeparatorContent";

            public const string ResultItemTemplate = "ResultItemTemplate";

            public const string DetailNavigationAction = "DetailNavigationAction";

            public const string MaxResults = "MaxResults";
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

        #endregion

        #region IRockMobileBlockType Implementation

        /// <summary>
        /// Gets the required mobile application binary interface version required to render this block.
        /// </summary>
        /// <value>
        /// The required mobile application binary interface version required to render this block.
        /// </value>
        public override int RequiredMobileAbiVersion => 3;

        /// <summary>
        /// Gets the class name of the mobile block to use during rendering on the device.
        /// </summary>
        /// <value>
        /// The class name of the mobile block to use during rendering on the device
        /// </value>
        public override string MobileBlockType => "Rock.Mobile.Blocks.Core.Search";

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
                DetailNavigationAction
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
        }

        #endregion
    }
}
