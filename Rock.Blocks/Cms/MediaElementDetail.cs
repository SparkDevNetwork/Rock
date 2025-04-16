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
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
using System.Data;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
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

    [Rock.SystemGuid.EntityTypeGuid( "6a7052f9-94df-4244-bbf0-db688c3acbbc" )]
    [Rock.SystemGuid.BlockTypeGuid( "d481ae29-a6aa-49f4-9dbb-d3fdf0995ca3" )]
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
                // Existing entity was found, prepare for view mode by default.
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
                // New entity is being created, prepare for edit mode by default.
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
                MediaFolderId = entity.MediaFolderId,
                MetricData = entity.MetricData,
                Name = entity.Name,
                SourceCreatedDateTime = entity.SourceCreatedDateTime,
                SourceData = entity.SourceData,
                SourceKey = entity.SourceKey,
                SourceModifiedDateTime = entity.SourceModifiedDateTime,
                ThumbnailDataJson = entity.ThumbnailDataJson,
                TranscriptionText = entity.TranscriptionText,
            };

            var rockContext = new RockContext();
            var mediaElementInteractions = GetInteractions(entity, null, rockContext);
            GetStandardKpiMetrics(entity, mediaElementInteractions, bag);

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

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        //// <inheritdoc/>
        protected override MediaElementBag GetEntityBagForEdit( MediaElement entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );

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

            //box.IfValidProperty( nameof( box.Bag.DefaultFileUrl ),
            //    () => entity.DefaultFileUrl = box.Bag.DefaultFileUrl );

            //box.IfValidProperty( nameof( box.Bag.DefaultThumbnailUrl ),
            //    () => entity.DefaultThumbnailUrl = box.Bag.DefaultThumbnailUrl );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.DurationSeconds ),
                () => entity.DurationSeconds = box.Bag.DurationSeconds );

            box.IfValidProperty( nameof( box.Bag.FileDataJson ),
                () => entity.FileDataJson = box.Bag.FileDataJson );

            box.IfValidProperty( nameof( box.Bag.MediaFolder ),
                () => entity.MediaFolderId = box.Bag.MediaFolder.GetEntityId<MediaFolder>( RockContext ).Value );

            box.IfValidProperty( nameof( box.Bag.MediaFolderId ),
                () => entity.MediaFolderId = box.Bag.MediaFolderId );

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

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                } );

            return true;
        }

        /// <summary>
        /// Gets all of the interactions that have watch map data for the media element.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="newerThanDate">The optional date that interactions must be newer than.</param>
        /// <param name="rockContext">The rock context to load from.</param>
        /// <returns>A list of interaction data.</returns>
        private static List<InteractionData> GetInteractions(MediaElement mediaElement, DateTime? newerThanDate, RockContext rockContext)
        {
            var interactionChannelId = InteractionChannelCache.Get(Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS).Id;

            // Get all the interactions for this media element and then pull
            // in just the columns we need for performance.
            var interactionData = new InteractionService(rockContext).Queryable()
                .Where(i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && (!newerThanDate.HasValue || i.InteractionDateTime > newerThanDate.Value)
                    && i.InteractionComponent.EntityId == mediaElement.Id
                    && i.Operation == "WATCH")
                .Select(i => new
                {
                    i.Id,
                    i.InteractionDateTime,
                    i.InteractionData
                })
                .ToList();

            // Do some post-processing on the data to parse the watch map and
            // filter out any that had invalid watch maps.
            return interactionData
                .Select(i => new InteractionData
                {
                    InteractionDateTime = i.InteractionDateTime,
                    WatchMap = i.InteractionData.FromJsonOrNull<WatchMapData>()
                })
                .Where(i => i.WatchMap != null && i.WatchMap.WatchedPercentage > 0)
                .ToList();
        }

        private static List<string> GetStandardKpiMetrics(MediaElement mediaElement, List<InteractionData> interactions, MediaElementBag mediaElementBag)
        {
            var kpiMetrics = new List<string>();

            var engagement = interactions.Sum(a => a.WatchMap.WatchedPercentage) / interactions.Count();
            mediaElementBag.EngagementStat = engagement;

            var playCount = interactions.Count();
            var playCountText = GetFormattedNumber(playCount);
            mediaElementBag.PlayCountText = playCountText;
    

            var minutesWatched = (int)(interactions.Sum(a => a.WatchMap.WatchedPercentage) * (mediaElement.DurationSeconds ?? 0) / 100 / 60);
            var minutesWatchedText = GetFormattedNumber(minutesWatched);
            mediaElementBag.MinutesWatchedText = minutesWatchedText;

            return kpiMetrics;
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

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
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

        /// <summary>
        /// Gets the formatted number in thousands and millions. That is, if
        /// number is more than 1,000 then divide by 1,000 and append "K".
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string that represents the formatted number.</returns>
        private static string GetFormattedNumber(long value)
        {
            if (value >= 1000000)
            {
                return $"{value / 1000000f:n2}M";
            }
            else if (value >= 1000)
            {
                return $"{value / 1000f:n2}K";
            }
            else
            {
                return value.ToString("n0");
            }
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

            entity.LoadAttributes( RockContext );

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

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateMediaElement( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.MediaElementId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

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

        #region Analytics View Support Classes

        /// <summary>
        /// Holds the data retrieved from the database so we can pass it
        /// around to different methods.
        /// </summary>
        private class InteractionData
        {
            /// <summary>
            /// Gets or sets the interaction date time.
            /// </summary>
            /// <value>
            /// The interaction date time.
            /// </value>
            public DateTime InteractionDateTime { get; set; }

            /// <summary>
            /// Gets or sets the watch map.
            /// </summary>
            /// <value>
            /// The watch map.
            /// </value>
            public WatchMapData WatchMap { get; set; }
        }

        /// <summary>
        /// Used to record grouped interaction data into a single date.
        /// </summary>
        private class InteractionDataForDate
        {
            /// <summary>
            /// Gets or sets the date this data is for.
            /// </summary>
            /// <value>
            /// The date this data is for.
            /// </value>
            public DateTime Date { get; set; }

            /// <summary>
            /// Gets or sets the number of interactions for this date.
            /// </summary>
            /// <value>
            /// The number of interactions for this date.
            /// </value>
            public long Count { get; set; }

            /// <summary>
            /// Gets or sets the average engagement for this date.
            /// </summary>
            /// <value>
            /// The average engagement for this date.
            /// </value>
            public double Engagement { get; set; }

            /// <summary>
            /// Gets or sets the number of minutes watched for this date.
            /// </summary>
            /// <value>
            /// The minutes number of watched for this date.
            /// </value>
            public int MinutesWatched { get; set; }
        }

        /// <summary>
        /// Watch Map Data that has been saved into the interaction.
        /// </summary>
        private class WatchMapData
        {
            /// <summary>
            /// Gets or sets the watch map run length encoded data.
            /// </summary>
            /// <value>
            /// The watch map.
            /// </value>
            public string WatchMap
            {
                get
                {
                    return _watchMap;
                }
                set
                {
                    _watchMap = value;

                    var segments = new List<WatchSegment>();
                    var segs = _watchMap.Split(',');

                    foreach (var seg in segs)
                    {
                        if (seg.Length < 2)
                        {
                            continue;
                        }

                        var duration = seg.Substring(0, seg.Length - 1).AsInteger();
                        var count = seg.Substring(seg.Length - 1).AsInteger();

                        segments.Add(new WatchSegment { Duration = duration, Count = count });
                    }

                    Segments = segments;
                }
            }
            private string _watchMap;

            /// <summary>
            /// Gets or sets the watched percentage.
            /// </summary>
            /// <value>
            /// The watched percentage.
            /// </value>
            public double WatchedPercentage { get; set; }

            /// <summary>
            /// The segments
            /// </summary>
            /// <remarks>
            /// Defined as field instead of property as it is accessed
            /// millions of times per page load.
            /// </remarks>
            public IReadOnlyList<WatchSegment> Segments;

            /// <summary>
            /// Gets the count at the specified position of the segment map.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <returns>The count or 0 if not found.</returns>
            public int GetCountAtPosition(int position)
            {
                // Walk through each segment until we find a segment that
                // "owns" the position we are interested in. Generally
                // speaking, there should probably always be less than 4 or 5
                // segments so this should be pretty fast.
                foreach (var segment in Segments)
                {
                    // This segment owns the position we care about.
                    if (position < segment.Duration)
                    {
                        return segment.Count;
                    }

                    // Decrement the position before moving to the next segment.
                    position -= segment.Duration;

                    if (position < 0)
                    {
                        break;
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// Individual segment of the watch map. Use fields instead of properties
        /// for performance reasons as they can be access tens of millions of
        /// times per page load.
        /// </summary>
        private class WatchSegment
        {
            /// <summary>
            /// The duration of this segment in seconds.
            /// </summary>
            public int Duration;

            /// <summary>
            /// The number of times this segment was watched.
            /// </summary>
            public int Count;
        }

        private class PageContext
        {
            public DateTime LastDate { get; set; }

            public int LastId { get; set; }
        }

        /// <summary>
        /// DateTimeParameterInterceptor fixes the incorrect behavior of
        /// Entity Framework library when for datetime columns it's generating
        /// datetime2(7) parameters  when using SQL Server 2016 and greater.
        /// Because of that, there were date comparison issues.
        /// Without this, there are rounding errors if the date time in the
        /// database has a value of 7 in the thousands milliseconds position.
        /// Links:
        /// https://github.com/aspnet/EntityFramework6/issues/49
        /// https://github.com/aspnet/EntityFramework6/issues/578
        /// </summary>
        public class DateTimeParameterInterceptor : DbCommandInterceptor
        {
            private readonly RockContext _context;

            public DateTimeParameterInterceptor(RockContext rockContext)
            {
                _context = rockContext;
            }

            public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
            {
                if (interceptionContext.DbContexts.Any(db => db == _context))
                {
                    ChangeDateTime2ToDateTime(command);
                }
            }

            public static void UseWith(RockContext rockContext, Action action)
            {
                var interceptor = new DateTimeParameterInterceptor(rockContext);
                DbInterception.Add(interceptor);

                try
                {
                    action();
                }
                finally
                {
                    DbInterception.Remove(interceptor);
                }
            }

            private static void ChangeDateTime2ToDateTime(DbCommand command)
            {
                command.Parameters
                    .OfType<SqlParameter>()
                    .Where(p => p.SqlDbType == SqlDbType.DateTime2)
                    .Where(p => p.Value != DBNull.Value)
                    .Where(p => p.Value is DateTime)
                    .Where(p => p.Value as DateTime? != DateTime.MinValue)
                    .ToList()
                    .ForEach(p => p.SqlDbType = SqlDbType.DateTime);
            }
        }
        #endregion

    }
}