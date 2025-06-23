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
#if REVIEW_WEBFORMS
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
#endif
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
using Rock.Utility.ExtensionMethods;
using Rock.Web.UI;
using Rock.Web;
    
namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular media element.
    /// </summary>

    [DisplayName( "Media Element Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular media element." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "6a7052f9-94df-4244-bbf0-db688c3acbbc" )]
    [Rock.SystemGuid.BlockTypeGuid( "d481ae29-a6aa-49f4-9dbb-d3fdf0995ca3" )]
    public class MediaElementDetail : RockEntityDetailBlockType<MediaElement, MediaElementBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string MediaElementId = "MediaElementId";
            public const string MediaFolderId = "MediaFolderId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var breadCrumbs = new List<IBreadCrumb>();
                var pageParameters = new Dictionary<string, string>();
                var key = pageReference.GetPageParameter( PageParameterKey.MediaElementId );
                var additionalParameters = new Dictionary<string, string>();

                var data = new MediaElementService( rockContext )
                 .GetSelect( key, mf => new
                 {
                     mf.Name,
                     mf.MediaFolderId
                 } );

                if ( data != null )
                {
                    pageParameters.Add( PageParameterKey.MediaElementId, key );
                    additionalParameters.Add( PageParameterKey.MediaFolderId, data.MediaFolderId.ToString() );
                }

                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
                var breadCrumb = new BreadCrumbLink( data?.Name ?? "New Media Elment", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb> { breadCrumb },
                    AdditionalParameters = additionalParameters
                };
            }
        }

        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<MediaElementBag, MediaElementDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            RequestContext.Response.AddCssLink( "~/Styles/Blocks/Cms/MediaElementDetail.css", true);

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

            box.NavigationUrls = GetBoxNavigationUrls( entity );
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
                MetricData = entity.MetricData,
                Name = entity.Name,
                ThumbnailDataJson = entity.ThumbnailDataJson,
                TranscriptionText = entity.TranscriptionText,
            };

            var rockContext = new RockContext();
            var mediaElementInteractions = GetInteractions( entity, null, rockContext );
            GetStandardKpiMetrics( entity, mediaElementInteractions, bag );

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

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.DurationSeconds ),
                () => entity.DurationSeconds = box.Bag.DurationSeconds );

            box.IfValidProperty( nameof( box.Bag.FileDataJson ),
                () => entity.FileDataJson = box.Bag.FileDataJson );

            box.IfValidProperty( nameof( box.Bag.MetricData ),
                () => entity.MetricData = box.Bag.MetricData );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.ThumbnailDataJson ),
                () => entity.ThumbnailDataJson = box.Bag.ThumbnailDataJson );

            box.IfValidProperty( nameof( box.Bag.TranscriptionText ),
                () => entity.TranscriptionText = box.Bag.TranscriptionText );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                });

            return true;
        }

        /// <summary>
        /// Gets all of the interactions that have watch map data for the media element.
        /// </summary>
        /// <param name="mediaElement">The media element.</param>
        /// <param name="newerThanDate">The optional date that interactions must be newer than.</param>
        /// <param name="rockContext">The rock context to load from.</param>
        /// <returns>A list of interaction data.</returns>
        private static List<InteractionData> GetInteractions( MediaElement mediaElement, DateTime? newerThanDate, RockContext rockContext )
        {
            var interactionChannelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS ).Id;

            // Get all the interactions for this media element and then pull
            // in just the columns we need for performance.
            var interactionData = new InteractionService( rockContext ).Queryable()
                .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && ( !newerThanDate.HasValue || i.InteractionDateTime > newerThanDate.Value )
                    && i.InteractionComponent.EntityId == mediaElement.Id
                    && i.Operation == "WATCH" )
                .Select( i => new
                {
                    i.Id,
                    i.InteractionDateTime,
                    i.InteractionData
                } )
                .ToList();

            // Do some post-processing on the data to parse the watch map and
            // filter out any that had invalid watch maps.
            return interactionData
                .Select( i => new InteractionData
                {
                    InteractionDateTime = i.InteractionDateTime,
                    WatchMap = i.InteractionData.FromJsonOrNull<WatchMapData>()
                })
                .Where( i => i.WatchMap != null && i.WatchMap.WatchedPercentage > 0 )
                .ToList();
        }

        private static List<string> GetStandardKpiMetrics( MediaElement mediaElement, List<InteractionData> interactions, MediaElementBag mediaElementBag )
        {
            var kpiMetrics = new List<string>();

            var engagement = interactions.Sum( a => a.WatchMap.WatchedPercentage ) / interactions.Count();
            mediaElementBag.EngagementStat = engagement;

            var playCount = interactions.Count();
            var playCountText = GetFormattedNumber( playCount );
            mediaElementBag.PlayCountText = playCountText;
    

            var minutesWatched = ( int )( interactions.Sum( a => a.WatchMap.WatchedPercentage ) * ( mediaElement.DurationSeconds ?? 0 ) / 100 / 60 );
            var minutesWatchedText = GetFormattedNumber( minutesWatched );
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
        private Dictionary<string, string> GetBoxNavigationUrls( MediaElement entity )
        {
            var folderId = entity.MediaFolderId;

            if ( folderId == 0 )
            {
                folderId = PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull() ?? 0;
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    // if media element is being edit, use it's folder id
                    // new media element use page parameter
                    [PageParameterKey.MediaFolderId] = folderId.ToString()
                } )
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
                entity = new MediaElement();
                entity.MediaFolderId = PageParameter( PageParameterKey.MediaFolderId ).AsIntegerOrNull() ?? 0;
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
        private static string GetFormattedNumber( long value )
        {
            if ( value >= 1000000 )
            {
                return $"{value / 1000000f:n2}M";
            }
            else if ( value >= 1000 )
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

        /// <summary>
        /// Gets play count data based on the specified duration.
        /// </summary>
        /// <param name="duration">The duration in days (90 or 365).</param>
        /// <returns>An array of data points with play counts and dates.</returns>
        [BlockAction]
        public BlockActionResult GetPlayCount( int duration )
        {
            var rockContext = new RockContext();
            var mediaElementId = PageParameter( PageParameterKey.MediaElementId ).AsInteger();
            var mediaElement = new MediaElementService( rockContext ).Get( mediaElementId );

            if ( mediaElement == null )
            {
                return ActionBadRequest( "Media element not found." );
            }

            var dataPoints = new List<object>();
            var today = RockDateTime.Now.Date;

            // Get all interactions for the media element
            DateTime startDate;
            List<InteractionDataForDate> resultData;

            if ( duration == 365 )
            {
                startDate = today.AddDays( -365 );

                var currentDay = today;
                while ( currentDay.DayOfWeek != DayOfWeek.Monday )
                {
                    currentDay = currentDay.AddDays( -1 );
                }

                var interactions = GetInteractions( mediaElement, startDate, rockContext );
                    
                var interactionsByWeek = new Dictionary<DateTime, List<InteractionData>>();
                    
                for ( int i = 0; i < 52; i++ )
                {
                    var weekStart = currentDay.AddDays( -7 * i );
                    interactionsByWeek[weekStart] = new List<InteractionData>();
                }
                    
                foreach ( var interaction in interactions )
                {
                    var interactionDate = interaction.InteractionDateTime.Date;
                        
                    var dayOfWeek = interactionDate.DayOfWeek;
                    var daysToMonday = dayOfWeek == DayOfWeek.Sunday ? 6 : dayOfWeek - DayOfWeek.Monday;
                    var weekStart = interactionDate.AddDays( -daysToMonday );
                        
                    if (interactionsByWeek.ContainsKey( weekStart ) )
                    {
                        interactionsByWeek[weekStart].Add( interaction );
                    }
                }
                    
                resultData = interactionsByWeek.Select( week => new InteractionDataForDate
                {
                    Date = week.Key,
                    Count = week.Value.Count,
                    Engagement = week.Value.Any() ? week.Value.Sum( i => i.WatchMap.WatchedPercentage ) / week.Value.Count : 0,
                    MinutesWatched = ( int )(( mediaElement.DurationSeconds ?? 0 ) * week.Value.Sum( i => i.WatchMap.WatchedPercentage ) / 100 / 60 )
                })
                .OrderBy( d => d.Date )
                .ToList();
            }
            else
            {
                startDate = today.AddDays( -duration );
                    
                var interactions = GetInteractions( mediaElement, startDate, rockContext );
                    
                var interactionsByDay = interactions
                    .GroupBy( i => i.InteractionDateTime.Date )
                    .ToDictionary(
                        g => g.Key,
                        g => new InteractionDataForDate
                        {
                            Date = g.Key,
                            Count = g.Count(),
                            Engagement = g.Sum( i => i.WatchMap.WatchedPercentage ) / g.Count(),
                            MinutesWatched = ( int )(( mediaElement.DurationSeconds ?? 0 ) * g.Sum( i => i.WatchMap.WatchedPercentage ) / 100 / 60 )
                        }
                    );
                    
                resultData = new List<InteractionDataForDate>();
                for ( var date = startDate; date <= today; date = date.AddDays( 1 ) )
                {
                    if ( interactionsByDay.TryGetValue( date, out var data ) )
                    {
                        resultData.Add(data);
                    }
                    else
                    {
                        resultData.Add(new InteractionDataForDate
                        {
                            Date = date,
                            Count = 0,
                            Engagement = 0,
                            MinutesWatched = 0
                        });
                    }
                }
            }
                
            foreach ( var item in resultData )
            {
                dataPoints.Add( new object[]
                {
                    item.Count,
                    item.Date.ToString( "MM/dd/yyyy" )
                } );
            }

            // Return the array in chronological order (oldest to newest)
            return ActionOk( dataPoints.OrderBy( d => DateTime.Parse( ( ( object[]) d )[1].ToString())).ToArray());
        }

        /// <summary>
        /// Loads individual play data for a media element with pagination support
        /// </summary>
        /// <param name="mediaElementId">The ID of the media element</param>
        /// <param name="pageContext">Optional page context for pagination</param>
        /// <returns>A collection of interaction data</returns>
        [BlockAction]
        public BlockActionResult LoadIndividualPlays( string mediaElementId, string pageContext )
        {
            var interactionChannelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS ).Id;
            var rockContext = new RockContext();
            PageContext context = null;

            if ( pageContext.IsNotNullOrWhiteSpace() )
            {
                var jsonContext = System.Text.Encoding.UTF8.GetString( Convert.FromBase64String(pageContext) );
                context = jsonContext.FromJsonOrNull<PageContext>();
            }

            // Try to get the media element ID from the provided string ID
            int mediaElementIdValue;
            if ( !int.TryParse( mediaElementId, out mediaElementIdValue ) )
            {
                var guidValue = mediaElementId.AsGuidOrNull();
                if ( guidValue.HasValue )
                {
                    mediaElementIdValue = new MediaElementService( rockContext ).Get( guidValue.Value )?.Id ?? 0;
                }
                else
                {
                    mediaElementIdValue = new MediaElementService( rockContext ).Get( mediaElementId, !PageCache.Layout.Site.DisablePredictableIds )?.Id ?? 0;
                }
            }

            // Get all the interactions for this media element and then pull
            // in just the columns we need for performance.
            var interactionQuery = new InteractionService( rockContext ).Queryable()
                .Include( i => i.PersonAlias.Person )
                .Include( i => i.InteractionSession )
                .Include( i => i.InteractionSession.InteractionSessionLocation )
                .Include( i => i.InteractionSession.DeviceType )
                .Where( i => i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && i.Operation == "WATCH"
                    && i.InteractionComponent.EntityId == mediaElementIdValue );

            var filteredQuery = interactionQuery;

            if  (context != null )
            {
                // Query for the next page of results.
                // If the dates are the same, then take only older Ids.
                // If dates are not the same, then only take older dates.
                filteredQuery = filteredQuery
                    .Where( i => ( i.InteractionDateTime == context.LastDate && i.Id < context.LastId) || i.InteractionDateTime < context.LastDate );
            }

            List<Interaction> interactions = null;

            // Load the next 25 results.
#if REVIEW_WEBFORMS
            DateTimeParameterInterceptor.UseWith( rockContext, () =>
            {
                interactions = filteredQuery
                    .OrderByDescending( i => i.InteractionDateTime )
                    .ThenByDescending( i => i.Id )
                    .Take( 25 )
                    .ToList();
            });
#else
            interactions = filteredQuery
                .OrderByDescending( i => i.InteractionDateTime )
                .ThenByDescending( i => i.Id )
                .Take( 25 )
                .ToList();
#endif

            // If we got any results then figure out the next page context.
            if ( interactions.Count > 0 )
            {
                context = new PageContext
                {
                    LastDate = interactions.Last().InteractionDateTime,
                    LastId = interactions.Last().Id
                };

                pageContext = Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( context.ToJson() ) );
            }
            else
            {
                pageContext = null;
            }

            // Get the actual data to provide to the client.
            var data = interactions
                .Select( i => new
                {
                    DateTime = new DateTimeOffset( i.InteractionDateTime, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( i.InteractionDateTime ) ),
                    FullName = i.PersonAlias?.Person?.FullName ?? "Unknown",
                    PhotoUrl = i.PersonAlias?.Person?.PhotoUrl ?? "/Assets/Images/person-no-photo-unknown.svg",
                    Platform = "Unknown",
                    Data = i.InteractionData.FromJsonOrNull<WatchMapData>(),
                    Location = i.InteractionSession?.InteractionSessionLocation?.Location,
                    ClientType = i.InteractionSession?.DeviceType.ClientType,
                    Isp = i.InteractionSession?.InteractionSessionLocation?.ISP,
                    OperatingSystem = i.InteractionSession?.DeviceType?.OperatingSystem,
                    Application = i.InteractionSession?.DeviceType?.Application,
                    InteractionsCount = interactionQuery.Where( m => m.PersonAliasId != null && m.PersonAliasId == i.PersonAliasId ).Count()
                });

            return ActionOk(new {
                Items = data,
                NextPage = pageContext
            });
        }

        /// <summary>
        /// Gets detailed video engagement data for a media element
        /// </summary>
        /// <param name="mediaElementId">The ID of the media element</param>
        /// <returns>Detailed video engagement data including watched and rewatched metrics</returns>
        [BlockAction]
        public BlockActionResult GetVideoEngagementData( string mediaElementId )
        {
            var rockContext = new RockContext();
            
            // Try to get the media element ID from the provided string ID
            int mediaElementIdValue;
            if ( !int.TryParse( mediaElementId, out mediaElementIdValue ) )
            {
                var guidValue = mediaElementId.AsGuidOrNull();
                if ( guidValue.HasValue )
                {
                    mediaElementIdValue = new MediaElementService( rockContext ).Get( guidValue.Value )?.Id ?? 0;
                }
                else
                {
                    mediaElementIdValue = new MediaElementService( rockContext ).Get( mediaElementId, !PageCache.Layout.Site.DisablePredictableIds )?.Id ?? 0;
                }
            }

            // Get the media element
            var mediaElement = new MediaElementService( rockContext ).Get( mediaElementIdValue );
            if ( mediaElement == null )
            {
                return ActionBadRequest( "Media element not found." );
            }
            
            // Get all the interactions for this media element
            var interactionChannelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS ).Id;
            var interactions = GetInteractions( mediaElement, null, rockContext );
            
            if ( !interactions.Any() )
            {
                return ActionOk( new
                {
                    PlayCount = 0,
                    MinutesWatched = "0",
                    AverageWatchEngagement = "n/a",
                    Duration = mediaElement.DurationSeconds ?? 0,
                    Watched = new int[0],
                    Rewatched = new int[0]
                });
            }
            
            // Get the video data
            var videoData = GetVideoData( mediaElement, interactions );
            
            return ActionOk( videoData );
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
        /// Gets detailed video engagement data from interaction data
        /// </summary>
        /// <param name="mediaElement">The media element</param>
        /// <param name="interactions">The list of interactions</param>
        /// <returns>An object containing detailed video analytics data</returns>
        private static object GetVideoData( MediaElement mediaElement, List<InteractionData> interactions )
        {
            var duration = mediaElement.DurationSeconds ?? 0;
            var totalCount = interactions.Count;
            var totalSecondsWatched = 0;

            // Construct the arrays that will hold the value at each second.
            var engagementMap = new double[duration];
            var watchedMap = new int[duration];
            var rewatchedMap = new int[duration];

            totalSecondsWatched += ( int ) interactions.Select( i => i.WatchMap.WatchedPercentage / 100.0 * duration ).Sum();

            // Loop through every second of the video and build the maps.
            // Be cautious when modifying this code. A 70 minute video is
            // 4,200 seconds long, and if there are 10,000 watch maps to
            // process that means we are going to have 42 million calls to
            // GetCountAtPosition().
            for ( int second = 0; second < duration; second++ )
            {
                var counts = interactions.Select( i => i.WatchMap.GetCountAtPosition( second ) ).ToList();

                watchedMap[second] = counts.Count( a => a > 0 );
                rewatchedMap[second] = counts.Sum();
            }

            string averageWatchEngagement = "n/a";

            if ( duration > 0 )
            {
                averageWatchEngagement = string.Format( "{0}%", ( int ) ( totalSecondsWatched / duration * totalCount / 100.0 ) );
            }

            return new
            {
                PlayCount = totalCount,
                MinutesWatched = string.Format( "{0:n0}", totalSecondsWatched / 60 ),
                AverageWatchEngagement = averageWatchEngagement,
                Duration = duration,
                Watched = watchedMap,
                Rewatched = rewatchedMap
            };
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

                    foreach ( var seg in segs )
                    {
                        if ( seg.Length < 2 )
                        {
                            continue;
                        }

                        var duration = seg.Substring( 0, seg.Length - 1 ).AsInteger();
                        var count = seg.Substring( seg.Length - 1 ).AsInteger();

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
                foreach ( var segment in Segments )
                {
                    // This segment owns the position we care about.
                    if ( position < segment.Duration )
                    {
                        return segment.Count;
                    }

                    // Decrement the position before moving to the next segment.
                    position -= segment.Duration;

                    if ( position < 0 )
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

        /// <summary>
        /// Used for pagination when loading individual play data
        /// </summary>
        private class PageContext
        {
            /// <summary>
            /// Gets or sets the last date in the current page of results
            /// </summary>
            public DateTime LastDate { get; set; }

            /// <summary>
            /// Gets or sets the last ID in the current page of results
            /// </summary>
            public int LastId { get; set; }
        }

#if REVIEW_WEBFORMS
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
                if ( interceptionContext.DbContexts.Any( db => db == _context ) )
                {
                    ChangeDateTime2ToDateTime( command );
                }
            }

            public static void UseWith( RockContext rockContext, Action action )
            {
                var interceptor = new DateTimeParameterInterceptor( rockContext );
                DbInterception.Add( interceptor );

                try
                {
                    action();
                }
                finally
                {
                    DbInterception.Remove( interceptor );
                }
            }

            private static void ChangeDateTime2ToDateTime( DbCommand command )
            {
                command.Parameters
                    .OfType<SqlParameter>()
                    .Where (p => p.SqlDbType == SqlDbType.DateTime2 )
                    .Where( p => p.Value != DBNull.Value )
                    .Where( p => p.Value is DateTime )
                    .Where( p => p.Value as DateTime? != DateTime.MinValue )
                    .ToList()
                    .ForEach( p => p.SqlDbType = SqlDbType.DateTime );
            }
        }
#endif
        #endregion

    }
}