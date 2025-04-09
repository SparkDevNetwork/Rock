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
using Rock.Constants;
using Rock.Data;
using Rock.Media;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.MediaElementDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular media element.
    /// </summary>

    [DisplayName( "Media Element Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular media element." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "17be5040-bffa-4e07-9879-170fedeb14e7" )]
    [Rock.SystemGuid.BlockTypeGuid( "71633475-db86-4d7b-a62a-bf33254e6269" )]
    public class MediaElementDetail : RockEntityDetailBlockType<MediaElement, MediaElementBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string MediaElementId = "MediaElementId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<MediaElementBag, MediaElementDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private MediaElementDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new MediaElementDetailOptionsBag();
            return options;
        }

        /// <summary>
        /// Validates the MediaElement for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="mediaElement">The MediaElement to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the MediaElement is valid, <c>false</c> otherwise.</returns>
        private bool ValidateMediaElement( MediaElement mediaElement, out string errorMessage )
        {
            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<MediaElementBag, MediaElementDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {MediaElement.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id != 0 )
            {
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( MediaElement.FriendlyTypeName );
                }
            }
            else
            {
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( MediaElement.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="MediaElementBag"/> that represents the entity.</returns>
        private MediaElementBag GetCommonEntityBag( MediaElement entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new MediaElementBag
            {
                IdKey = entity.IdKey,
                CloseCaption = entity.CloseCaption,
                DefaultFileUrl = entity.DefaultFileUrl,
                DefaultThumbnailUrl = entity.DefaultThumbnailUrl,
                Description = entity.Description,
                DurationSeconds = entity.DurationSeconds,
                FileDataJson = entity.FileDataJson,
                MediaFolder = entity.MediaFolder.ToListItemBag(),
                MetricData = entity.MetricData,
                Name = entity.Name,
                SourceCreatedDateTime = entity.SourceCreatedDateTime,
                SourceData = entity.SourceData,
                SourceKey = entity.SourceKey,
                SourceModifiedDateTime = entity.SourceModifiedDateTime,
                ThumbnailDataJson = entity.ThumbnailDataJson,
                TranscriptionText = entity.TranscriptionText
            };

            var mediaFiles = entity.FileDataJson.FromJsonOrNull<List<MediaFileData>>() ?? new List<MediaFileData>();
            var thumbnailData = entity.ThumbnailDataJson.FromJsonOrNull<List<MediaElementThumbnailData>>() ?? new List<MediaElementThumbnailData>();

            // Set quality descriptions
            foreach (var file in mediaFiles)
            {
                if (int.TryParse(file.Quality, out var qualityInt))
                {
                    var qualityEnum = (MediaElementQuality)qualityInt;
                    file.QualityDescription = qualityEnum.GetDescription() ?? qualityEnum.ToString();
                }
                else if (Enum.TryParse<MediaElementQuality>(file.Quality, out var qualityEnum))
                {
                    file.QualityDescription = qualityEnum.GetDescription() ?? qualityEnum.ToString();
                }
            }

            bag.MediaFileGridData = GetMediaElementGridBuilder().Build(mediaFiles);
            bag.MediaFileGridDefinition = GetMediaElementGridBuilder().BuildDefinition();

            bag.ThumbnailGridData = GetThumbnailElementGridBuilder().Build(thumbnailData);
            bag.ThumbnailGridDefinition = GetThumbnailElementGridBuilder().BuildDefinition();

            return bag;
        }

        /// <inheritdoc/>
        protected override MediaElementBag GetEntityBagForView( MediaElement entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            return bag;
        }

        public class MediaFileData
        {
            public string PublicName { get; set; }
            public string Link { get; set; }
            public string Quality { get; set; }
            public string QualityDescription { get; set; }
            public string Format { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public int FPS { get; set; }
            public long Size { get; set; }
            public bool AllowDownload { get; set; }
        }

        private GridBuilder<MediaElementThumbnailData> GetThumbnailElementGridBuilder()
        {
            return new GridBuilder<MediaElementThumbnailData>()
                .AddField("link", a => a.Link)
                .AddField("width", a => a.Width)
                .AddField("Height", a => a.Height)
                .AddField("size", a => a.Size)
                .AddField("linkWithPlayButton", a => a.LinkWithPlayButton)
                .AddField("formattedFileSize", a => a.FormattedFileSize)
                .AddField("dimensions", a => a.Width > 0 && a.Height > 0 ? $"{a.Width}x{a.Height}" : "");
        }

        private GridBuilder<MediaFileData> GetMediaElementGridBuilder()
        {
            return new GridBuilder<MediaFileData>()
                .AddField("quality", a => a.QualityDescription ?? a.Quality)
                .AddField("format", a => a.Format)
                .AddField("dimensions", a => a.Width > 0 && a.Height > 0 ? $"{a.Width}x{a.Height}" : "")
                .AddField("size", a => FormatFileSize(a.Size))
                .AddField("allowDownload", a => a.AllowDownload)
                .AddField("link", a => a.Link);
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes <= 0) return "";
            const int scale = 1024;
            string[] units = { "Bytes", "KB", "MB", "GB", "TB" };
            int power = (int)Math.Log(bytes, scale);
            return $"{Math.Round(bytes / Math.Pow(scale, power), 2)} {units[power]}";
        }

        /// <inheritdoc/>
        protected override MediaElementBag GetEntityBagForEdit( MediaElement entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( MediaElement entity, ValidPropertiesBox<MediaElementBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.CloseCaption ),
                () => entity.CloseCaption = box.Bag.CloseCaption );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.DurationSeconds ),
                () => entity.DurationSeconds = box.Bag.DurationSeconds );

            box.IfValidProperty( nameof( box.Bag.FileDataJson ),
                () => entity.FileDataJson = box.Bag.FileDataJson );

            box.IfValidProperty( nameof( box.Bag.MediaFolder ),
                () => entity.MediaFolderId = box.Bag.MediaFolder.GetEntityId<MediaFolder>( RockContext ).Value );

            box.IfValidProperty( nameof( box.Bag.MetricData ),
                () => entity.MetricData = box.Bag.MetricData );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.SourceCreatedDateTime ),
                () => entity.SourceCreatedDateTime = box.Bag.SourceCreatedDateTime );

            box.IfValidProperty( nameof( box.Bag.SourceData ),
                () => entity.SourceData = box.Bag.SourceData );

            box.IfValidProperty( nameof( box.Bag.SourceKey ),
                () => entity.SourceKey = box.Bag.SourceKey );

            box.IfValidProperty( nameof( box.Bag.SourceModifiedDateTime ),
                () => entity.SourceModifiedDateTime = box.Bag.SourceModifiedDateTime );

            box.IfValidProperty( nameof( box.Bag.ThumbnailDataJson ),
                () => entity.ThumbnailDataJson = box.Bag.ThumbnailDataJson );

            box.IfValidProperty( nameof( box.Bag.TranscriptionText ),
                () => entity.TranscriptionText = box.Bag.TranscriptionText );

            return true;
        }

        /// <inheritdoc/>
        protected override MediaElement GetInitialEntity()
        {
            return GetInitialEntity<MediaElement, MediaElementService>( RockContext, PageParameterKey.MediaElementId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out MediaElement entity, out BlockActionResult error )
        {
            var entityService = new MediaElementService( RockContext );
            error = null;

            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                entity = new MediaElement();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{MediaElement.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${MediaElement.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<MediaElementBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<MediaElementBag> box )
        {
            var entityService = new MediaElementService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            if ( !ValidateMediaElement( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.MediaElementId] = entity.IdKey
                } ) );
            }

            entity = entityService.Get( entity.Id );
            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<MediaElementBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new MediaElementService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }

    public class MediaElementDetailOptionsBag
    {
    }
}