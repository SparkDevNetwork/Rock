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
using System.Text;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.AchievementAttemptDetail;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular achievement attempt.
    /// </summary>

    [DisplayName( "Achievement Attempt Detail" )]
    [Category( "Engagement" )]
    [Description( "Displays the details of a particular achievement attempt." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes
    [LinkedPage(
        "Achievement Type Page",
        Description = "Page used for viewing the achievement type that this attempt is toward.",
        Key = AttributeKey.AchievementPage,
        IsRequired = false,
        Order = 2 )]
    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "a80564d5-701b-4f3a-8ba1-20baa2304da6" )]
    [Rock.SystemGuid.BlockTypeGuid( "fbe75c18-7f71-4d23-a546-7a17cf944ba6" )]
    public class AchievementAttemptDetail : RockDetailBlockType
    {
        #region Keys

        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The achievement page
            /// </summary>
            public const string AchievementPage = "AchievementPage";
        }

        private static class PageParameterKey
        {
            public const string AchievementTypeId = "AchievementTypeId";
            public const string AchievementAttemptId = "AchievementAttemptId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        private AchievementAttempt _attempt = null;
        private RockContext _rockContext = null;
        private AchievementTypeCache _achievementTypeCache = null;

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<AchievementAttempt>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private AchievementAttemptDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new AchievementAttemptDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the AchievementAttempt for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="achievementAttempt">The AchievementAttempt to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AchievementAttempt is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAchievementAttempt( AchievementAttempt achievementAttempt, RockContext rockContext, out string errorMessage )
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
        private void SetBoxInitialEntityState( DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AchievementAttempt.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AchievementAttempt.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AchievementAttempt.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="AchievementAttemptBag"/> that represents the entity.</returns>
        private AchievementAttemptBag GetCommonEntityBag( AchievementAttempt entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new AchievementAttemptBag
            {
                IdKey = entity.IdKey,
                AchievementAttemptEndDateTime = entity.AchievementAttemptEndDateTime,
                AchievementAttemptStartDateTime = entity.Id == 0 ? RockDateTime.Now : entity.AchievementAttemptStartDateTime,
                AchievementType = GetAchievementTypeCache().ToListItemBag(),
                IsClosed = entity.IsClosed,
                IsSuccessful = entity.IsSuccessful,
                Progress = entity.Progress
            };

            if ( entity.Id != 0 )
            {
                bag.AchieverEntityId = entity.Id;
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="AchievementAttemptBag"/> that represents the entity.</returns>
        private AchievementAttemptBag GetEntityBagForView( AchievementAttempt entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            var achievementPage = GetAttributeValue( AttributeKey.AchievementPage );

            var achiever = GetAchiever();
            bag.AchieverHtml = GetPersonHtml( achiever );
            bag.ProgressHtml = GetProgressHtml();
            bag.AttemptDescription = GetAttemptDateRangeString();
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            if ( entity.Id != 0 && achievementPage.IsNotNullOrWhiteSpace() )
            {
                bag.AchievementPageUrl = this.GetLinkedPageUrl( AttributeKey.AchievementPage, new Dictionary<string, string> {
                    { PageParameterKey.AchievementTypeId, entity.AchievementTypeId.ToString() }
                } );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="AchievementAttemptBag"/> that represents the entity.</returns>
        private AchievementAttemptBag GetEntityBagForEdit( AchievementAttempt entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( AchievementAttempt entity, DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.AchievementAttemptStartDateTime ),
                () => entity.AchievementAttemptStartDateTime = box.Entity.AchievementAttemptStartDateTime );

            box.IfValidProperty( nameof( box.Entity.AchievementType ),
                () => entity.AchievementTypeId = box.Entity.AchievementType.GetEntityId<AchievementType>( rockContext ).Value );

            box.IfValidProperty( nameof( box.Entity.AchieverEntityId ),
                () => entity.AchieverEntityId = box.Entity.AchieverEntityId.GetValueOrDefault() );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="AchievementAttempt"/> to be viewed or edited on the page.</returns>
        private AchievementAttempt GetInitialEntity( RockContext rockContext )
        {
            return _attempt ?? ( _attempt = GetInitialEntity<AchievementAttempt, AchievementAttemptService>( rockContext, PageParameterKey.AchievementAttemptId ) );
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
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( AchievementAttempt entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out AchievementAttempt entity, out BlockActionResult error )
        {
            var entityService = new AchievementAttemptService( rockContext );
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
                entity = new AchievementAttempt();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{AchievementAttempt.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${AchievementAttempt.FriendlyTypeName}." );
                return false;
            }

            _attempt = entity;

            return true;
        }

        /// <summary>
        /// Gets the person HTML.
        /// </summary>
        /// <returns></returns>
        private string GetPersonHtml( IEntity achiever )
        {
            var personImageStringBuilder = new StringBuilder();
            const string photoFormat = "<div class=\"photo-icon photo-round photo-round-sm pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url( '{2}' ); background-size: cover; background-repeat: no-repeat;\"></div>";
            const string nameLinkFormat = @"
    {0}
    <p><small><a href='/Person/{1}'>View Profile</a></small></p>
";

            if ( achiever is PersonAlias personAlias )
            {
                personImageStringBuilder.AppendFormat( photoFormat, personAlias.PersonId, personAlias.Person.PhotoUrl, RequestContext.ResolveRockUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
                personImageStringBuilder.AppendFormat( nameLinkFormat, personAlias.Person.FullName, personAlias.PersonId );
            }
            else
            {
                personImageStringBuilder.AppendFormat( photoFormat, null, null, RequestContext.ResolveRockUrl( "~/Assets/Images/person-no-photo-unknown.svg" ) );
                personImageStringBuilder.Append( "Unknown" );
            }

            return personImageStringBuilder.ToString();
        }

        private string GetAttemptDateRangeString()
        {
            var attempt = GetAttempt();

            if ( attempt == null )
            {
                return string.Empty;
            }

            var start = attempt.AchievementAttemptStartDateTime;
            var end = attempt.AchievementAttemptEndDateTime;

            if ( !end.HasValue )
            {
                return string.Format( "Started on {0}", start.ToShortDateString() );
            }

            return string.Format( "Ranging from {0} - {1}", start.ToShortDateString(), end.ToShortDateString() );
        }

        private string GetProgressHtml()
        {
            var attempt = GetAttempt();

            if ( attempt == null )
            {
                return string.Empty;
            }

            var progressLong = Convert.ToInt64( decimal.Round( attempt.Progress * 100 ) );
            var progressBarWidth = progressLong < 0 ? 0 : ( progressLong > 100 ? 100 : progressLong );
            var insideProgress = progressLong >= 50 ? progressLong : ( long? ) null;
            var outsideProgress = progressLong < 50 ? progressLong : ( long? ) null;
            var progressBarClass = progressLong >= 100 ? "progress-bar-success" : string.Empty;

            return string.Format(
@"<div class=""progress"">
    <div class=""progress-bar {5}"" role=""progressbar"" style=""width: {0}%;"">
        {1}{2}
    </div>
    <span style=""padding-left: 5px;"">{3}{4}</span>
</div>", progressBarWidth, insideProgress, insideProgress.HasValue ? "%" : string.Empty, outsideProgress, outsideProgress.HasValue ? "%" : string.Empty, progressBarClass );
        }

        /// <summary>
        /// Gets the attempt.
        /// </summary>
        /// <returns></returns>
        private AchievementAttempt GetAttempt()
        {
            if ( _attempt == null )
            {
                var attemptId = PageParameter( PageParameterKey.AchievementAttemptId ).AsIntegerOrNull();

                if ( attemptId > 0 )
                {
                    var service = new AchievementAttemptService( GetRockContext() );
                    _attempt = service.Queryable().FirstOrDefault( saa => saa.Id == attemptId.Value );
                }
            }

            return _attempt;
        }

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            return _rockContext ?? ( _rockContext = new RockContext() );
        }

        /// <summary>
        /// Gets the achiever.
        /// </summary>
        /// <returns></returns>
        private IEntity GetAchiever()
        {
            var attempt = GetAttempt();
            var achievementTypeCache = GetAchievementTypeCache();

            if ( attempt != null && achievementTypeCache != null )
            {
                var service = new EntityTypeService( GetRockContext() );
                return service.GetEntity( achievementTypeCache.AchieverEntityTypeId, attempt.AchieverEntityId );
            }

            return null;
        }

        /// <summary>
        /// Gets the type of the achievement.
        /// </summary>
        /// <returns></returns>
        private AchievementTypeCache GetAchievementTypeCache()
        {
            if ( _achievementTypeCache != null )
            {
                return _achievementTypeCache;
            }

            var attempt = GetAttempt();
            var achievementTypeId = PageParameter( PageParameterKey.AchievementTypeId ).AsIntegerOrNull();

            if ( attempt != null && attempt.AchievementTypeId != 0 )
            {
                achievementTypeId = attempt.AchievementTypeId;
            }

            if ( achievementTypeId > 0 )
            {
                _achievementTypeCache = AchievementTypeCache.Get( achievementTypeId.Value );
            }

            return _achievementTypeCache;
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new AchievementAttemptService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                var achievementType = AchievementTypeCache.Get( entity.AchievementTypeId );
                var progress = box.Entity.Progress;

                if ( progress < 0m )
                {
                    entity.Progress = 0m;
                }

                if ( progress > 1m && !achievementType.AllowOverAchievement )
                {
                    entity.Progress = 1m;
                }

                var isSuccess = progress >= 1m;

                if ( !box.Entity.AchievementAttemptEndDateTime.HasValue && isSuccess && !achievementType.AllowOverAchievement )
                {
                    entity.AchievementAttemptEndDateTime = RockDateTime.Today;
                }

                if ( box.Entity.AchievementAttemptEndDateTime.HasValue && box.Entity.AchievementAttemptEndDateTime < box.Entity.AchievementAttemptStartDateTime )
                {
                    box.Entity.AchievementAttemptEndDateTime = box.Entity.AchievementAttemptStartDateTime;
                }

                entity.IsClosed = ( box.Entity.AchievementAttemptEndDateTime.HasValue && box.Entity.AchievementAttemptEndDateTime.Value < RockDateTime.Today ) || ( isSuccess && !achievementType.AllowOverAchievement );
                entity.AchievementAttemptStartDateTime = box.Entity.AchievementAttemptStartDateTime;
                entity.AchievementAttemptEndDateTime = box.Entity.AchievementAttemptEndDateTime;
                entity.Progress = progress;
                entity.IsSuccessful = isSuccess;

                // Ensure everything is valid before saving.
                if ( !ValidateAchievementAttempt( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( !entity.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                {
                    entity.AllowPerson( Authorization.VIEW, GetCurrentPerson(), rockContext );
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
                {
                    entity.AllowPerson( Authorization.EDIT, GetCurrentPerson(), rockContext );
                }

                if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) )
                {
                    entity.AllowPerson( Authorization.ADMINISTRATE, GetCurrentPerson(), rockContext );
                }

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.AchievementAttemptId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity ) );
            }
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
                var entityService = new AchievementAttemptService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
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
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
