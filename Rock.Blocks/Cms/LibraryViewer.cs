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
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Model.CMS.ContentChannelItem.Options;
using Rock.Utility.ContentLibraryApi;
using Rock.ViewModels.Blocks.Cms.LibraryViewer;
using Rock.Web.Cache;
using Rock.Cms;

namespace Rock.Blocks.Cms
{
    [DisplayName( "Library Viewer" )]
    [Category( "CMS" )]
    [Description( "Displays items from the Spark Development Network Content Library." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "C368D439-37CC-4304-AC18-873DEC76289C" )]
    [Rock.SystemGuid.BlockTypeGuid( "B147D578-B5CF-4265-9D92-B7BC43BF1CBC" )]
    public class LibraryViewer : RockBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ContentChannelIdKey = "ContentChannelIdKey";
        }

        #endregion

        #region IRockObsidianBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannelIdKey = PageParameter( PageParameterKey.ContentChannelIdKey );
                var isUserIntendingToDownloadItems = contentChannelIdKey.IsNotNullOrWhiteSpace();
                LibraryViewerValidationResultsBag validationResults = null;
                var isDownloadStatusShown = false;
                var isDownloadDateShown = false;
                var isDownloadDateShownWithTime = false;
                var isDownloadDateShownAsDateRange = false;

                /*
                     8/4/2023 - JMH 

                     There are two ways to view this block--with and without a Content Channel ID Key.
                     If the Content Channel ID Key exists,
                     then the user intends to browse through the Content Library items
                     and possibly download some into their content channel.
                     Otherwise, the user intends to browse through the Content Library items without downloading.

                     Reason: Content Library Feature
                 */

                if ( isUserIntendingToDownloadItems )
                {
                    // If the Content Channel ID key was supplied,
                    // then the individual is viewing this block with the intent to download Content Library items.
                    // Validate that the Content Channel is configured correctly for the Content Library.

                    var contentChannelData = GetCurrentContentChannelQuery( rockContext )
                        .Select( c => new
                        {
                            c.ContentLibraryConfigurationJson,
                            c.ContentChannelType.DateRangeType,
                            c.ContentChannelType.IncludeTime,
                            c.ContentChannelType.DisableStatus
                        } )
                        .FirstOrDefault();

                    var contentLibraryConfiguration = contentChannelData?.ContentLibraryConfigurationJson.FromJsonOrNull<ContentLibraryConfiguration>();

                    if ( contentLibraryConfiguration?.IsEnabled != true )
                    {
                        // If the user intended to download items from
                        // the Content Library into their content channel,
                        // but the Content Library is not enabled for the content channel,
                        // then display an error to the user.
                        return new LibraryViewerInitializationBox
                        {
                            ErrorMessage = "Your content channel is currently not configured to use the Content Library. Please adjust the content channel configuration settings accordingly.",
                        };
                    }

                    isDownloadDateShown = contentChannelData.DateRangeType != ContentChannelDateType.NoDates;
                    isDownloadDateShownAsDateRange = contentChannelData.DateRangeType == ContentChannelDateType.DateRange;
                    isDownloadDateShownWithTime = contentChannelData.IncludeTime;
                    isDownloadStatusShown = !contentChannelData.DisableStatus;

                    validationResults = new LibraryViewerValidationResultsBag
                    {
                        IsAuthorAttributeMapped = contentLibraryConfiguration.AuthorAttributeGuid.HasValue,
                        IsImageAttributeMapped = contentLibraryConfiguration.ImageAttributeGuid.HasValue,
                        IsSummaryAttributeMapped = contentLibraryConfiguration.SummaryAttributeGuid.HasValue,
                    };
                }

                var metadata = new CachedContentLibraryApi().GetMetadata()?.Data;

                return new LibraryViewerInitializationBox
                {
                    ContentChannelIdKey = contentChannelIdKey,
                    Items = ConvertToItemBags( metadata?.Items, rockContext ),
                    ValidationResults = validationResults,
                    IsDownloadDateShown = isDownloadDateShown,
                    IsDownloadDateShownAsDateRange = isDownloadDateShownAsDateRange,
                    IsDownloadDateShownWithTime = isDownloadDateShownWithTime,
                    IsDownloadStatusShown = isDownloadStatusShown,
                };
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Downloads an item into a ContentChannel and returns the downloaded item.
        /// </summary>
        /// <param name="contentLibraryItemId">The content library item identifier.</param>
        /// <returns></returns>
        [BlockAction( "DownloadItem" )]
        public BlockActionResult DownloadItem( LibraryViewerDownloadItemBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannelData = GetCurrentContentChannelQuery( rockContext )
                    .Select( c => new
                    {
                        c.Guid,
                        c.ContentLibraryConfigurationJson
                    } )
                    .FirstOrDefault();

                if ( contentChannelData == null )
                {
                    return ActionBadRequest( $"{PageParameterKey.ContentChannelIdKey} is invalid." );
                }

                var contentLibraryConfiguration = contentChannelData.ContentLibraryConfigurationJson?.FromJsonOrNull<ContentLibraryConfiguration>();

                if ( contentLibraryConfiguration?.IsEnabled != true )
                {
                    return ActionBadRequest( "This channel is not configured to use the Content Library. Please update the configuration of this content channel." );
                }

                try
                {
                    var contentChannelItemService = new ContentChannelItemService( rockContext );
                    var contentLibraryApiItem = contentChannelItemService.AddFromContentLibrary(
                        new ContentLibraryItemDownloadOptions
                        {
                            ContentLibraryItemGuidToDownload = bag.ContentLibraryItemGuid,
                            DownloadIntoContentChannelGuid = contentChannelData.Guid,
                            CurrentPersonPerformingDownload = GetCurrentPerson(),
                            ContentChannelItemStatusOverride = bag.Status,
                            ContentChannelItemStartDateOverride = bag.StartDate,
                            ContentChannelItemEndDateOverride = bag.EndDate,
                        } );

                    var contentLibraryItem = ConvertToItemBag( contentLibraryApiItem, rockContext );

                    return ActionOk( contentLibraryItem );
                }
                catch ( AddFromContentLibraryException ex )
                {
                    RockLogger.Log.Error( RockLogDomains.Cms, ex, ex.Message );
                    return ActionBadRequest( ex.Message );
                }
                catch ( Exception ex )
                {
                    RockLogger.Log.Error( RockLogDomains.Cms, ex, ex.Message );
                    return ActionInternalServerError( "An unexpected error occurred while downloading the item." );
                }
            }
        }

        [BlockAction( "GetItemDetails" )]
        public BlockActionResult GetItemDetails( Guid contentLibraryItemId )
        {
            var contentLibraryApi = new CachedContentLibraryApi();
            var response = contentLibraryApi.GetItem( contentLibraryItemId );

            if ( !response.IsSuccess )
            {
                return ActionBadRequest( response.Error );
            }

            if ( response.IsSuccess && response.Data == null )
            {
                return ActionNotFound();
            }

            using ( var rockContext = new RockContext() )
            {
                var item = ConvertToItemBag( response.Data, rockContext );
                return ActionOk( item );
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts a Content Libray API item detail to a <see cref="LibraryViewerItemBag"/>.
        /// </summary>
        /// <param name="apiItemDetailBag">The API item detail bag.</param>
        /// <param name="rockContext">The rock context.</param>
        private LibraryViewerItemBag ConvertToItemBag( ContentLibraryApiItemDetailBag apiItemDetailBag, RockContext rockContext )
        {
            if ( apiItemDetailBag == null )
            {
                return null;
            }

            var itemBag = CopyToItemBag( apiItemDetailBag );

            var contentChannelId = GetCurrentContentChannelQuery( rockContext ).Select( c => ( int? ) c.Id ).FirstOrDefault();

            if ( contentChannelId.HasValue )
            {
                SetIsDownloadedOrUploaded( rockContext, contentChannelId.Value, itemBag );
            }

            return itemBag;
        }

        /// <summary>
        /// Converts Content Libray API item summaries to <see cref="LibraryViewerItemBag"/>s.
        /// </summary>
        /// <param name="apiItemSummaryBags">The API item summary bags.</param>
        /// <param name="rockContext">The rock context.</param>
        private List<LibraryViewerItemBag> ConvertToItemBags( List<ContentLibraryApiItemSummaryBag> apiItemSummaryBags, RockContext rockContext )
        {
            if ( apiItemSummaryBags?.Any() != true )
            {
                return new List<LibraryViewerItemBag>();
            }

            var itemBags = apiItemSummaryBags.Select( CopyToItemBag ).ToList();

            var contentChannelId = GetCurrentContentChannelQuery( rockContext ).Select( c => ( int? ) c.Id ).FirstOrDefault();

            if ( contentChannelId.HasValue )
            {
                SetIsDownloadedOrUploaded( rockContext, contentChannelId.Value, itemBags );
            }

            return itemBags;
        }

        /// <summary>
        /// Copies an item summary to a new <see cref="LibraryViewerItemBag"/>.
        /// </summary>
        /// <param name="apiItemSummaryBag">The API item summary bag.</param>
        private LibraryViewerItemBag CopyToItemBag( ContentLibraryApiItemSummaryBag apiItemSummaryBag )
        {
            return new LibraryViewerItemBag
            {
                AuthorName = apiItemSummaryBag.AuthorName,
                Downloads = apiItemSummaryBag.Downloads,
                ExperienceLevel = new ViewModels.Utility.ListItemBag
                {
                    Text = apiItemSummaryBag.ExperienceLevel.ConvertToString(),
                    Value = apiItemSummaryBag.ExperienceLevel.ConvertToInt().ToString()
                },
                Guid = apiItemSummaryBag.Guid,
                HtmlContent = null,
                ImageDownloadUrl = apiItemSummaryBag.ImageDownloadUrl,
                IsNew = apiItemSummaryBag.IsNew,
                IsPopular = apiItemSummaryBag.IsPopular,
                IsTrending = apiItemSummaryBag.IsTrending,
                LicenseType = new ViewModels.Utility.ListItemBag
                {
                    Text = DefinedValueCache.Get( apiItemSummaryBag.LicenseTypeGuid ).Value,
                    Value = apiItemSummaryBag.LicenseTypeGuid.ToString()
                },
                PublishedDateTime = apiItemSummaryBag.PublishedDateTime,
                SourcePublisherName = apiItemSummaryBag.SourcePublisherName,
                StructuredContent = null,
                Summary = apiItemSummaryBag.Summary,
                Title = apiItemSummaryBag.Title,
                Topic = new ViewModels.Utility.ListItemBag
                {
                    Text = ContentTopicCache.Get( apiItemSummaryBag.TopicGuid ).Name,
                    Value = apiItemSummaryBag.TopicGuid.ToString()
                }
            };
        }

        /// <summary>
        /// Copies an item detail to a new <see cref="LibraryViewerItemBag"/>.
        /// </summary>
        /// <param name="apiItemDetailBag">The API item detail bag.</param>
        private LibraryViewerItemBag CopyToItemBag( ContentLibraryApiItemDetailBag apiItemDetailBag )
        {
            return new LibraryViewerItemBag
            {
                AuthorName = apiItemDetailBag.AuthorName,
                Downloads = apiItemDetailBag.Downloads,
                ExperienceLevel = new ViewModels.Utility.ListItemBag
                {
                    Text = apiItemDetailBag.ExperienceLevel.ConvertToString(),
                    Value = apiItemDetailBag.ExperienceLevel.ConvertToInt().ToString()
                },
                Guid = apiItemDetailBag.Guid,
                HtmlContent = apiItemDetailBag.HtmlContent,
                ImageDownloadUrl = apiItemDetailBag.ImageDownloadUrl,
                IsNew = apiItemDetailBag.IsNew,
                IsPopular = apiItemDetailBag.IsPopular,
                IsTrending = apiItemDetailBag.IsTrending,
                LicenseType = new ViewModels.Utility.ListItemBag
                {
                    Text = DefinedValueCache.Get( apiItemDetailBag.LicenseTypeGuid ).Value,
                    Value = apiItemDetailBag.LicenseTypeGuid.ToString()
                },
                PublishedDateTime = apiItemDetailBag.PublishedDateTime,
                SourcePublisherName = apiItemDetailBag.SourcePublisherName,
                StructuredContent = apiItemDetailBag.StructuredContent,
                Summary = apiItemDetailBag.Summary,
                Title = apiItemDetailBag.Title,
                Topic = new ViewModels.Utility.ListItemBag
                {
                    Text = ContentTopicCache.Get( apiItemDetailBag.TopicGuid ).Name,
                    Value = apiItemDetailBag.TopicGuid.ToString()
                },
                WarningMessage = apiItemDetailBag.WarningMessage
            };
        }

        /// <summary>
        /// Gets a query returning the content channel associated with the ContentChannelIdKey page parameter, if set; otherwise, returns an empty query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        private IQueryable<ContentChannel> GetCurrentContentChannelQuery( RockContext rockContext )
        {
            var contentChannelIdKey = PageParameter( PageParameterKey.ContentChannelIdKey );

            if ( contentChannelIdKey.IsNullOrWhiteSpace() )
            {
                return Enumerable.Empty<ContentChannel>().AsQueryable();
            }

            return new ContentChannelService( rockContext ).GetQueryableByKey( contentChannelIdKey );
        }

        /// <summary>
        /// Sets the <see cref="LibraryViewerItemBag.IsDownloaded"/> and <see cref="LibraryViewerItemBag.IsUploaded"/> properties.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <param name="items">The items.</param>
        private void SetIsDownloadedOrUploaded( RockContext rockContext, int contentChannelId, params LibraryViewerItemBag[] items )
        {
            SetIsDownloadedOrUploaded( rockContext, contentChannelId, items.ToList() );
        }

        /// <summary>
        /// Sets the <see cref="LibraryViewerItemBag.IsDownloaded"/> and <see cref="LibraryViewerItemBag.IsUploaded"/> properties.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <param name="items">The items.</param>
        private void SetIsDownloadedOrUploaded( RockContext rockContext, int contentChannelId, List<LibraryViewerItemBag> items )
        {
            var itemMap = items.ToDictionary( item => item.Guid, item => item );

            var contentChannelItemService = new ContentChannelItemService( rockContext );

            var downloadedAndUploadedItemGuids = contentChannelItemService
                .AsNoFilter()
                .Where( i => i.ContentChannelId == contentChannelId && i.ContentLibrarySourceIdentifier.HasValue )
                .Select( i => new
                {
                    Guid = i.ContentLibrarySourceIdentifier.Value,
                    IsDownloaded = i.IsContentLibraryOwner.HasValue ? !i.IsContentLibraryOwner.Value : false,
                    IsUploaded = i.IsContentLibraryOwner.HasValue ? i.IsContentLibraryOwner.Value : false
                } )
                .ToList();

            foreach ( var downloadedOrUploadedItem in downloadedAndUploadedItemGuids )
            {
                if ( itemMap.TryGetValue( downloadedOrUploadedItem.Guid, out var item ) )
                {
                    item.IsDownloaded = downloadedOrUploadedItem.IsDownloaded;
                    item.IsUploaded = downloadedOrUploadedItem.IsUploaded;
                }
            }
        }

        #endregion
    }
}
