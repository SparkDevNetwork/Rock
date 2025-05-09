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
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular achievement attempt.
    /// </summary>

    [DisplayName( "Attempt Detail" )]
    [Category( "Achievements" )]
    [Description( "Displays the details of the given attempt for editing." )]
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
    public class AchievementAttemptDetail : RockEntityDetailBlockType<AchievementAttempt, AchievementAttemptBag>, IBreadCrumbBlock
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
        private AchievementTypeCache _achievementTypeCache = null;

        #region Methods

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var key = pageReference.GetPageParameter( PageParameterKey.AchievementAttemptId );
            var pageParameters = new Dictionary<string, string>();
            var name = "New Attempt";

            var date = new AchievementAttemptService( RockContext ).GetSelect( key, l => l.AchievementAttemptStartDateTime );

            if ( date != null )
            {
                pageParameters.Add( PageParameterKey.AchievementAttemptId, key );
                name = date.ToShortDateString();
            }

            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
            var breadCrumb = new BreadCrumbLink( name, breadCrumbPageRef );

            return new BreadCrumbResult
            {
                BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
            };
        }

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private AchievementAttemptDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new AchievementAttemptDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the AchievementAttempt for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="achievementAttempt">The AchievementAttempt to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the AchievementAttempt is valid, <c>false</c> otherwise.</returns>
        private bool ValidateAchievementAttempt( AchievementAttempt achievementAttempt, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<AchievementAttemptBag, AchievementAttemptDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {AchievementAttempt.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( AchievementAttempt.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( AchievementAttempt.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );

            box.NavigationUrls = GetBoxNavigationUrls( entity );
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
                AchievementAttemptEndDateTime = entity.AchievementAttemptEndDateTime.HasValue ? entity.AchievementAttemptEndDateTime.Value.ToRockDateTimeOffset() : ( DateTimeOffset? ) null,
                AchievementAttemptStartDateTime = entity.Id == 0 ? RockDateTime.Now : entity.AchievementAttemptStartDateTime.ToRockDateTimeOffset(),
                AchievementType = GetAchievementTypeCache().ToListItemBag(),
                IsClosed = entity.IsClosed,
                IsSuccessful = entity.IsSuccessful,
                Progress = entity.Progress.ToString(),
                AchieverEntityId = entity.AchieverEntityId == 0 ? null : entity.AchieverEntityId.ToString()
            };

            return bag;
        }

        /// <inheritdoc/>
        protected override AchievementAttemptBag GetEntityBagForView( AchievementAttempt entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetAttemptDetailsForBag( GetCommonEntityBag( entity ), entity );

            var achievementPage = GetAttributeValue( AttributeKey.AchievementPage );

            if ( entity.Id != 0 && achievementPage.IsNotNullOrWhiteSpace() )
            {
                bag.AchievementPageUrl = this.GetLinkedPageUrl( AttributeKey.AchievementPage, new Dictionary<string, string> {
                    { PageParameterKey.AchievementTypeId, entity.AchievementTypeId.ToString() }
                } );
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override AchievementAttemptBag GetEntityBagForEdit( AchievementAttempt entity )
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
        protected override bool UpdateEntityFromBox( AchievementAttempt entity, ValidPropertiesBox<AchievementAttemptBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AchievementAttemptStartDateTime ),
                () => entity.AchievementAttemptStartDateTime = box.Bag.AchievementAttemptStartDateTime.DateTime );

            box.IfValidProperty( nameof( box.Bag.AchievementType ),
                () => entity.AchievementTypeId = box.Bag.AchievementType.GetEntityId<AchievementType>( RockContext ).Value );

            box.IfValidProperty( nameof( box.Bag.AchieverEntityId ),
                () => entity.AchieverEntityId = box.Bag.AchieverEntityId.AsInteger() );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override AchievementAttempt GetInitialEntity()
        {
            return _attempt ?? ( _attempt = GetInitialEntity<AchievementAttempt, AchievementAttemptService>( RockContext, PageParameterKey.AchievementAttemptId ) );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( AchievementAttempt entity )
        {
            var achievementTypeId = entity.AchievementTypeId.ToString();

            if ( achievementTypeId == "0" )
            {
                achievementTypeId = PageParameter( PageParameterKey.AchievementTypeId );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.AchievementTypeId] = achievementTypeId
                } )
            };
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

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out AchievementAttempt entity, out BlockActionResult error )
        {
            var entityService = new AchievementAttemptService( RockContext );
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
                personImageStringBuilder.AppendFormat( photoFormat, personAlias.PersonId, personAlias.Person.PhotoUrl, personAlias.Person.PhotoUrl );
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
                    var service = new AchievementAttemptService( RockContext );
                    _attempt = service.Queryable().FirstOrDefault( saa => saa.Id == attemptId.Value );
                }
            }

            return _attempt;
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
                var service = new EntityTypeService( RockContext );
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

        /// <summary>
        /// Hydrates extra bag properties for the view panel display.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        private AchievementAttemptBag GetAttemptDetailsForBag( AchievementAttemptBag bag, AchievementAttempt entity )
        {
            var achiever = GetAchiever();
            bag.AchieverHtml = GetPersonHtml( achiever );
            bag.ProgressHtml = GetProgressHtml();
            bag.AttemptDescription = GetAttemptDateRangeString();
            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            return bag;
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

            return ActionOk( new ValidPropertiesBox<AchievementAttemptBag>
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
        public BlockActionResult Save( ValidPropertiesBox<AchievementAttemptBag> box )
        {
            var entityService = new AchievementAttemptService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            var achievementType = AchievementTypeCache.Get( entity.AchievementTypeId );
            var progress = box.Bag.Progress.AsDecimal();

            if ( progress < 0m )
            {
                entity.Progress = 0m;
            }

            if ( progress > 1m && !achievementType.AllowOverAchievement )
            {
                entity.Progress = 1m;
            }

            var isSuccess = progress >= 1m;

            if ( !box.Bag.AchievementAttemptEndDateTime.HasValue && isSuccess && !achievementType.AllowOverAchievement )
            {
                entity.AchievementAttemptEndDateTime = RockDateTime.Today;
            }

            if ( box.Bag.AchievementAttemptEndDateTime.HasValue && box.Bag.AchievementAttemptEndDateTime < box.Bag.AchievementAttemptStartDateTime )
            {
                box.Bag.AchievementAttemptEndDateTime = box.Bag.AchievementAttemptStartDateTime;
            }

            entity.IsClosed = ( box.Bag.AchievementAttemptEndDateTime.HasValue && box.Bag.AchievementAttemptEndDateTime.Value < RockDateTime.Today ) || ( isSuccess && !achievementType.AllowOverAchievement );
            entity.AchievementAttemptStartDateTime = box.Bag.AchievementAttemptStartDateTime.DateTime;
            entity.AchievementAttemptEndDateTime = box.Bag.AchievementAttemptEndDateTime?.DateTime;
            entity.Progress = progress;
            entity.IsSuccessful = isSuccess;

            // Ensure everything is valid before saving.
            if ( !ValidateAchievementAttempt( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( !entity.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
            {
                entity.AllowPerson( Authorization.VIEW, GetCurrentPerson(), RockContext );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                entity.AllowPerson( Authorization.EDIT, GetCurrentPerson(), RockContext );
            }

            if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, GetCurrentPerson() ) )
            {
                entity.AllowPerson( Authorization.ADMINISTRATE, GetCurrentPerson(), RockContext );
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
            entity.LoadAttributes( RockContext );

            var bag = GetAttemptDetailsForBag( GetEntityBagForView( entity ), entity );

            return ActionOk( new ValidPropertiesBox<AchievementAttemptBag>
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
            var entityService = new AchievementAttemptService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            var parameters = new Dictionary<string, string>
            {
                [PageParameterKey.AchievementTypeId] = entity.AchievementTypeId.ToString()
            };

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl( parameters ) );
        }

        #endregion
    }
}
