﻿// <copyright>
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
using Rock.Constants;
using Rock.Data;
using Rock.Lms;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Lms.LearningParticipantDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Lms
{
    /// <summary>
    /// Displays the details of a particular learning participant.
    /// </summary>
    [DisplayName( "Learning Participant Detail" )]
    [Category( "LMS" )]
    [Description( "Displays the details of a particular learning participant." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "7f3752ef-7a4a-4f96-bd5c-e6609f0bfac6" )]
    [Rock.SystemGuid.BlockTypeGuid( "f1179439-31a1-4897-ab2e-b991d60455aa" )]
    public class LearningParticipantDetail : RockEntityDetailBlockType<LearningParticipant, LearningParticipantBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LearningProgramId = "LearningProgramId";
            public const string LearningCourseId = "LearningCourseId";
            public const string LearningParticipantId = "LearningParticipantId";
            public const string LearningClassId = "LearningClassId";
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
            var box = new DetailBlockBox<LearningParticipantBag, LearningParticipantDetailOptionsBag>();

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
        private LearningParticipantDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new LearningParticipantDetailOptionsBag();

            var learningClass = new LearningClassService( RockContext )
                .GetInclude(
                    PageParameter( PageParameterKey.LearningClassId ),
                    c => c.LearningCourse.LearningProgram,
                    !PageCache.Layout.Site.DisablePredictableIds );

            options.ClassRoles = new LearningClassService( RockContext ).GetClassRoles( learningClass.Id )?.ToListItemBagList();
            options.CanViewGrades = learningClass.IsAuthorized( Authorization.VIEW_GRADES, GetCurrentPerson() );
            options.ConfigurationMode = learningClass.LearningCourse.LearningProgram.ConfigurationMode;

            return options;
        }

        /// <summary>
        /// Validates the LearningParticipant for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="learningParticipant">The LearningParticipant to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LearningParticipant is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLearningParticipant( LearningParticipant learningParticipant, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LearningParticipantBag, LearningParticipantDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LearningParticipant.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( LearningParticipant.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    var bag = GetEntityBagForEdit( entity );
                    box.Entity = bag;
                    box.ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList();
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( LearningParticipant.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="LearningParticipantBag"/> that represents the entity.</returns>
        private LearningParticipantBag GetCommonEntityBag( LearningParticipant entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var canViewGrades = entity.IsAuthorized( Authorization.VIEW_GRADES, RequestContext.CurrentPerson );
            var absences = GetAbsences( entity );
            return new LearningParticipantBag
            {
                IdKey = entity.IdKey,
                Absences = absences,
                AbsencesLabelStyle = entity.LearningClass?.AbsencesLabelStyle( absences ?? 0 ),
                CurrentGradePercent = canViewGrades ? Math.Round( entity.LearningGradePercent, 1 ) : 0,
                CurrentGradeText = canViewGrades ? entity.LearningGradingSystemScale?.Name : null,
                Note = entity.Note,
                ParticipantRole = entity.GroupRole?.ToListItemBag(),
                PersonAlias = entity.Person?.PrimaryAlias?.ToListItemBag(),
                IsFacilitator = entity.GroupRole?.IsLeader ?? false
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="LearningParticipantBag"/> that represents the entity.</returns>
        protected override LearningParticipantBag GetEntityBagForView( LearningParticipant entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="LearningParticipantBag"/> that represents the entity.</returns>
        protected override LearningParticipantBag GetEntityBagForEdit( LearningParticipant entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        protected override bool UpdateEntityFromBox( LearningParticipant entity, ValidPropertiesBox<LearningParticipantBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            if ( Guid.TryParse( box.Bag.PersonAlias.Value, out var primaryAliasGuid ) )
            {
                var person = new PersonAliasService( RockContext ).Get( primaryAliasGuid );
                entity.PersonId = person.PersonId;
            }

            var classId = RequestContext.PageParameterAsId( PageParameterKey.LearningClassId );

            if ( classId > 0 && Guid.TryParse( box.Bag.ParticipantRole.Value, out var roleGuid ) )
            {
                entity.GroupRoleId = new LearningClassService( RockContext ).GetClassRoles( classId ).First( r => r.Guid == roleGuid ).Id;
            }

            box.IfValidProperty( nameof( box.Bag.CurrentGradePercent ),
                () => entity.LearningGradePercent = box.Bag.CurrentGradePercent );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <returns>The <see cref="LearningParticipant"/> to be viewed or edited on the page.</returns>
        protected override LearningParticipant GetInitialEntity()
        {
            var partipicantId = RequestContext.PageParameterAsId( PageParameterKey.LearningParticipantId );

            if ( partipicantId == 0 )
            {

                /*
                    12/12/2024 - JC

                    We must load the parent LearningClass for new records.
                    When the authorization is checked the LearningClass (the ParentAuthority)
                    will be responsible for approving/denying access (see LearningParticipant.IsAuthorized).

                    Reason: ParentAuthority (LearningClass) will be checked for authorization.
                */
                var learningClass = new LearningClassService( RockContext ).Get(
                    PageParameter( PageParameterKey.LearningClassId ),
                    !this.PageCache.Layout.Site.DisablePredictableIds );

                return new LearningParticipant
                {
                    LearningClass = learningClass,
                    LearningClassId = learningClass.Id
                };
            }

            return new LearningParticipantService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Include( p => p.Person )
                .Include( p => p.Group.GroupType )
                .Include( p => p.GroupRole )
                .Include( p => p.LearningGradingSystemScale )
                .Include( p => p.LearningClass )
                .Include( p => p.LearningClass.LearningSemester )
                .FirstOrDefault( p => p.Id == partipicantId );
        }

        private int? GetAbsences( LearningParticipant entity )
        {

            if ( entity.LearningClass?.GroupType?.TakesAttendance != true )
            {
                return null;
            }

            return new AttendanceService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Count( a =>
                    a.DidAttend.HasValue &&
                    a.DidAttend.Value &&
                    a.Occurrence.GroupId == entity.LearningClassId &&
                    a.PersonAlias != null &&
                    a.PersonAlias.PersonId == entity.PersonId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var queryParams = new Dictionary<string, string>
            {
                [PageParameterKey.LearningProgramId] = PageParameter( PageParameterKey.LearningProgramId ),
                [PageParameterKey.LearningCourseId] = PageParameter( PageParameterKey.LearningCourseId ),
                [PageParameterKey.LearningClassId] = PageParameter( PageParameterKey.LearningClassId )
            };

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( queryParams )
            };
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        protected override bool TryGetEntityForEditAction( string idKey, out LearningParticipant entity, out BlockActionResult error )
        {
            var entityService = new LearningParticipantService( RockContext );
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
                entity = new LearningParticipant();

                var classIdKey = PageParameter( PageParameterKey.LearningClassId );
                if ( classIdKey.IsNotNullOrWhiteSpace() )
                {
                    var classId = IdHasher.Instance.GetId( classIdKey );
                    entity.LearningClass = new LearningClassService( RockContext ).Queryable().Include( c => c.GroupType ).FirstOrDefault( c => c.Id == classId );
                }

                entity.GroupId = IdHasher.Instance.GetId( classIdKey ) ?? 0;

                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LearningParticipant.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit {LearningParticipant.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var entityKey = pageReference.GetPageParameter( PageParameterKey.LearningParticipantId ) ?? "";

            var entityDetail =
                    entityKey.Length == 0 ?
                    null :
                    new Service<LearningParticipant>( RockContext )
                        .GetSelect( entityKey, p => new { p.Person.NickName, p.Person.LastName, p.Person.SuffixValueId } );

            var breadCrumbPageRef = new PageReference( pageReference.PageId, pageReference.RouteId, pageReference.Parameters );
            var entityName = entityDetail == null ? null : Rock.Model.Person.FormatFullName( entityDetail.NickName, entityDetail.LastName, entityDetail.SuffixValueId );
            var breadCrumb = new BreadCrumbLink( entityName ?? "New Participant", breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb>
                    {
                        breadCrumb
                    }
            };
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
            var box = new DetailBlockBox<LearningParticipantBag, LearningParticipantDetailOptionsBag>
            {
                Entity = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<LearningParticipantBag> box )
        {
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
            if ( !ValidateLearningParticipant( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.SaveChanges();

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.LearningParticipantId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = new LearningParticipantService( RockContext ).Get( entity.Id );

            return ActionOk( GetEntityBagForView( entity ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new LearningParticipantService( RockContext );

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
        /// Gets a list of activities for the current course.
        /// </summary>
        /// <returns>A list of Courses</returns>
        [BlockAction]
        public BlockActionResult GetLearningPlan()
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                return ActionBadRequest( $"The {LearningParticipant.FriendlyTypeName} was not found." );
            }

            var now = RockDateTime.Now;
            var participantService = new LearningParticipantService( RockContext );

            // Get the grade scales first since we'll need them for the grade calculations.
            var gradeScales = participantService.Queryable()
                .Where( p => p.Id == entity.Id )
                .Include( c => c.LearningClass.LearningGradingSystem.LearningGradingSystemScales )
                .SelectMany( c => c.LearningClass.LearningGradingSystem.LearningGradingSystemScales )
                .ToList()
                .OrderByDescending( g => g.ThresholdPercentage );

            var learningClassService = new LearningClassService( RockContext );
            var learningClassId = learningClassService.GetSelect(
                PageParameter( PageParameterKey.LearningClassId ),
                c => c.Id,
                !PageCache.Layout.Site.DisablePredictableIds );
            var personId = participantService.GetSelect( PageParameter( PageParameterKey.LearningParticipantId ), p => p.PersonId );
            var learningPlan = participantService.GetStudentLearningPlan( learningClassId, personId );

            var components = LearningActivityContainer.Instance.Components;

            var canViewGrades = entity.IsAuthorized( Authorization.VIEW_GRADES, GetCurrentPerson() );

            // Return all activities for the course.
            var gridBuilder = new GridBuilder<LearningClassActivityCompletion>()
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.LearningClassActivity.Name )
                .AddField( "type", a => a.LearningClassActivity.LearningActivity.ActivityComponentId )
                .AddField( "componentIconCssClass", a => components.FirstOrDefault( c => c.Value.Value.EntityType.Id == a.LearningClassActivity.LearningActivity.ActivityComponentId ).Value.Value.IconCssClass )
                .AddField( "componentHighlightColor", a => components.FirstOrDefault( c => c.Value.Value.EntityType.Id == a.LearningClassActivity.LearningActivity.ActivityComponentId ).Value.Value.HighlightColor )
                .AddField( "componentName", a => components.FirstOrDefault( c => c.Value.Value.EntityType.Id == a.LearningClassActivity.LearningActivity.ActivityComponentId ).Value.Value.Name )
                .AddField( "dateCompleted", a => a.CompletedDateTime )
                .AddField( "dateAvailable", a => a.AvailableDateTime )
                .AddField( "dueDate", a => a.DueDate )
                .AddField( "isPastDue", a => a.DueDate != null && a.DueDate >= now && !a.CompletedDateTime.HasValue )
                .AddField( "isAvailableNow", a => a.AvailableDateTime != null && now >= a.AvailableDateTime )
                .AddTextField( "grade", a => !canViewGrades || a.RequiresGrading || a.LearningClassActivity.Points == 0 ? null : a.GetGradeText( gradeScales ) );

            return ActionOk( gridBuilder.Build( learningPlan ) );
        }

        #endregion
    }
}
