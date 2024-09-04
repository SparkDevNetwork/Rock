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
using System.Data.SqlClient;
using System.Linq;

using Rock.Attribute;
using Rock.Cms;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.PageShortLinkDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular page short link.
    /// </summary>

    [DisplayName( "Page Short Link Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular page short link." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField(
        "Minimum Token Length",
        Key = AttributeKey.MinimumTokenLength,
        Description = "The minimum number of characters for the token.",
        IsRequired = false,
        DefaultIntegerValue = 7,
        Order = 0 )]

    #endregion

    [InitialBlockHeight( 700 )]
    [Rock.SystemGuid.EntityTypeGuid( "AD614123-C7CA-40EE-B5D5-64D0D1C91378" )]
    [Rock.SystemGuid.BlockTypeGuid( "72EDDF3D-625E-40A9-A68B-76236E77A3F3" )]
    public class PageShortLinkDetail : RockEntityDetailBlockType<PageShortLink, PageShortLinkBag>
    {
        private int _minTokenLength = 7;

        #region Keys

        private static class AttributeKey
        {
            public const string MinimumTokenLength = "MinimumTokenLength";
        }

        private static class PageParameterKey
        {
            public const string ShortLinkId = "ShortLinkId";
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
            _minTokenLength = GetAttributeValue( AttributeKey.MinimumTokenLength ).AsIntegerOrNull() ?? 7;

            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<PageShortLinkBag, PageShortLinkDetailOptionsBag>();

                SetBoxInitialEntityState( box );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<PageShortLink>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private PageShortLinkDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new PageShortLinkDetailOptionsBag
            {
                SiteOptions = Web.Cache.SiteCache
                    .All()
                    .Where( site => site.EnabledForShortening )
                    .OrderBy( site => site.Name )
                    .Select( site => new ListItemBag
                    {
                        Value = site.Guid.ToString(),
                        Text = site.Name
                    } )
                   .ToList(),
            };

            return options;
        }

        /// <summary>
        /// Validates the PageShortLink for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="pageShortLink">The PageShortLink to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the PageShortLink is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePageShortLink( PageShortLink pageShortLink, out string errorMessage )
        {
            errorMessage = null;

            // should have a token of minimum length
            var minTokenLength = GetAttributeValue( AttributeKey.MinimumTokenLength ).AsIntegerOrNull() ?? 7;
            if ( pageShortLink.Token.Length < minTokenLength )
            {
                errorMessage = string.Format( "Please enter a token that is a least {0} characters long.", minTokenLength );
                return false;
            }

            // should have a token that is unique for the siteId
            var service = new PageShortLinkService( RockContext );
            bool isTokenUsedBySite = !service.VerifyUniqueToken( pageShortLink.SiteId, pageShortLink.Id, pageShortLink.Token );
            if ( isTokenUsedBySite )
            {
                errorMessage = "The selected token is already being used. Please enter a different token.";
                return false;
            }

            if ( pageShortLink.SiteId == 0 )
            {
                errorMessage = "Please select a valid site.";
                return false;
            }

            if ( pageShortLink.Url.IsNullOrWhiteSpace() )
            {
                errorMessage = "Please enter a valid URL.";
                return false;
            }

            var scheduleData = pageShortLink.GetScheduleData();
            if ( scheduleData.Schedules != null )
            {
                foreach ( var schedule in scheduleData.Schedules )
                {
                    if ( !schedule.ScheduleId.HasValue && schedule.CustomCalendarContent.IsNullOrWhiteSpace() )
                    {
                        errorMessage = "Scheduled redirect does not have a valid schedule.";
                        return false;
                    }
                }
            }

            if ( !pageShortLink.IsValid )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<PageShortLinkBag, PageShortLinkDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity != null )
            {
                var isViewable = BlockCache.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
                box.IsEditable = BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

                entity.LoadAttributes( RockContext );

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        box.Entity = GetEntityBagForView( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToView( PageShortLink.FriendlyTypeName );
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
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( PageShortLink.FriendlyTypeName );
                    }
                }
            }
            else
            {
                box.ErrorMessage = $"The {PageShortLink.FriendlyTypeName} was not found.";
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="PageShortLinkBag"/> that represents the entity.</returns>
        private PageShortLinkBag GetCommonEntityBag( PageShortLink entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new PageShortLinkBag
            {
                IdKey = entity.IdKey,
                Site = entity.Site.ToListItemBag(),
                Token = entity.Token,
                Url = entity.Url,
                ScheduledRedirects = entity.GetScheduleData()
                    .Schedules
                    ?.Select( ConvertToScheduledRedirectBag )
                    .ToList()
            };
        }

        /// <inheritdoc/>
        protected override PageShortLinkBag GetEntityBagForView( PageShortLink entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.CopyLink = entity.Site
                .DefaultDomainUri
                .ToString()
                .EnsureTrailingForwardslash()
                + entity.Token;

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override PageShortLinkBag GetEntityBagForEdit( PageShortLink entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            var utmSettings = entity.GetAdditionalSettings<UtmSettings>();

            bag.UtmSettings = new UtmSettingsBag
            {
                UtmSource = DefinedValueCache.Get( utmSettings.UtmSourceValueId ?? 0 ).ToListItemBag(),
                UtmMedium = DefinedValueCache.Get( utmSettings.UtmMediumValueId ?? 0 ).ToListItemBag(),
                UtmCampaign = DefinedValueCache.Get( utmSettings.UtmCampaignValueId ?? 0 ).ToListItemBag(),
                UtmTerm = utmSettings.UtmTerm ?? "",
                UtmContent = utmSettings.UtmContent ?? ""
            };

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( PageShortLink entity, ValidPropertiesBox<PageShortLinkBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Site ),
                () => entity.SiteId = box.Bag.Site.GetEntityId<Site>( RockContext ).ToIntSafe() );

            box.IfValidProperty( nameof( box.Bag.Token ),
                () => entity.Token = box.Bag.Token );

            box.IfValidProperty( nameof( box.Bag.Url ),
                () => entity.Url = box.Bag.Url );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            box.IfValidProperty( nameof( box.Bag.UtmSettings ), () =>
            {
                var utmSettings = entity.GetAdditionalSettings<UtmSettings>();

                utmSettings.UtmSourceValueId = DefinedValueCache.Get( ( box.Bag.UtmSettings.UtmSource?.Value ).AsGuid(), RockContext )?.Id;
                utmSettings.UtmMediumValueId = DefinedValueCache.Get( ( box.Bag.UtmSettings.UtmMedium?.Value ).AsGuid(), RockContext )?.Id;
                utmSettings.UtmCampaignValueId = DefinedValueCache.Get( ( box.Bag.UtmSettings.UtmCampaign?.Value ).AsGuid(), RockContext )?.Id;
                utmSettings.UtmTerm = box.Bag.UtmSettings.UtmTerm;
                utmSettings.UtmContent = box.Bag.UtmSettings.UtmContent;

                entity.SetAdditionalSettings( utmSettings );
            } );

            box.IfValidProperty( nameof( box.Bag.ScheduledRedirects ), () =>
            {
                var scheduleData = entity.GetScheduleData();

                scheduleData.Schedules = box.Bag.ScheduledRedirects
                    ?.Select( ConvertToShortLinkSchedule )
                    .ToList();

                entity.SetScheduleData( scheduleData );
            } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="PageShortLink"/> to be viewed or edited on the page.</returns>
        protected override PageShortLink GetInitialEntity()
        {
            return GetInitialEntity<PageShortLink, PageShortLinkService>( RockContext, PageParameterKey.ShortLinkId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out PageShortLink entity, out BlockActionResult error )
        {
            var entityService = new PageShortLinkService( RockContext );
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
                entity = new PageShortLink();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{PageShortLink.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionForbidden( $"Not authorized to edit ${PageShortLink.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a <see cref="PageShortLinkSchedule"/> object to a bag that can
        /// be sent down to the client.
        /// </summary>
        /// <param name="scheduleData">The object to be converted.</param>
        /// <returns>A new instance of <see cref="ScheduledRedirectBag"/>.</returns>
        private ScheduledRedirectBag ConvertToScheduledRedirectBag( PageShortLinkSchedule scheduleData )
        {
            var scheduleBag = new ScheduledRedirectBag
            {
                PurposeKey = scheduleData.PurposeKey,
                Url = scheduleData.Url,
                UtmSettings = new UtmSettingsBag
                {
                    UtmSource = DefinedValueCache.Get( scheduleData.UtmSettings?.UtmSourceValueId ?? 0 ).ToListItemBag(),
                    UtmMedium = DefinedValueCache.Get( scheduleData.UtmSettings?.UtmMediumValueId ?? 0 ).ToListItemBag(),
                    UtmCampaign = DefinedValueCache.Get( scheduleData.UtmSettings?.UtmCampaignValueId ?? 0 ).ToListItemBag(),
                    UtmTerm = scheduleData.UtmSettings?.UtmTerm ?? "",
                    UtmContent = scheduleData.UtmSettings?.UtmContent ?? ""
                }
            };

            if ( scheduleData.ScheduleId.HasValue )
            {
                var schedule = NamedScheduleCache.Get( scheduleData.ScheduleId.Value, RockContext );

                if ( schedule != null )
                {
                    scheduleBag.NamedSchedule = schedule.ToListItemBag();
                    scheduleBag.ScheduleText = schedule.Name;
                    scheduleBag.ScheduleRangeText = GetScheduleRangeText( schedule.EffectiveStartDate, schedule.EffectiveEndDate );
                }
                else
                {
                    scheduleBag.ScheduleText = "No Schedule";
                    scheduleBag.ScheduleRangeText = "No Schedule";
                }
            }
            else
            {
                var tempSchedule = new Schedule
                {
                    iCalendarContent = scheduleData.CustomCalendarContent
                };
                tempSchedule.EnsureEffectiveStartEndDates();

                scheduleBag.CustomCalendarContent = scheduleData.CustomCalendarContent;
                scheduleBag.ScheduleText = tempSchedule.ToFriendlyScheduleText( true );
                scheduleBag.ScheduleRangeText = GetScheduleRangeText( tempSchedule.EffectiveStartDate, tempSchedule.EffectiveEndDate );
            }

            return scheduleBag;
        }

        /// <summary>
        /// Converts a <see cref="ScheduledRedirectBag"/> bag to an object that
        /// can be stored in the database.
        /// </summary>
        /// <param name="scheduleBag">The object to be converted.</param>
        /// <returns>A new instance of <see cref="PageShortLinkSchedule"/>.</returns>
        private PageShortLinkSchedule ConvertToShortLinkSchedule( ScheduledRedirectBag scheduleBag )
        {
            var scheduleData = new PageShortLinkSchedule
            {
                PurposeKey = scheduleBag.PurposeKey,
                Url = scheduleBag.Url,
                UtmSettings = new UtmSettings
                {
                    UtmSourceValueId = DefinedValueCache.Get( ( scheduleBag.UtmSettings.UtmSource?.Value ).AsGuid(), RockContext )?.Id,
                    UtmMediumValueId = DefinedValueCache.Get( ( scheduleBag.UtmSettings.UtmMedium?.Value ).AsGuid(), RockContext )?.Id,
                    UtmCampaignValueId = DefinedValueCache.Get( ( scheduleBag.UtmSettings.UtmCampaign?.Value ).AsGuid(), RockContext )?.Id,
                    UtmTerm = scheduleBag.UtmSettings.UtmTerm,
                    UtmContent = scheduleBag.UtmSettings.UtmContent
                }
            };

            if ( scheduleBag.NamedSchedule != null )
            {
                var scheduleGuid = scheduleBag.NamedSchedule.Value.AsGuidOrNull();

                if ( scheduleGuid.HasValue )
                {
                    scheduleData.ScheduleId = NamedScheduleCache.Get( scheduleGuid.Value, RockContext )?.Id;
                }
            }
            else
            {
                scheduleData.CustomCalendarContent = scheduleBag.CustomCalendarContent;
            }

            return scheduleData;
        }

        /// <summary>
        /// Gets the text that describes the date range this schedule is valid for.
        /// </summary>
        /// <param name="effectiveStartDate">The date this schedule begins.</param>
        /// <param name="effectiveEndDate">The date this schedule ends.</param>
        /// <returns>A string that represents the date ranges.</returns>
        private static string GetScheduleRangeText( DateTime? effectiveStartDate, DateTime? effectiveEndDate )
        {
            if ( !effectiveStartDate.HasValue )
            {
                return "Unknown";
            }

            if ( !effectiveEndDate.HasValue )
            {
                return $"{effectiveStartDate:d} - Ongoing";
            }

            return $"{effectiveStartDate:d} - {effectiveEndDate:d}";
        }

        /// <summary>
        /// Gets the data that will be used to display an analytics chart of
        /// usage counts for the specified short link.
        /// </summary>
        /// <param name="shortLinkId">The identifier of the <see cref="PageShortLink"/>.</param>
        /// <param name="startDateTime">The inclusive minimum date to include in the results.</param>
        /// <param name="endDateTime">The exclusive maximum date to include in the results.</param>
        /// <param name="dateBucket">A string that represents how to bucket the counts by date: <c>HOUR</c>, <c>DAY</c>, <c>WEEK</c> or <c>MONTH</c>.</param>
        /// <param name="partitionBy">The type of partitioning to do to further sub-divide the results.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A list of <see cref="ChartRow"/> objects that represent the aggregated results.</returns>
        private static List<ChartRow> GetChartData( int shortLinkId, DateTime startDateTime, DateTime endDateTime, string dateBucket, string partitionBy, RockContext rockContext )
        {
            var shortLinkMediumValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_URLSHORTENER.AsGuid(), rockContext )?.Id;
            DateTime origin;
            Func<DateTime, DateTime> nextInterval;
            var bucketWidth = 1;
            string partitionBySql;

            if ( dateBucket == "HOUR" )
            {
                origin = new DateTime( startDateTime.Year, startDateTime.Month, startDateTime.Day, startDateTime.Hour, 0, 0 );
                nextInterval = d => d.AddHours( 1 );
            }
            else if ( dateBucket == "DAY" )
            {
                origin = new DateTime( startDateTime.Year, startDateTime.Month, startDateTime.Day );
                nextInterval = d => d.AddDays( 1 );
            }
            else if ( dateBucket == "WEEK" )
            {
                origin = new DateTime( startDateTime.Year, startDateTime.Month, startDateTime.Day ).StartOfWeek( Rock.Web.SystemSettings.StartDayOfWeek );
                nextInterval = d => d.AddDays( 7 );
            }
            else if ( dateBucket == "MONTH" )
            {
                origin = new DateTime( startDateTime.Year, startDateTime.Month, 1 );
                nextInterval = d => d.AddMonths( 1 );
            }
            else
            {
                throw new Exception( "Invalid date bucket type." );
            }

            if ( partitionBy == "PurposeKey" )
            {
                partitionBySql = "[I].[ChannelCustomIndexed1]";
            }
            else if ( partitionBy == "Source" )
            {
                partitionBySql = "[I].[Source]";
            }
            else if ( partitionBy == "Medium" )
            {
                partitionBySql = "[I].[Medium]";
            }
            else if ( partitionBy == "Campaign" )
            {
                partitionBySql = "[I].[Campaign]";
            }
            else
            {
                throw new Exception( "Invalid partition type." );
            }

            var sql = $@"
SELECT
    [Bucket]
    , [Partition]
    , COUNT(*) AS [Count]
FROM (
    SELECT
        DATEADD(
            {dateBucket},
            ( DATEDIFF({dateBucket}, @Origin, [I].[InteractionDateTime])
                - CASE
                WHEN DATEADD(
                    {dateBucket},
                    DATEDIFF({dateBucket}, @Origin, [I].[InteractionDateTime]),
                    @Origin) > [I].[InteractionDateTime]
                THEN 1
                ELSE 0
                END )
                / @BucketWidth * @BucketWidth,
            @Origin) AS [Bucket]
        , ISNULL({partitionBySql}, '') AS [Partition]
    FROM [Interaction] AS [I]
    INNER JOIN [InteractionComponent] AS [IComp] ON [IComp].[Id] = [I].[InteractionComponentId]
    INNER JOIN [InteractionChannel] AS [IChan] ON [IChan].[Id] = [IComp].[InteractionChannelId]
    WHERE [IChan].[ChannelTypeMediumValueId] = @ShortLinkMediumValueId
    AND [I].[InteractionDateKey] >= @StartDateKey
    AND [I].[InteractionDateTime] >= @StartDateTime
    AND [I].[InteractionDateKey] <= @EndDateKey
    AND [I].[InteractionDateTime] < @EndDateTime
    AND [IComp].[EntityId] = @ShortLinkId
) AS [IQ]
GROUP BY [Bucket], [Partition]";

            var rows = rockContext.Database.SqlQuery<ChartRow>( sql,
                new SqlParameter( "ShortLinkMediumValueId", shortLinkMediumValueId ),
                new SqlParameter( "Origin", origin ),
                new SqlParameter( "BucketWidth", bucketWidth ),
                new SqlParameter( "ShortLinkId", shortLinkId ),
                new SqlParameter( "StartDateTime", startDateTime ),
                new SqlParameter( "StartDateKey", startDateTime.AsDateKey() ),
                new SqlParameter( "EndDateTime", endDateTime ),
                new SqlParameter( "EndDateKey", endDateTime.AsDateKey() ) );

            var results = new List<ChartRow>( rows );
            var allPartitions = results.Select( r => r.Partition ).Distinct().ToList();
            var nextExpectedDate = origin;

            // If we have no results at all, then include an empty partition
            // value so we can properly fill the buckets..
            if ( allPartitions.Count == 0 )
            {
                allPartitions.Add( string.Empty );
            }

            // Fill in any missing date buckets.
            for ( var date = origin; date <= endDateTime; date = nextInterval( date ) )
            {
                foreach ( var partition in allPartitions )
                {
                    if ( results.FirstOrDefault( r => r.Bucket == date && r.Partition == partition ) != null )
                    {
                        continue;
                    }

                    results.Add( new ChartRow
                    {
                        Bucket = date,
                        Partition = partition,
                        Count = 0
                    } );
                }
            }

            return results
                .OrderBy( r => r.Bucket )
                .ThenBy( r => r.Partition )
                .ToList();
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

            return ActionOk( new ValidPropertiesBox<PageShortLinkBag>
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
        public BlockActionResult Save( ValidPropertiesBox<PageShortLinkBag> box )
        {
            var entityService = new PageShortLinkService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidatePageShortLink( entity, out var validationMessage ) )
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
                    [PageParameterKey.ShortLinkId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            return ActionOk( GetEntityBagForView( entity) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new PageShortLinkService( rockContext );

                if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Gets a string that represents the date range of a named schedule.
        /// </summary>
        /// <param name="scheduleGuid">The unique identifier of the schedule.</param>
        /// <returns>A string that represents the date range.</returns>
        [BlockAction]
        public BlockActionResult GetNamedScheduleRange( Guid scheduleGuid )
        {
            var schedule = NamedScheduleCache.Get( scheduleGuid, RockContext );

            if ( schedule == null )
            {
                return ActionBadRequest( "Schedule not found." );
            }

            return ActionOk( GetScheduleRangeText( schedule.EffectiveStartDate, schedule.EffectiveEndDate ) );
        }

        /// <summary>
        /// Gets an instance of <see cref="ScheduledRedirectBag"/> that contains
        /// the schedule text and date range. Other properties are not set.
        /// </summary>
        /// <param name="calendarContent">The custom calendar content to build the schedule with.</param>
        /// <returns>An instance of <see cref="ScheduledRedirectBag"/>.</returns>
        [BlockAction]
        public BlockActionResult GetCustomScheduleRange( string calendarContent )
        {
            var tempSchedule = new Schedule
            {
                iCalendarContent = calendarContent
            };
            tempSchedule.EnsureEffectiveStartEndDates();

            var scheduleBag = new ScheduledRedirectBag
            {
                ScheduleText = tempSchedule.ToFriendlyScheduleText( true ),
                ScheduleRangeText = GetScheduleRangeText( tempSchedule.EffectiveStartDate, tempSchedule.EffectiveEndDate )
            };

            return ActionOk( scheduleBag );
        }

        /// <summary>
        /// Gets the data that will be used to display an analytics chart of
        /// usage counts for the specified short link.
        /// </summary>
        /// <param name="shortLinkId">The identifier key of the <see cref="PageShortLink"/>.</param>
        /// <param name="startDateTime">The inclusive minimum date and time to include in the results.</param>
        /// <param name="endDateTime">The exclusive maximum date and time to include in the results.</param>
        /// <param name="dateBucket">A string that represents how to bucket the counts by date: <c>HOUR</c>, <c>DAY</c>, <c>WEEK</c> or <c>MONTH</c>.</param>
        /// <param name="partitionBy">The type of partitioning to do to further sub-divide the results.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A list of <see cref="ChartRow"/> objects that represent the aggregated results.</returns>
        [BlockAction]
        public BlockActionResult GetShortLinkChartData( string shortLinkId, DateTimeOffset startDateTime, DateTimeOffset endDateTime, string dateBucket, string partitionBy )
        {
            var shortLinkIdNumber = IdHasher.Instance.GetId( shortLinkId );

            if ( !shortLinkIdNumber.HasValue )
            {
                return ActionBadRequest( "Invalid page short link." );
            }

            var data = GetChartData( shortLinkIdNumber.Value,
                startDateTime.ToOrganizationDateTime(),
                endDateTime.ToOrganizationDateTime(),
                dateBucket,
                partitionBy,
                RockContext );

            return ActionOk( data );
        }

        #endregion

        private class ChartRow
        {
            public DateTime Bucket { get; set; }

            public string Partition { get; set; }

            public int Count { get; set; }
        }
    }
}
