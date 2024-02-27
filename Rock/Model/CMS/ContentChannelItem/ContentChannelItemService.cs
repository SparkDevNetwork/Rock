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
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DotLiquid.Util;

using Rock.Attribute;
using Rock.Cms;
using Rock.Data;
using Rock.Enums.Cms;
using Rock.Logging;
using Rock.Model.CMS.ContentChannelItem.Options;
using Rock.Store;
using Rock.Tasks;
using Rock.Utility.ContentLibraryApi;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Manually created Service methods for ContentChannelItem
    /// </summary>
    public partial class ContentChannelItemService
    {
        /// <summary>
        /// Gets the maximum item order value for content channel.
        /// </summary>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <returns>The highest value in the Order field for the specified content channel</returns>
        public int? GetMaxItemOrderValueForContentChannel( int contentChannelId )
        {
            return Queryable().Where( i => i.ContentChannelId == contentChannelId ).Max( i => ( int? ) i.Order );
        }

        /// <summary>
        /// Gets the next Order value for the content channel.
        /// </summary>
        /// <param name="contentChannelId">The content channel identifier.</param>
        /// <returns></returns>
        public int GetNextItemOrderValueForContentChannel( int contentChannelId )
        {
            int? i = GetMaxItemOrderValueForContentChannel( contentChannelId );
            return i == null ? 0 : ( int ) i + 1;
        }

        /// <summary>
        /// Downloads or redownloads an item from the Content Library and saves the context.
        /// </summary>
        /// <param name="options">The content library item download options.</param>
        /// <exception cref="AddFromContentLibraryException">Thrown when an error occurs while adding an item from the Content Library.</exception>
        /// <returns>The downloaded Content Library item details.</returns>
        [RockInternal( "1.16" )]
        public ContentLibraryApiItemDetailBag AddFromContentLibrary( ContentLibraryItemDownloadOptions options )
        {
            #region Validation

            var rockContext = ( RockContext ) this.Context;
            var contentChannel = new ContentChannelService( rockContext ).GetInclude( options.DownloadIntoContentChannelGuid, c => c.ContentChannelType );
            
            if ( contentChannel == null )
            {
                throw new ArgumentException( $"ContentChannel not found for { nameof( options.DownloadIntoContentChannelGuid ) }. [{ nameof( options.DownloadIntoContentChannelGuid ) }={ options.DownloadIntoContentChannelGuid }]", nameof( options.DownloadIntoContentChannelGuid ) );
            }

            ThrowIfContentLibraryNotEnabled( contentChannel );
            var contentLibraryConfiguration = contentChannel.ContentLibraryConfiguration;

            #endregion

            // Download the Content Library item.
            var contentLibraryApi = new CachedContentLibraryApi();
            var response = contentLibraryApi.DownloadItem(
                options.ContentLibraryItemGuidToDownload,
                new ContentLibraryApiItemDownloadBag
                {
                    DownloadedBy = options.CurrentPersonPerformingDownload?.FullName,
                    IsStructuredContentIncluded = contentChannel.IsStructuredContent,
                    OrganizationKey = OrganizationService.GetOrganizationKey()
                } );

            if ( response.IsSuccess && response.Data == null )
            {
                throw new AddFromContentLibraryException( options.ContentLibraryItemGuidToDownload, "Content Library Item not found." );
            }

            if ( !response.IsSuccess )
            {
                throw new AddFromContentLibraryException(
                    options.ContentLibraryItemGuidToDownload,
                    response.Error.IsNotNullOrWhiteSpace() ? response.Error : AddFromContentLibraryException.DefaultMessage
                );
            }

            // Ensure the author attribute is specified
            // if attribution is required by the item's license.
            var contentLibraryItem = response.Data;
            var attributionLicenseGuids = new List<Guid>
                {
                    SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_AUTHOR_ATTRIBUTION.AsGuid(),
                    SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_ORGANIZATION_ATTRIBUTION.AsGuid()
                };

            if ( attributionLicenseGuids.Contains( contentLibraryItem.LicenseTypeGuid )
                 && !contentLibraryConfiguration.AuthorAttributeGuid.HasValue )
            {
                throw new AddFromContentLibraryException(
                    options.ContentLibraryItemGuidToDownload,
                    "To comply with the attribution requirements of this item's license, the author information must be included. Please update the configuration of this content channel to include an attribute for storing the author information."
                );
            }

            // Check if this article has already been downloaded for the content channel.
            var contentChannelItemService = new ContentChannelItemService( rockContext );
            var contentChannelItem = contentChannelItemService.AsNoFilter()
                .Where( c => c.ContentChannelId == contentChannel.Id && c.ContentLibrarySourceIdentifier == options.ContentLibraryItemGuidToDownload )
                .FirstOrDefault();

            if ( contentChannelItem == null )
            {
                // Start and End Dates
                var startDateTime = RockDateTime.Now;
                DateTime? endDateTime = null;

                var dateRangeType = contentChannel.ContentChannelType.DateRangeType;

                if ( dateRangeType != ContentChannelDateType.NoDates )
                {
                    startDateTime = options.ContentChannelItemStartDateOverride ?? RockDateTime.Now;

                    if ( dateRangeType == ContentChannelDateType.DateRange )
                    {
                        endDateTime = options.ContentChannelItemEndDateOverride;
                    }

                    if ( !contentChannel.ContentChannelType.IncludeTime )
                    {
                        startDateTime = startDateTime.Date;
                        endDateTime = endDateTime?.Date;
                    }
                }

                // Create new content channel item.
                contentChannelItem = new ContentChannelItem
                {
                    ContentChannel = contentChannel,
                    ContentChannelId = contentChannel.Id,
                    ContentChannelType = contentChannel.ContentChannelType,
                    ContentChannelTypeId = contentChannel.ContentChannelType.Id,
                    Priority = 0,
                    StartDateTime = startDateTime,
                    ExpireDateTime = endDateTime,
                };

                if ( contentChannel.ItemsManuallyOrdered )
                {
                    contentChannelItem.Order = contentChannelItemService.GetNextItemOrderValueForContentChannel( contentChannel.Id );
                }

                contentChannelItem.ItemGlobalKey = CreateSlug( rockContext, contentLibraryItem.Title );

                contentChannelItemService.Add( contentChannelItem );
            }

            // Always keep these properties up-to-date (new and existing items).
            contentChannelItem.ContentLibrarySourceIdentifier = contentLibraryItem.Guid;
            contentChannelItem.ContentLibraryLicenseTypeValueId = DefinedValueCache.GetId( contentLibraryItem.LicenseTypeGuid );
            contentChannelItem.StructuredContent = contentChannel.IsStructuredContent ? contentLibraryItem.StructuredContent : null;
            contentChannelItem.Content = contentLibraryItem.HtmlContent;
            contentChannelItem.Title = contentLibraryItem.Title;
            contentChannelItem.ExperienceLevel = contentLibraryItem.ExperienceLevel;
            contentChannelItem.ContentLibraryContentTopicId = new ContentTopicService( rockContext ).GetId( contentLibraryItem.TopicGuid );
            contentChannelItem.IsContentLibraryOwner = false;

            // Approval status
            if ( options.ContentChannelItemStatusOverride.HasValue )
            {
                contentChannelItem.Status = options.ContentChannelItemStatusOverride.Value;
            }
            else if ( contentChannel.ContentChannelType.DisableStatus )
            {
                contentChannelItem.Status = ContentChannelItemStatus.Approved;
            }
            else if ( contentChannel.RequiresApproval || !contentChannelItem.IsAuthorized( Rock.Security.Authorization.APPROVE, options.CurrentPersonPerformingDownload ) )
            {
                contentChannelItem.Status = ContentChannelItemStatus.PendingApproval;
                contentChannelItem.ApprovedDateTime = null;
                contentChannelItem.ApprovedByPersonAliasId = null;
            }
            else
            {
                contentChannelItem.Status = ContentChannelItemStatus.Approved;
                contentChannelItem.ApprovedDateTime = RockDateTime.Now;
                contentChannelItem.ApprovedByPersonAliasId = options.CurrentPersonPerformingDownload?.PrimaryAliasId;
            }

            // Content Library Item Attributes
            var attributes = new List<AttributeCache>();

            if ( contentLibraryConfiguration != null )
            {
                contentChannelItem.LoadAttributes( rockContext );

                // Summary
                var summaryAttribute = contentChannelItem.Attributes.Values.FirstOrDefault( a => a.Guid == contentLibraryConfiguration.SummaryAttributeGuid );
                if ( summaryAttribute != null )
                {
                    contentChannelItem.SetAttributeValue( summaryAttribute.Key, contentLibraryItem.Summary );
                }

                // Author (based on license)
                var authorAttribute = contentChannelItem.Attributes.Values.FirstOrDefault( a => a.Guid == contentLibraryConfiguration.AuthorAttributeGuid );
                if ( authorAttribute != null )
                {
                    if ( contentLibraryItem.LicenseTypeGuid == Rock.SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_ORGANIZATION_ATTRIBUTION.AsGuid() )
                    {
                        // If the license is organization attribution,
                        // then save the organization (publisher) name as author.
                        contentChannelItem.SetAttributeValue( authorAttribute.Key, contentLibraryItem.SourcePublisherName );
                    }
                    else
                    {
                        // If the license is anything else (open or author attribution),
                        // then save the author name as author.
                        contentChannelItem.SetAttributeValue( authorAttribute.Key, contentLibraryItem.AuthorName );
                    }
                }

                // Image
                var imageAttribute = contentChannelItem.Attributes.Values.FirstOrDefault( a => a.Guid == contentLibraryConfiguration.ImageAttributeGuid );
                if ( imageAttribute != null )
                {
                    if ( contentLibraryItem.ImageDownloadUrl.IsNotNullOrWhiteSpace() )
                    {
                        try
                        {
                            // Download the image from the content library item URL.
                            var imageRequest = WebRequest.Create( contentLibraryItem.ImageDownloadUrl );
                            var imageResponse = ( HttpWebResponse ) imageRequest.GetResponse();
                            using ( var stream = imageResponse.GetResponseStream() )
                            {
                                var binaryFileGuid = Guid.NewGuid();
                                var fileNameMatch = Regex.Match( imageResponse.GetResponseHeader( "content-disposition" ), ".*filename=\"?(.+)\"?.*" );

                                // Do not use a using block for the memory stream since it needs to remain open until the BinaryFile is saved.
                                var memoryStream = new System.IO.MemoryStream();
                                stream.CopyTo( memoryStream );

                                var binaryFile = new BinaryFile
                                {
                                    IsTemporary = false,
                                    BinaryFileTypeId = BinaryFileTypeCache.GetId( SystemGuid.BinaryFiletype.CONTENT_CHANNEL_ITEM_IMAGE.AsGuid() ),
                                    MimeType = imageResponse.ContentType,
                                    // Use the BinaryFile guid as the filename if the filename isn't in the response headers.
                                    FileName = fileNameMatch.Success && fileNameMatch.Groups.Count > 1 && fileNameMatch.Groups[1].Value.IsNotNullOrWhiteSpace() ? fileNameMatch.Groups[1].Value : binaryFileGuid.ToString(),
                                    FileSize = imageResponse.ContentLength,
                                    ContentStream = memoryStream,
                                    Guid = binaryFileGuid
                                };

                                var binaryFileService = new BinaryFileService( rockContext );
                                binaryFileService.Add( binaryFile );
                                contentChannelItem.SetAttributeValue( imageAttribute.Key, binaryFileGuid );
                            }
                        }
                        catch ( Exception ex )
                        {
                            RockLogger.Log.Error( RockLogDomains.Cms, ex, "Failed to download Content Library Item image at {Url}", contentLibraryItem.ImageDownloadUrl );

                            // Although the Content Library item image failed to download,
                            // the rest of the data was downloaded successfully.
                            contentChannelItem.SetAttributeValue( imageAttribute.Key, string.Empty );
                            contentLibraryItem.WarningMessage = "The item downloaded successfully but the image download failed.";
                        }
                    }
                    else
                    {
                        contentChannelItem.SetAttributeValue( imageAttribute.Key, string.Empty );
                    } 
                }
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                contentChannelItem.SaveAttributeValues( rockContext );
            } );

            // Update the content collection index for 
            // efficient searching and filtering operations.
            new ProcessContentCollectionDocument.Message
            {
                EntityTypeId = contentChannelItem.TypeId,
                EntityId = contentChannelItem.Id
            }.Send();

            return contentLibraryItem;
        }

        /// <summary>
        /// Builds a request to upload an item to the Content Library.
        /// </summary>
        /// <param name="id">The <see cref="ContentChannelItem"/> identifier.</param>
        [RockInternal( "1.16" )]
        public ContentLibraryApiItemUploadBag GetContentLibraryUploadRequest( int id )
        {
            var rockContext = ( RockContext ) this.Context;
            var item = GetInclude( id, i => i.ContentChannel );

            if ( item == null )
            {
                throw new ArgumentNullException( nameof( item ) );
            }

            ThrowIfContentLibraryNotEnabled( item.ContentChannel );

            item.LoadAttributes( rockContext );
            var itemAttributes = new ContentLibraryItemAttributes( item, item.ContentChannel.ContentLibraryConfiguration );
            var contentTopicGuid = ( item.ContentLibraryContentTopicId.HasValue ? new ContentTopicService( rockContext ).GetGuid( item.ContentLibraryContentTopicId.Value ) : null ) ?? Guid.Empty;
            var imageGuid = itemAttributes.ImageGuid.AsGuidOrNull();
            var licenseTypeValueGuid = ( item.ContentChannel.ContentLibraryConfiguration.LicenseTypeValueGuid.HasValue ? DefinedValueCache.Get( item.ContentChannel.ContentLibraryConfiguration.LicenseTypeValueGuid.Value )?.Guid : null ) ?? Guid.Empty;
            var experienceLevel = item.ExperienceLevel ?? ( ContentLibraryItemExperienceLevel )( -1 );
            var structuredContent = item.ContentChannel.IsStructuredContent ? item.StructuredContent : null;
            var imageDownloadUrl = ( string ) null;
            if ( imageGuid.HasValue )
            {
                var imageBinaryFile = new BinaryFileService( rockContext ).Get( imageGuid.Value );
                imageDownloadUrl = imageBinaryFile?.Url;
                if ( imageDownloadUrl.IsNullOrWhiteSpace() )
                {
                    // If the image URL is not resolved, 
                    // then fallback to building the URL from the HTTP request.
                    if ( System.Web.HttpContext.Current?.Request != null )
                    {
                        var uri = new Uri( System.Web.HttpContext.Current.Request.UrlProxySafe().ToString() );
                        imageDownloadUrl = $"{ uri.Scheme }://{ uri.GetComponents( UriComponents.HostAndPort, UriFormat.Unescaped ).EnsureTrailingForwardslash() }GetImage.ashx?guid={imageGuid.Value}";
                    }
                }
            }

            return new ContentLibraryApiItemUploadBag
            {
                AuthorName = itemAttributes.AuthorName,
                ContentHtml = item.Content,
                ContentStructured = structuredContent,
                ContentTopicGuid = contentTopicGuid,
                ExperienceLevel = experienceLevel,
                ImageDownloadUrl = imageDownloadUrl,
                IsStructuredContentIncluded = item.ContentChannel.IsStructuredContent,
                LicenseTypeValueGuid = licenseTypeValueGuid,
                OrganizationKey = OrganizationService.GetOrganizationKey(),
                SourceIdentifier = item.Guid,
                SourcePublisherName = GlobalAttributesCache.Value( "OrganizationName" ),
                Summary = itemAttributes.Summary,
                Title = item.Title
            };
        }

        /// <summary>
        /// Uploads an item to the content library and saves changes.
        /// </summary>
        /// <param name="options">The content library item upload options.</param>
        /// <exception cref="AddToContentLibraryException">Thrown when an error occurs while adding an item to the Content Library.</exception>
        /// <returns>The uploaded Content Library item details.</returns>
        [RockInternal( "1.16" )]
        public ContentLibraryApiItemDetailBag UploadToContentLibrary( ContentLibraryItemUploadOptions options )
        {
            var uploadRequest = GetContentLibraryUploadRequest( options.ContentChannelItemId );

            var contentLibraryApi = new CachedContentLibraryApi();
            var response = contentLibraryApi.UploadItem( uploadRequest );

            if ( !response.IsSuccess )
            {
                throw new AddToContentLibraryException(
                    options.ContentChannelItemId,
                    response.Error.IsNotNullOrWhiteSpace() ? response.Error : AddToContentLibraryException.DefaultMessage
                );
            }

            // If the item was uploaded successfully,
            // then update the Content Library Item guid on this item
            // and mark as the library owner.
            var item = Get( options.ContentChannelItemId );
            item.ContentLibrarySourceIdentifier = response.Data.Guid;
            item.IsContentLibraryOwner = true;
            item.ContentLibraryLicenseTypeValueId = DefinedValueCache.GetId( response.Data.LicenseTypeGuid );
            item.ContentLibraryUploadedByPersonAliasId = options.UploadedByPersonAliasId;
            item.ContentLibraryUploadedDateTime = RockDateTime.Now;

            this.Context.SaveChanges();

            // Return the library item details.
            return response.Data;
        }

        /// <summary>
        /// Creates a content library item slug.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="contentChannelItemTitle">The content channel item title.</param>
        /// <returns>The slug.</returns>
        private string CreateSlug( RockContext rockContext, string contentChannelItemTitle )
        {
            var contentChannelItemSlugService = new ContentChannelItemSlugService( rockContext );
            return contentChannelItemSlugService.GetUniqueContentSlug( contentChannelItemTitle, null );
        }

        /// <summary>
        /// Validates the content library enabled for content channel.
        /// </summary>
        /// <param name="contentChannel">The content channel.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">$"The Content Library is not enabled for Content Channel. [ContentChannelId={ contentChannel?.Id }]</exception>
        private void ThrowIfContentLibraryNotEnabled( ContentChannel contentChannel )
        {
            if ( !contentChannel.IsContentLibraryEnabled )
            {
                throw new ArgumentException( $"The Content Library is not enabled for Content Channel. [ContentChannelId={contentChannel?.Id}]" );
            }
        }

        #region Helper Classes

        private class ContentLibraryItemAttributes
        {
            private readonly ContentChannelItem _contentChannelItem;
            private readonly string _summaryAttributeKey;
            private readonly string _authorNameAttributeKey;
            private readonly string _imageUrlAttributeKey;

            /// <summary>
            /// Gets or sets the summary attribute value.
            /// <para>If the item does not have a summary attribute, then this property does nothing.</para>
            /// </summary>
            public string Summary
            {
                get
                {
                    return GetAttributeValueIfExists( _summaryAttributeKey );
                }
                set
                {
                    SetAttributeValueIfExists( _summaryAttributeKey, value );
                }
            }

            /// <summary>
            /// Gets or sets the author attribute value.
            /// <para>If the item does not have an author attribute, then this property does nothing.</para>
            /// </summary>
            public string AuthorName
            {
                get
                {
                    return GetAttributeValueIfExists( _authorNameAttributeKey );
                }
                set
                {
                    SetAttributeValueIfExists( _authorNameAttributeKey, value );
                }
            }

            /// <summary>
            /// Gets or sets the image attribute value.
            /// <para>If the item does not have an image attribute, then this property does nothing.</para>
            /// </summary>
            public string ImageGuid
            {
                get
                {
                    return GetAttributeValueIfExists( _imageUrlAttributeKey );
                }
                set
                {
                    SetAttributeValueIfExists( _imageUrlAttributeKey, value );
                }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ContentLibraryItemAttributes"/> class.
            /// </summary>
            /// <param name="contentChannelItem">The content channel item.</param>
            /// <param name="contentLibraryConfiguration">The content library configuration.</param>
            /// <exception cref="ArgumentNullException">nameof( contentLibraryConfiguration )</exception>
            public ContentLibraryItemAttributes( ContentChannelItem contentChannelItem, ContentLibraryConfiguration contentLibraryConfiguration )
            {
                _contentChannelItem = contentChannelItem ?? throw new ArgumentNullException( nameof( contentChannelItem ) );

                if ( contentLibraryConfiguration == null )
                {
                    throw new ArgumentNullException( nameof( contentLibraryConfiguration ) );
                }

                _summaryAttributeKey = contentChannelItem.Attributes.Values.FirstOrDefault( a => a.Guid == contentLibraryConfiguration.SummaryAttributeGuid ) ?.Key;
                _authorNameAttributeKey = contentChannelItem.Attributes.Values.FirstOrDefault( a => a.Guid == contentLibraryConfiguration.AuthorAttributeGuid )?.Key;
                _imageUrlAttributeKey = contentChannelItem.Attributes.Values.FirstOrDefault( a => a.Guid == contentLibraryConfiguration.ImageAttributeGuid )?.Key;
            }

            /// <summary>
            /// Saves the attributes.
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            public void SaveAttributes( RockContext rockContext )
            {
                _contentChannelItem.SaveAttributeValues( rockContext );
            }

            /// <summary>
            /// Gets the attribute value if exists.
            /// </summary>
            /// <param name="key">The attribute key.</param>
            /// <returns>The attribute value or null.</returns>
            private string GetAttributeValueIfExists( string key )
            {
                if ( key.IsNotNullOrWhiteSpace() )
                {
                    return _contentChannelItem.GetAttributeValue( key );
                }

                return null;
            }

            /// <summary>
            /// Sets the attribute value if exists.
            /// </summary>
            /// <param name="key">The attribute key.</param>
            /// <param name="value">The attribute value.</param>
            private void SetAttributeValueIfExists( string key, string value )
            {
                if ( key.IsNotNullOrWhiteSpace() )
                {
                    _contentChannelItem.SetAttributeValue( key, value );
                }
            }
        }

        #endregion

        #region IHasAdditionalSettings Models

        /// <summary>
        /// Content channel item intent settings.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.4" )]
        public class IntentSettings
        {
            /// <summary>
            /// Interaction intent defined value identifiers.
            /// </summary>
            public List<int> InteractionIntentValueIds { get; set; }
        }

        #endregion IHasAdditionalSettings Models
    }
}
