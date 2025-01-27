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

using Rock.Achievement;
using Rock.Attribute;
using Rock.Chart;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.AchievementTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular achievement type.
    /// </summary>

    [DisplayName( "Achievement Type Detail" )]
    [Category( "Achievements" )]
    [Description( "Displays the details of the given Achievement Type for editing." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "8b22d387-c8f3-41ff-99ef-ee4f088610a1" )]
    [Rock.SystemGuid.BlockTypeGuid( "eddfcaff-70aa-4791-b051-6567b37518c4" )]
    public class AchievementTypeDetail : RockEntityDetailBlockType<AchievementType, AchievementTypeBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string AchievementTypeId = "AchievementTypeId";
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
            var box = new DetailBlockBox<AchievementTypeBag, AchievementTypeDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();

            return box;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.AchievementTypeId );
            var achievementTypeId = Rock.Utility.IdHasher.Instance.GetId( key ) ?? key.AsInteger();
            var achievementType = AchievementTypeCache.Get( achievementTypeId );

            var breadCrumbs = new List<IBreadCrumb>();
            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
            breadCrumbs.Add( new BreadCrumbLink( achievementType?.Name ?? "New Achievement Type", breadCrumbPageRef ) );

            return new BreadCrumbResult
            {
                BreadCrumbs = breadCrumbs
            };
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private AchievementTypeDetailOptionsBag GetBoxOptions()
        {
            var options = new AchievementTypeDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the AchievementType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="achievementType">The AchievementType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AchievementType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAchievementType( AchievementType achievementType, out string errorMessage )
        {
            errorMessage = null;
            var isNew = achievementType.Id == 0;
            var sb = new StringBuilder( "<ul>" );
            var isValid = true;

            if ( !isNew )
            {
                var achievementTypeCache = GetAchievementTypeCache();
                var eligibleAchievementTypes = achievementTypeCache == null
                    ? new List<AchievementTypeCache>()
                    : AchievementTypeService.GetEligiblePrerequisiteAchievementTypeCaches( achievementTypeCache );

                foreach ( var prerequisite in achievementType.Prerequisites )
                {
                    if ( !eligibleAchievementTypes.Exists( stat => stat.Id == prerequisite.PrerequisiteAchievementTypeId ) )
                    {
                        isValid = false;

                        sb.AppendFormat(
                            "<li>This achievement type cannot have prerequisite \"{0}\" because it would create a circular dependency.</li>",
                            prerequisite.PrerequisiteAchievementType.Name )
                            .AppendLine();
                    }
                }
            }

            if ( achievementType.MaxAccomplishmentsAllowed > 1 && achievementType.AllowOverAchievement )
            {
                sb.Append( "<li>" )
                    .Append( nameof( achievementType.MaxAccomplishmentsAllowed ) )
                    .Append( " cannot be greater than 1 if " )
                    .Append( nameof( achievementType.AllowOverAchievement ) )
                    .Append( " is set." )
                    .AppendLine( "</li>" );
                isValid = false;
            }

            if ( achievementType.ComponentEntityTypeId <= 0 )
            {
                sb.Append( "<li>Select a valid Entity Type.</li>" );
                isValid = false;
            }

            if ( !achievementType.IsValid )
            {
                foreach ( var validationResult in achievementType.ValidationResults )
                {
                    sb.Append("<li>" ).Append( validationResult.ErrorMessage ).AppendLine( "</li>" );
                }
            }

            sb.AppendLine( "</ul>" );

            errorMessage = sb.ToString();

            return isValid;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AchievementTypeBag, AchievementTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AchievementType.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AchievementType.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AchievementType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AchievementTypeBag"/> that represents the entity.</returns>
        private AchievementTypeBag GetCommonEntityBag( AchievementType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new AchievementTypeBag
            {
                IdKey = entity.IdKey,
                AchievementEntityType = entity.AchievementEntityType.ToListItemBag(),
                AchievementFailureWorkflowType = entity.AchievementFailureWorkflowType.ToListItemBag(),
                AchievementIconCssClass = entity.AchievementIconCssClass,
                AchievementStartWorkflowType = entity.AchievementStartWorkflowType.ToListItemBag(),
                AchievementStepStatus = entity.AchievementStepStatus.ToListItemBag(),
                AchievementStepType = entity.AchievementStepType.ToListItemBag(),
                AchievementSuccessWorkflowType = entity.AchievementSuccessWorkflowType.ToListItemBag(),
                AllowOverAchievement = entity.AllowOverAchievement,
                AlternateImageBinaryFile = entity.AlternateImageBinaryFile.ToListItemBag(),
                Attempts = entity.Attempts.ToListItemBagList(),
                BadgeLavaTemplate = entity.BadgeLavaTemplate,
                Category = entity.Category.ToListItemBag(),
                CustomSummaryLavaTemplate = entity.CustomSummaryLavaTemplate,
                Description = entity.Description,
                HighlightColor = entity.HighlightColor,
                ImageBinaryFile = entity.ImageBinaryFile.ToListItemBag(),
                IsActive = entity.Id == 0 || entity.IsActive,
                IsPublic = entity.Id == 0 || entity.IsPublic,
                MaxAccomplishmentsAllowed = entity.MaxAccomplishmentsAllowed,
                Name = entity.Name,
                ResultsLavaTemplate = entity.ResultsLavaTemplate,
                SourceEntityTypeId = entity.SourceEntityTypeId,
                StepProgram = entity.AchievementStepStatus?.StepProgram?.ToListItemBag(),
                AddStepOnSuccess = entity.AchievementStepStatus != null
            };

            SetPrerequisiteListValues( bag );

            return bag;
        }

        /// <inheritdoc/>
        protected override AchievementTypeBag GetEntityBagForView( AchievementType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            var chartFactory = GetChartJsFactory( "Current||Year||" );

            if ( chartFactory.HasData )
            {
                var args = new ChartJsTimeSeriesDataFactory.GetJsonArgs
                {
                    DisplayLegend = false,
                    LineTension = 0m,
                    MaintainAspectRatio = false,
                    SizeToFitContainerWidth = true
                };

                bag.ChartDataJSON = chartFactory.GetChartDataJson( args );
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override AchievementTypeBag GetEntityBagForEdit( AchievementType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( AchievementType entity, ValidPropertiesBox<AchievementTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AchievementFailureWorkflowType ),
                () => entity.AchievementFailureWorkflowTypeId = box.Bag.AchievementFailureWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AchievementIconCssClass ),
                () => entity.AchievementIconCssClass = box.Bag.AchievementIconCssClass );

            box.IfValidProperty( nameof( box.Bag.AchievementStartWorkflowType ),
                () => entity.AchievementStartWorkflowTypeId = box.Bag.AchievementStartWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AchievementSuccessWorkflowType ),
                () => entity.AchievementSuccessWorkflowTypeId = box.Bag.AchievementSuccessWorkflowType.GetEntityId<WorkflowType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AllowOverAchievement ),
                () => entity.AllowOverAchievement = box.Bag.AllowOverAchievement );

            box.IfValidProperty( nameof( box.Bag.BadgeLavaTemplate ),
                () => entity.BadgeLavaTemplate = box.Bag.BadgeLavaTemplate );

            box.IfValidProperty( nameof( box.Bag.Category ),
                () => entity.CategoryId = box.Bag.Category.GetEntityId<Category>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CustomSummaryLavaTemplate ),
                () => entity.CustomSummaryLavaTemplate = box.Bag.CustomSummaryLavaTemplate );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.HighlightColor ),
                () => entity.HighlightColor = box.Bag.HighlightColor );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.IsPublic ),
                () => entity.IsPublic = box.Bag.IsPublic );

            box.IfValidProperty( nameof( box.Bag.MaxAccomplishmentsAllowed ),
                () => entity.MaxAccomplishmentsAllowed = box.Bag.MaxAccomplishmentsAllowed );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.ResultsLavaTemplate ),
                () => entity.ResultsLavaTemplate = box.Bag.ResultsLavaTemplate );

            box.IfValidProperty( nameof( box.Bag.AchievementEntityType ),
                () => entity.ComponentEntityTypeId = box.Bag.AchievementEntityType.GetEntityId<EntityType>( RockContext ) ?? 0 );

            box.IfValidProperty( nameof( box.Bag.ImageBinaryFile ),
                () => SaveImage( box, entity ) );

            box.IfValidProperty( nameof( box.Bag.AlternateImageBinaryFile ),
                () => SaveAlternateImage( box, entity ) );

            box.IfValidProperty( nameof( box.Bag.AddStepOnSuccess ),
                () => SaveStepDetails( box, entity ) );

            box.IfValidProperty( nameof( box.Bag.Prerequisites ),
                () => SavePrerequisites( box, entity ));

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        // <inheritdoc/>
        protected override AchievementType GetInitialEntity()
        {
            return GetInitialEntity<AchievementType, AchievementTypeService>( RockContext, PageParameterKey.AchievementTypeId );
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

        // <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out AchievementType entity, out BlockActionResult error )
        {
            var entityService = new AchievementTypeService( RockContext );
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
                entity = new AchievementType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AchievementType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AchievementType.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Loads the prerequisite list.
        /// </summary>
        private void SetPrerequisiteListValues( AchievementTypeBag bag, Guid? componentEntityTypeGuid = null )
        {
            var config = GetAchievementConfiguration( componentEntityTypeGuid );
            var achievementTypeCache = GetAchievementTypeCache();
            var isNew = achievementTypeCache == null || achievementTypeCache.Id == 0;

            List<AchievementTypeCache> eligiblePrerequisites;

            if ( isNew )
            {
                eligiblePrerequisites = config == null
                    ? new List<AchievementTypeCache>()
                    : AchievementTypeService.GetEligiblePrerequisiteAchievementTypeCachesForNewAchievement( config.AchieverEntityTypeCache.Id );
            }
            else
            {
                eligiblePrerequisites = achievementTypeCache == null
                    ? new List<AchievementTypeCache>()
                    : AchievementTypeService.GetEligiblePrerequisiteAchievementTypeCaches( achievementTypeCache );
            }

            bag.AvailablePrerequisites = eligiblePrerequisites.ConvertAll( p => new ListItemBag() { Text = p.Name, Value = p.Guid.ToString() } );

            if ( !isNew && achievementTypeCache != null)
            {
                bag.Prerequisites = achievementTypeCache.Prerequisites.ConvertAll( p => p.PrerequisiteAchievementType.Guid.ToString() );
            }
        }

        /// <summary>
        /// Gets the achievement component.
        /// </summary>
        /// <returns></returns>
        private AchievementComponent GetAchievementComponent( Guid? componentEntityTypeGuid = null )
        {
            int? componentEntityTypeId;

            if ( componentEntityTypeGuid == null )
            {
                var achievementType = GetAchievementTypeCache();
                componentEntityTypeId = achievementType != null ? achievementType.ComponentEntityTypeId : ( int? ) null;
            }
            else
            {
                var entityType = EntityTypeCache.Get( componentEntityTypeGuid.Value );
                componentEntityTypeId = entityType?.Id;
            }

            var componentEntityType = componentEntityTypeId.HasValue ? EntityTypeCache.Get( componentEntityTypeId.Value ) : null;
            return componentEntityType == null ? null : AchievementContainer.GetComponent( componentEntityType.Name );
        }

        /// <summary>
        /// Gets the achievement configuration.
        /// </summary>
        /// <returns></returns>
        private AchievementConfiguration GetAchievementConfiguration( Guid? componentEntityTypeGuid = null )
        {
            var component = GetAchievementComponent( componentEntityTypeGuid );

            if ( component == null )
            {
                return null;
            }

            return component.SupportedConfiguration;
        }

        /// <summary>
        /// Gets the achievement type cache.
        /// </summary>
        /// <returns></returns>
        private AchievementTypeCache GetAchievementTypeCache()
        {
            var key = PageParameter( PageParameterKey.AchievementTypeId );
            var achievementTypeId = Rock.Utility.IdHasher.Instance.GetId( key ) ?? key.AsInteger();
            return AchievementTypeCache.Get( achievementTypeId );
        }

        /// <summary>
        /// Filter for attributes.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>
        ///   <c>true</c> if [is attribute included] [the specified attribute]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsAttributeIncluded( AttributeCache attribute )
        {
            return attribute.Key != "Order" && attribute.Key != "Active";
        }

        /// <summary>
        /// Marks the old image as temporary.
        /// </summary>
        /// <param name="oldbinaryFileId">The binary file identifier.</param>
        /// <param name="binaryFileService">The binary file service.</param>
        private void MarkOldImageAsTemporary( int? oldbinaryFileId, int? newBinaryFileId, BinaryFileService binaryFileService )
        {
            if ( oldbinaryFileId != newBinaryFileId )
            {
                var oldImageTemplatePreview = binaryFileService.Get( oldbinaryFileId ?? 0 );
                if ( oldImageTemplatePreview != null )
                {
                    // the old image won't be needed anymore, so make it IsTemporary and have it get cleaned up later
                    oldImageTemplatePreview.IsTemporary = true;
                }
            }
        }

        /// <summary>
        /// Ensures the current image is not marked as temporary.
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <param name="binaryFileService">The binary file service.</param>
        private static void EnsureCurrentImageIsNotMarkedAsTemporary( int? binaryFileId, BinaryFileService binaryFileService )
        {
            if ( binaryFileId.HasValue )
            {
                var imageTemplatePreview = binaryFileService.Get( binaryFileId.Value );
                if ( imageTemplatePreview != null && imageTemplatePreview.IsTemporary )
                {
                    imageTemplatePreview.IsTemporary = false;
                }
            }
        }

        /// <summary>
        /// Gets the attempts query for the chart. Only successes.
        /// </summary>
        /// <returns></returns>
        private IQueryable<AchievementAttempt> GetChartQuery()
        {
            var achievementType = GetAchievementTypeCache();
            var attemptService = new AchievementAttemptService( RockContext );
            var query = attemptService.Queryable().AsNoTracking().Where( saa =>
                saa.AchievementTypeId == achievementType.Id &&
                saa.IsSuccessful &&
                saa.AchievementAttemptEndDateTime.HasValue );
            return query;
        }

        /// <summary>
        /// Gets a configured factory that creates the data required for the chart.
        /// </summary>
        /// <param name="delimitedValues">The delimited date range</param>
        /// <returns></returns>
        public ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint> GetChartJsFactory( string delimitedValues )
        {
            var reportPeriod = new TimePeriod( delimitedValues );
            var isYearly = reportPeriod.TimeUnit == TimePeriodUnitSpecifier.Year;
            var dateRange = reportPeriod.GetDateRange();
            var startDate = dateRange.Start;
            var endDate = dateRange.End;

            var successDateQuery = GetChartQuery().Select( saa => saa.AchievementAttemptEndDateTime.Value );

            if ( startDate.HasValue )
            {
                startDate = startDate.Value.Date;
                successDateQuery = successDateQuery.Where( d => d >= startDate );
            }

            if ( endDate.HasValue )
            {
                endDate = endDate.Value.Date.AddDays( 1 );
                successDateQuery = successDateQuery.Where( d => d < endDate );
            }

            Func<DateTime, DateTime> groupByExpression;

            if ( isYearly )
            {
                groupByExpression = dt => new DateTime( dt.Year, dt.Month, 1 );
            }
            else
            {
                groupByExpression = dt => dt.Date;
            }

            var groupedSuccessData = successDateQuery.ToList().GroupBy( groupByExpression );

            // Initialize a new Chart Factory.
            var factory = new ChartJsTimeSeriesDataFactory<ChartJsTimeSeriesDataPoint>
            {
                TimeScale = isYearly ? ChartJsTimeSeriesTimeScaleSpecifier.Month : ChartJsTimeSeriesTimeScaleSpecifier.Day,
                StartDateTime = startDate,
                EndDateTime = endDate,
                ChartStyle = ChartJsTimeSeriesChartStyleSpecifier.Line
            };

            // Add data series for success
            factory.Datasets.Add( new ChartJsTimeSeriesDataset
            {
                Name = "Successful",
                BorderColor = ChartJsConstants.Colors.Green,
                DataPoints = groupedSuccessData.Select( g => ( IChartJsTimeSeriesDataPoint ) new ChartJsTimeSeriesDataPoint
                {
                    DateTime = g.Key,
                    Value = g.Count()
                } ).ToList()
            } );

            return factory;
        }

        /// <summary>
        /// Save the attached image file.
        /// </summary>
        /// <param name="box">The valid properties box</param>
        /// <param name="entity">The achievement type entity</param>
        private void SaveImage( ValidPropertiesBox<AchievementTypeBag> box, AchievementType entity )
        {
            var binaryFileService = new BinaryFileService( RockContext );

            if ( box.Bag.ImageBinaryFile != null )
            {
                var binaryFileId = box.Bag.ImageBinaryFile.GetEntityId<BinaryFile>( RockContext );
                MarkOldImageAsTemporary( entity.ImageBinaryFileId, binaryFileId, binaryFileService );
                entity.ImageBinaryFileId = binaryFileId;
                // Ensure that the Image is not set as IsTemporary=True
                EnsureCurrentImageIsNotMarkedAsTemporary( entity.ImageBinaryFileId, binaryFileService );
            }
        }


        /// <summary>
        /// Saves the alternate image.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="entity">The entity.</param>
        private void SaveAlternateImage( ValidPropertiesBox<AchievementTypeBag> box, AchievementType entity )
        {
            var binaryFileService = new BinaryFileService( RockContext );

            if ( box.Bag.AlternateImageBinaryFile != null )
            {
                var binaryFileId = box.Bag.AlternateImageBinaryFile.GetEntityId<BinaryFile>( RockContext );
                MarkOldImageAsTemporary( entity.AlternateImageBinaryFileId, binaryFileId, binaryFileService );
                entity.AlternateImageBinaryFileId = binaryFileId;
                // Ensure that the Image is not set as IsTemporary=True
                EnsureCurrentImageIsNotMarkedAsTemporary( entity.AlternateImageBinaryFileId, binaryFileService );
            }
        }

        /// <summary>
        /// Saves the prerequisites.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="entity">The entity.</param>
        private void SavePrerequisites( ValidPropertiesBox<AchievementTypeBag> box, AchievementType entity )
        {
            // Upsert Prerequisites
            var prerequisiteService = new AchievementTypePrerequisiteService( RockContext );

            // Remove existing prerequisites that are not selected
            var removePrerequisiteAchievementTypes = entity.Prerequisites
                .Where( statp => !box.Bag.Prerequisites.Contains( statp.PrerequisiteAchievementType.Guid.ToString() ) ).ToList();

            foreach ( var prerequisite in removePrerequisiteAchievementTypes )
            {
                entity.Prerequisites.Remove( prerequisite );
                prerequisiteService.Delete( prerequisite );
            }

            // Add selected achievement types prerequisites that are not existing
            var addPrerequisiteAchievementTypeIds = entity.Prerequisites
                .Where( statp => !box.Bag.Prerequisites.Any( statGuid => statGuid == statp.PrerequisiteAchievementType.Guid.ToString() ) )
                .Select( statp => statp.PrerequisiteAchievementType.Guid )
                .ToList();

            var prerequisiteTypes = new AchievementTypeService( RockContext ).GetByGuids( addPrerequisiteAchievementTypeIds );

            foreach ( var prerequisiteAchievementType in prerequisiteTypes )
            {
                entity.Prerequisites.Add( new AchievementTypePrerequisite
                {
                    AchievementTypeId = entity.Id,
                    PrerequisiteAchievementTypeId = prerequisiteAchievementType.Id
                } );
            }
        }

        /// <summary>
        /// Saves the step details.
        /// </summary>
        /// <param name="box">The box.</param>
        /// <param name="entity">The entity.</param>
        private void SaveStepDetails( ValidPropertiesBox<AchievementTypeBag> box, AchievementType entity )
        {
            // Both step type and status are required together or neither can be set
            var stepTypeId = box.Bag.AchievementStepType.GetEntityId<StepType>( RockContext );
            var stepStatusId = box.Bag.AchievementStepStatus.GetEntityId<StepStatus>( RockContext );

            if ( box.Bag.AddStepOnSuccess && stepTypeId.HasValue && stepStatusId.HasValue )
            {
                entity.AchievementStepTypeId = stepTypeId;
                entity.AchievementStepStatusId = stepStatusId;
            }
            else
            {
                entity.AchievementStepTypeId = null;
                entity.AchievementStepStatusId = null;
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the entity attributes and values.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <param name="idKey">The identifier key.</param>
        [BlockAction]
        public BlockActionResult GetEntityAttributes( Guid? entityTypeGuid, string idKey )
        {
            if ( !entityTypeGuid.HasValue )
            {
                return ActionNotFound();
            }

            var entityType = EntityTypeCache.Get( entityTypeGuid.Value );

            if ( entityType == null )
            {
                return ActionNotFound();
            }

            var achievementType = new AchievementType() { Id = 0, ComponentEntityTypeId = entityType.Id, Guid = Guid.Empty };

            if ( !string.IsNullOrWhiteSpace( idKey ) )
            {
                var achievementTypeService = new AchievementTypeService( RockContext );
                var existingEntity = achievementTypeService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( existingEntity.ComponentEntityTypeId == entityType.Id )
                {
                    achievementType = existingEntity;
                }
            }

            achievementType.LoadAttributes();

            var bag = GetCommonEntityBag( achievementType );
            bag.LoadAttributesAndValuesForPublicEdit( achievementType, RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: IsAttributeIncluded );
            SetPrerequisiteListValues( bag, entityTypeGuid );

            var component = AchievementContainer.GetComponent( entityType.Name );
            if ( component != null )
            {
                bag.AchievementEventDescription = component.Description;
            }

            return ActionOk( bag );
        }

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

            return ActionOk( new ValidPropertiesBox<AchievementTypeBag>
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
        public BlockActionResult Save( ValidPropertiesBox<AchievementTypeBag> box )
        {
            var entityService = new AchievementTypeService( RockContext );
            AchievementType entity;

            // Determine if we are editing an existing entity or creating a new one.
            if ( box.Bag.IdKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( box.Bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new AchievementType();
                entityService.Add( entity );
            }

            var isNew = entity.Id == 0;

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateAchievementType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( !entity.IsAuthorized( Rock.Security.Authorization.VIEW, GetCurrentPerson() ) )
            {
                entity.AllowPerson( Rock.Security.Authorization.VIEW, GetCurrentPerson(), RockContext );
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, GetCurrentPerson() ) )
            {
                entity.AllowPerson( Rock.Security.Authorization.EDIT, GetCurrentPerson(), RockContext );
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, GetCurrentPerson() ) )
            {
                entity.AllowPerson( Rock.Security.Authorization.ADMINISTRATE, GetCurrentPerson(), RockContext );
            }

            // Now that the component attributes are saved, generate the config JSON from the component
            var updatedCacheItem = AchievementTypeCache.Get( entity.Id );
            var component = updatedCacheItem.AchievementComponent;
            var configDictionary = component.GenerateConfigFromAttributeValues( updatedCacheItem );
            entity.ComponentConfigJson = configDictionary.ToJson();

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.AchievementTypeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<AchievementTypeBag>
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
            var entityService = new AchievementTypeService( RockContext );

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
        /// Performs the rebuild action on the Achievement Type
        /// </summary>
        /// <param name="idKey"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Rebuild( string idKey )
        {
            if ( idKey.IsNullOrWhiteSpace() )
            {
                return ActionNotFound();
            }

            using ( RockContext rockContext = new RockContext() )
            {
                var achievementTypeService = new AchievementTypeService( rockContext );
                var achievementType = achievementTypeService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( !achievementType.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, GetCurrentPerson() ) )
                {
                    return ActionUnauthorized( "You are not authorized to rebuild this item." );
                }

                AchievementTypeService.Process( achievementType.Id );

                return ActionOk( "The achievement type rebuild was successful!" );
            }
        }

        /// <summary>
        /// Refresh the chart using the current filter settings.
        /// </summary>
        /// <param name="dateRange"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult RefreshChart( string dateRange )
        {
            var query = GetChartQuery();
            var hasData = query?.Any() == true;

            if ( !hasData )
            {
                return ActionNotFound();
            }

            // Get chart data and set visibility of related elements.
            var chartFactory = GetChartJsFactory( dateRange );

            if ( !chartFactory.HasData )
            {
                return ActionNotFound( "There are no Attempts matching the current filter." );
            }

            var args = new ChartJsTimeSeriesDataFactory.GetJsonArgs
            {
                DisplayLegend = false,
                LineTension = 0m,
                MaintainAspectRatio = false,
                SizeToFitContainerWidth = true
            };
            var chartData = chartFactory.GetChartDataJson( args );

            return ActionOk( new { chartData } );
        }

        #endregion
    }
}
