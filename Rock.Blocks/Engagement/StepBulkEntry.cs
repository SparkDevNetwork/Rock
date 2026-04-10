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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Engagement.StepBulkEntry;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays a form to add steps for multiple people at once.
    /// People are selected upfront, then step configuration is applied to all.
    /// </summary>

    [DisplayName( "Step Bulk Entry" )]
    [Category( "Steps" )]
    [Description( "Displays a form to add steps for multiple people at once." )]
    [IconCssClass( "ti ti-truck" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [StepProgramStepTypeField(
        name: "Step Program and Type",
        Description = "The step program and step type to use to add a new step. Leave this empty to allow the user to choose.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.StepProgramStepType )]

    [StepProgramStepStatusField(
        name: "Step Program and Status",
        Description = "The step program and step status to use to add a new step. Leave this empty to allow the user to choose.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKey.StepProgramStepStatus )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "24C06D8F-6384-49F5-82D9-B2F8A7C37343" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "9A0DEDC8-D93A-40BD-A555-F426282CE07D" )]
    [Rock.SystemGuid.BlockTypeGuid( "6535FA22-9630-49A3-B8FF-A672CD91B8EE" )]
    public class StepBulkEntry : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys for block attributes.
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The step program and step type block setting.
            /// </summary>
            public const string StepProgramStepType = "StepProgramStepType";

            /// <summary>
            /// The step program and step status block setting.
            /// </summary>
            public const string StepProgramStepStatus = "StepProgramStepStatus";
        }

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The step program identifier page parameter.
            /// </summary>
            public const string StepProgramId = "StepProgramId";

            /// <summary>
            /// The step type identifier page parameter.
            /// </summary>
            public const string StepTypeId = "StepTypeId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new StepBulkEntryInitializationBox();

            // Parse block settings.
            var stepProgramStepTypeSettingValue = GetAttributeValue( AttributeKey.StepProgramStepType );
            StepProgramStepTypeFieldType.ParseDelimitedGuids( stepProgramStepTypeSettingValue, out var settingProgramGuid, out var settingTypeGuid );

            var stepStatusSettingValue = GetAttributeValue( AttributeKey.StepProgramStepStatus );
            StepProgramStepStatusFieldType.ParseDelimitedGuids( stepStatusSettingValue, out var statusSettingProgramGuid, out var settingStatusGuid );

            // Resolve step program guid from settings or page parameters.
            Guid? resolvedProgramGuid = null;
            Guid? resolvedTypeGuid = null;
            Guid? resolvedStatusGuid = settingStatusGuid;

            // Priority 1: Block setting step type (implies program).
            if ( settingTypeGuid.HasValue )
            {
                resolvedTypeGuid = settingTypeGuid;
                resolvedProgramGuid = settingProgramGuid ?? ResolveProgramGuidFromStepType( settingTypeGuid.Value );
            }
            // Priority 2: Block setting program (from the type setting).
            else if ( settingProgramGuid.HasValue )
            {
                resolvedProgramGuid = settingProgramGuid;
            }

            // Priority 3: Page parameter step type.
            if ( !resolvedTypeGuid.HasValue )
            {
                var stepTypeKey = RequestContext.GetPageParameter( PageParameterKey.StepTypeId );

                if ( stepTypeKey.IsNotNullOrWhiteSpace() )
                {
                    var stepType = new StepTypeService( RockContext ).Get( stepTypeKey, !PageCache.Layout.Site.DisablePredictableIds );

                    if ( stepType != null )
                    {
                        resolvedTypeGuid = stepType.Guid;

                        if ( !resolvedProgramGuid.HasValue )
                        {
                            resolvedProgramGuid = stepType.StepProgram?.Guid
                                ?? new StepProgramService( RockContext ).GetGuid( stepType.StepProgramId );
                        }
                    }
                }
            }

            // Priority 4: Page parameter step program (only if program not yet resolved).
            if ( !resolvedProgramGuid.HasValue )
            {
                var stepProgramKey = RequestContext.GetPageParameter( PageParameterKey.StepProgramId );

                if ( stepProgramKey.IsNotNullOrWhiteSpace() )
                {
                    var stepProgram = new StepProgramService( RockContext ).Get( stepProgramKey, !PageCache.Layout.Site.DisablePredictableIds );

                    if ( stepProgram != null )
                    {
                        resolvedProgramGuid = stepProgram.Guid;
                    }
                }
            }

            // Also try resolving program from the status setting if not yet resolved.
            if ( !resolvedProgramGuid.HasValue && statusSettingProgramGuid.HasValue )
            {
                resolvedProgramGuid = statusSettingProgramGuid;
            }

            /*
                4/6/26 - MSE

                Clear the pre-selected status if it belongs to a different program
                than the resolved program. This prevents confusing behavior where
                the status picker (filtered by program) won't show the pre-selected value.

                Reason: The two block settings can point to different programs.
            */
            if ( resolvedStatusGuid.HasValue && resolvedProgramGuid.HasValue && statusSettingProgramGuid.HasValue
                && statusSettingProgramGuid.Value != resolvedProgramGuid.Value )
            {
                resolvedStatusGuid = null;
            }

            // Determine picker visibility.
            var isProgramPickerVisible = !resolvedProgramGuid.HasValue;
            var isTypePickerDisabled = resolvedTypeGuid.HasValue;

            // Build step type configuration if a type is pre-selected.
            StepBulkEntryStepTypeConfigurationBag stepTypeConfig = null;

            if ( resolvedTypeGuid.HasValue )
            {
                var stepTypeCache = StepTypeCache.Get( resolvedTypeGuid.Value );

                if ( stepTypeCache == null || !stepTypeCache.IsActive || stepTypeCache.StepProgram == null )
                {
                    box.ErrorMessage = "The specified step type could not be found.";
                    return box;
                }

                stepTypeConfig = BuildStepTypeConfiguration( stepTypeCache );
            }

            box.Options = new StepBulkEntryOptionsBag
            {
                StepProgram = resolvedProgramGuid.HasValue ? StepProgramCache.Get( resolvedProgramGuid.Value )?.ToListItemBag() : null,
                StepType = resolvedTypeGuid.HasValue ? StepTypeCache.Get( resolvedTypeGuid.Value )?.ToListItemBag() : null,
                StepStatus = resolvedStatusGuid.HasValue ? new StepStatusService( RockContext ).Get( resolvedStatusGuid.Value )?.ToListItemBag() : null,
                IsProgramPickerVisible = isProgramPickerVisible,
                IsTypePickerDisabled = isTypePickerDisabled,
                StepTypeConfiguration = stepTypeConfig
            };

            return box;
        }

        /// <summary>
        /// Resolves the step program Guid from a step type Guid.
        /// </summary>
        /// <param name="stepTypeGuid">The step type Guid.</param>
        /// <returns>The program Guid, or null if not found.</returns>
        private Guid? ResolveProgramGuidFromStepType( Guid stepTypeGuid )
        {
            var stepTypeCache = StepTypeCache.Get( stepTypeGuid );

            return stepTypeCache?.StepProgram?.Guid;
        }

        /// <summary>
        /// Builds the step type configuration bag from the provided cache object.
        /// Callers must verify the cache is non-null and active before calling.
        /// </summary>
        /// <param name="stepTypeCache">The validated step type cache.</param>
        /// <returns>The configuration bag.</returns>
        private StepBulkEntryStepTypeConfigurationBag BuildStepTypeConfiguration( StepTypeCache stepTypeCache )
        {
            // Build a temporary step to load its attributes.
            var tempStep = new Step { StepTypeId = stepTypeCache.Id };
            tempStep.LoadAttributes( RockContext );

            return new StepBulkEntryStepTypeConfigurationBag
            {
                HasEndDate = stepTypeCache.HasEndDate,
                IsDateRequired = stepTypeCache.IsDateRequired,
                StepProgramGuid = stepTypeCache.StepProgram.Guid,
                StartDateLabel = stepTypeCache.HasEndDate ? "Start Date" : "Date",
                StepAttributes = tempStep.GetPublicAttributesForEdit( RequestContext.CurrentPerson, enforceSecurity: true, attributeFilter: a => a.ShowOnBulk )
            };
        }

        /// <summary>
        /// Gets the attribute keys for attributes that should be shown on bulk entry.
        /// Only includes attributes where ShowOnBulk is true.
        /// </summary>
        /// <param name="step">A step entity used to derive the step type qualifier.</param>
        /// <returns>A set of attribute keys.</returns>
        private HashSet<string> GetBulkAttributeKeys( Step step )
        {
            var stepAttributeCaches = AttributeCache.AllForEntityType<Step>();
            var stepTypeQualifier = step.StepTypeId.ToString();

            return stepAttributeCaches
                .Where( a =>
                    a.EntityTypeQualifierColumn == "StepTypeId" &&
                    a.EntityTypeQualifierValue == stepTypeQualifier &&
                    a.ShowOnBulk )
                .Select( a => a.Key )
                .ToHashSet( StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Determines whether the current person can manage steps for the specified secured item.
        /// Returns true if the person has EDIT or MANAGE_STEPS authorization.
        /// </summary>
        /// <param name="securedItem">The secured item to check authorization against.</param>
        /// <returns><c>true</c> if the person can manage steps; otherwise, <c>false</c>.</returns>
        private bool CanManageSteps( ISecured securedItem )
        {
            return securedItem.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson )
                || securedItem.IsAuthorized( Authorization.MANAGE_STEPS, RequestContext.CurrentPerson );
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Resolves a person alias Guid into a person item bag with photo URL.
        /// Called when a person is selected from the picker.
        /// </summary>
        /// <param name="personAliasGuid">The person alias Guid from the picker.</param>
        /// <returns>The person item bag or an error.</returns>
        [BlockAction]
        public BlockActionResult GetPersonItem( Guid personAliasGuid )
        {
            var personAlias = new PersonAliasService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( pa => pa.Person )
                .FirstOrDefault( pa => pa.Guid == personAliasGuid );

            if ( personAlias?.Person == null )
            {
                return ActionNotFound( "The person could not be found." );
            }

            return ActionOk( new StepBulkEntryPersonItemBag
            {
                Name = personAlias.Person.FullName,
                PersonAliasGuid = personAlias.Guid,
                PhotoUrl = personAlias.Person.PhotoUrl
            } );
        }

        /// <summary>
        /// Gets the step type configuration when the step type picker changes.
        /// </summary>
        /// <param name="stepTypeGuid">The selected step type Guid.</param>
        /// <returns>The step type configuration or an error.</returns>
        [BlockAction]
        public BlockActionResult GetStepTypeConfiguration( Guid stepTypeGuid )
        {
            var stepTypeCache = StepTypeCache.Get( stepTypeGuid );

            if ( stepTypeCache == null || !stepTypeCache.IsActive )
            {
                return ActionNotFound( "The step type could not be found." );
            }

            // Check authorization using the cache.
            if ( !CanManageSteps( stepTypeCache ) )
            {
                return ActionForbidden( "You do not have permission to add steps for this step type." );
            }

            if ( stepTypeCache.StepProgram == null )
            {
                return ActionNotFound( "The step type configuration could not be loaded." );
            }

            return ActionOk( BuildStepTypeConfiguration( stepTypeCache ) );
        }

        /// <summary>
        /// Validates whether each selected person can have a step created for the
        /// specified step type. Uses batch queries for performance.
        /// </summary>
        /// <param name="request">The validation request containing person alias Guids and step type.</param>
        /// <returns>Per-person validation results.</returns>
        [BlockAction]
        public BlockActionResult ValidatePeople( StepBulkEntryValidateRequestBag request )
        {
            if ( request?.PersonAliasGuids == null || !request.PersonAliasGuids.Any() )
            {
                return ActionBadRequest( "At least one person is required." );
            }

            // Resolve the step type with prerequisites.
            var stepType = new StepTypeService( RockContext ).Queryable()
                .AsNoTracking()
                .Include( st => st.StepTypePrerequisites.Select( p => p.PrerequisiteStepType ) )
                .FirstOrDefault( st => st.Guid == request.StepTypeGuid && st.IsActive );

            if ( stepType == null )
            {
                return ActionBadRequest( "The step type could not be found." );
            }

            // Check authorization.
            if ( !CanManageSteps( stepType ) )
            {
                return ActionForbidden( "You do not have permission to add steps for this step type." );
            }

            // Batch load all person aliases.
            var personAliasService = new PersonAliasService( RockContext );
            var aliases = personAliasService.Queryable()
                .Where( pa => request.PersonAliasGuids.Contains( pa.Guid ) )
                .Select( pa => new
                {
                    pa.Guid,
                    PersonId = pa.PersonId,
                    NickName = pa.Person.NickName,
                    LastName = pa.Person.LastName
                } )
                .ToList();

            var personIds = aliases.Select( a => a.PersonId ).Distinct().ToList();
            var stepService = new StepService( RockContext );

            // Batch check AllowMultiple rule.
            var existingStepPersonIds = new HashSet<int>();
            if ( !stepType.AllowMultiple )
            {
                existingStepPersonIds = stepService.Queryable()
                    .Where( s => s.StepTypeId == stepType.Id && personIds.Contains( s.PersonAlias.PersonId ) )
                    .Select( s => s.PersonAlias.PersonId )
                    .Distinct()
                    .ToHashSet();
            }

            // Batch check prerequisites.
            var prereqStepTypeIds = stepType.StepTypePrerequisites
                .Select( p => p.PrerequisiteStepTypeId )
                .ToList();

            var completedPrereqsByPerson = new Dictionary<int, HashSet<int>>();

            if ( prereqStepTypeIds.Any() )
            {
                var completedData = stepService.Queryable()
                    .Where( s =>
                        personIds.Contains( s.PersonAlias.PersonId ) &&
                        prereqStepTypeIds.Contains( s.StepTypeId ) &&
                        s.StepStatus != null &&
                        s.StepStatus.IsCompleteStatus )
                    .Select( s => new
                    {
                        PersonId = s.PersonAlias.PersonId,
                        s.StepTypeId
                    } )
                    .ToList();

                foreach ( var item in completedData )
                {
                    if ( !completedPrereqsByPerson.ContainsKey( item.PersonId ) )
                    {
                        completedPrereqsByPerson[item.PersonId] = new HashSet<int>();
                    }

                    completedPrereqsByPerson[item.PersonId].Add( item.StepTypeId );
                }
            }

            // Build per-person results.
            var results = new List<StepBulkEntryValidatePersonResultBag>();

            foreach ( var alias in aliases )
            {
                var errors = new List<string>();
                var fullName = $"{alias.NickName} {alias.LastName}";

                // Check AllowMultiple.
                if ( !stepType.AllowMultiple && existingStepPersonIds.Contains( alias.PersonId ) )
                {
                    errors.Add( $"{alias.NickName} is not able to complete {stepType.Name} again because of the 'Allow Multiple' setting." );
                }

                // Check prerequisites.
                if ( prereqStepTypeIds.Any() )
                {
                    completedPrereqsByPerson.TryGetValue( alias.PersonId, out var completedIds );
                    var unmetPrereqs = stepType.StepTypePrerequisites
                        .Where( p => completedIds == null || !completedIds.Contains( p.PrerequisiteStepTypeId ) )
                        .Select( p => p.PrerequisiteStepType?.Name ?? "Unknown" )
                        .ToList();

                    if ( unmetPrereqs.Any() )
                    {
                        var prereqList = string.Join( ", ", unmetPrereqs );
                        errors.Add( $"{alias.NickName} has not completed the following prerequisites: {prereqList}." );
                    }
                }

                results.Add( new StepBulkEntryValidatePersonResultBag
                {
                    PersonName = fullName,
                    PersonAliasGuid = alias.Guid,
                    IsValid = !errors.Any(),
                    Errors = errors
                } );
            }

            return ActionOk( results );
        }

        /// <summary>
        /// Creates steps for all specified people in a single transaction.
        /// </summary>
        /// <param name="request">The save request containing people and step data.</param>
        /// <returns>The save result with success count and any errors.</returns>
        [BlockAction]
        public BlockActionResult SaveSteps( StepBulkEntrySaveRequestBag request )
        {
            if ( request?.PersonAliasGuids == null || !request.PersonAliasGuids.Any() )
            {
                return ActionBadRequest( "At least one person is required." );
            }

            // Resolve step type.
            var stepType = new StepTypeService( RockContext ).Queryable()
                .AsNoTracking()
                .FirstOrDefault( st => st.Guid == request.StepTypeGuid && st.IsActive );

            if ( stepType == null )
            {
                return ActionBadRequest( "The step type could not be found." );
            }

            // Check authorization.
            if ( !CanManageSteps( stepType ) )
            {
                return ActionForbidden( "You do not have permission to add steps for this step type." );
            }

            // Resolve step status.
            var stepStatus = new StepStatusService( RockContext ).Queryable()
                .AsNoTracking()
                .FirstOrDefault( ss => ss.Guid == request.StepStatusGuid );

            if ( stepStatus == null )
            {
                return ActionBadRequest( "The step status could not be found." );
            }

            // Resolve campus.
            int? campusId = null;

            if ( request.CampusGuid.HasValue )
            {
                campusId = CampusCache.Get( request.CampusGuid.Value )?.Id;
            }

            // Parse dates.
            var startDate = request.StartDate.AsDateTime();
            var endDate = stepType.HasEndDate ? request.EndDate.AsDateTime() : null;

            // Validate date ordering.
            if ( startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value )
            {
                return ActionBadRequest( "The start date must be before the end date." );
            }

            // Compute CompletedDateTime.
            DateTime? completedDateTime = null;

            if ( stepStatus.IsCompleteStatus )
            {
                completedDateTime = endDate ?? startDate;
            }

            // Load all person aliases.
            var personAliasService = new PersonAliasService( RockContext );
            var aliases = personAliasService.Queryable()
                .Where( pa => request.PersonAliasGuids.Contains( pa.Guid ) )
                .Select( pa => new
                {
                    pa.Id,
                    pa.Guid,
                    PersonName = pa.Person.NickName + " " + pa.Person.LastName
                } )
                .ToList();

            var stepService = new StepService( RockContext );
            var stepsToSave = new List<Step>();
            var errors = new List<string>();

            // Create Step entities.
            foreach ( var alias in aliases )
            {
                var step = new Step
                {
                    StepTypeId = stepType.Id,
                    PersonAliasId = alias.Id,
                    StepStatusId = stepStatus.Id,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    CampusId = campusId,
                    CompletedDateTime = completedDateTime
                };

                try
                {
                    stepService.Add( step );
                    stepsToSave.Add( step );
                }
                catch ( Exception ex )
                {
                    errors.Add( $"{alias.PersonName}: {ex.Message}" );
                }
            }

            Dictionary<string, string> bulkAttributeValues = null;

            if ( request.AttributeValues != null && request.AttributeValues.Any() )
            {
                var bulkKeys = GetBulkAttributeKeys( new Step { StepTypeId = stepType.Id } );

                bulkAttributeValues = request.AttributeValues
                    .Where( kvp => bulkKeys.Contains( kvp.Key ) )
                    .ToDictionary( kvp => kvp.Key, kvp => kvp.Value );
            }

            // Save all steps and their attributes in a single transaction.
            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();

                // Save attributes for each step.
                foreach ( var step in stepsToSave )
                {
                    step.LoadAttributes( RockContext );

                    if ( bulkAttributeValues != null )
                    {
                        step.SetPublicAttributeValues( bulkAttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                    }

                    try
                    {
                        step.SaveAttributeValues( RockContext );
                    }
                    catch ( Exception ex )
                    {
                        var personName = aliases.FirstOrDefault( a => a.Id == step.PersonAliasId )?.PersonName ?? "Unknown";
                        errors.Add( $"Failed to save attributes for {personName}: {ex.Message}" );
                    }
                }
            } );

            // Build the navigation URL for the "Done" button.
            var navParams = new Dictionary<string, string>();

            navParams[PageParameterKey.StepTypeId] = stepType.IdKey;

            var navigationUrl = this.GetParentPageUrl( navParams );

            return ActionOk( new StepBulkEntrySaveResultBag
            {
                SuccessCount = stepsToSave.Count,
                Errors = errors,
                NavigationUrl = navigationUrl
            } );
        }

        #endregion Block Actions
    }
}
