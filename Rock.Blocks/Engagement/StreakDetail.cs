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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Engagement.StreakDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Rock.Blocks.Engagement
{
    /// <summary>
    /// Displays the details of a particular streak.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Streak Detail" )]
    [Category( "Engagement" )]
    [Description( "Displays the details of a particular streak." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "867abce8-47a9-46fa-8a35-47ebbc60c4fe" )]
    [Rock.SystemGuid.BlockTypeGuid( "1c98107f-dfbf-44bd-a860-0c9df2e6c495" )]
    public class StreakDetail : RockDetailBlockType
    {
        private readonly int ChartBitsToShow = 350;
        #region Keys

        private static class PageParameterKey
        {
            public const string StreakId = "StreakId";
            public const string StreakTypeId = "StreakTypeId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string CancelLink = "CancelLink";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<StreakBag, StreakDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );
                if(box.Entity == null)
                {
                    return box;
                }

                box.NavigationUrls = GetBoxNavigationUrls( StreakTypeCache.GetId( box.Entity.StreakType.Value.AsGuid() ).ToString() );
                box.Options = GetBoxOptions( box.IsEditable, box.Entity, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<Streak>();

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
        private StreakDetailOptionsBag GetBoxOptions( bool isEditable, StreakBag entity, RockContext rockContext )
        {
            var options = new StreakDetailOptionsBag
            {
                CurrentStreak = GetStreakStateString( entity.CurrentStreakCount, entity.CurrentStreakStartDate ),
                LongestStreak = GetStreakStateString( entity.LongestStreakCount, entity.LongestStreakStartDate, entity.LongestStreakEndDate ),
                ChartHTML = StreakChartHTML( rockContext, entity ),
                personHTML = GetPersonHtml( rockContext, entity )
            };

            return options;
        }

        /// <summary>
        /// Validates the Streak for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="streak">The Streak to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Streak is valid, <c>false</c> otherwise.</returns>
        private bool ValidateStreak( Streak streak, RockContext rockContext, out string errorMessage )
        {
            errorMessage = "";
            if ( streak.IsValid )
            {
                return true;
            }
            var validationResult = streak.ValidationResults.FirstOrDefault();
            var message = validationResult == null ? "The values entered are not valid." : validationResult.ErrorMessage;
            errorMessage = message;
            return false;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<StreakBag, StreakDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Streak.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Streak.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Streak.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="StreakBag"/> that represents the entity.</returns>
        private StreakBag GetCommonEntityBag( Streak entity )
        {
            if ( entity == null )
            {
                return null;
            }
            var streakTypeId = entity.StreakTypeId;
            if ( streakTypeId == 0 )
            {
                streakTypeId = PageParameter( PageParameterKey.StreakTypeId ).AsInteger();
            }
            ListItemBag streakType = entity.StreakType?.ToListItemBag() ?? StreakTypeCache.Get( streakTypeId ).ToListItemBag() ?? new ListItemBag ();
            return new StreakBag
            {
                IdKey = entity.IdKey,
                EnrollmentDate = entity.EnrollmentDate,
                Location = entity.Location.ToListItemBag(),
                PersonAlias = entity.PersonAlias.ToListItemBag(),
                CurrentStreakCount = entity.CurrentStreakCount,
                CurrentStreakStartDate = entity.CurrentStreakStartDate,
                IsActive = entity.IsActive,
                LongestStreakCount = entity.LongestStreakCount,
                LongestStreakEndDate = entity.LongestStreakEndDate,
                LongestStreakStartDate = entity.LongestStreakStartDate,
                StreakType = streakType
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="StreakBag"/> that represents the entity.</returns>
        private StreakBag GetEntityBagForView( Streak entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="StreakBag"/> that represents the entity.</returns>
        private StreakBag GetEntityBagForEdit( Streak entity )
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
        private bool UpdateEntityFromBox( Streak entity, DetailBlockBox<StreakBag, StreakDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.EnrollmentDate ),
                () => entity.EnrollmentDate = box.Entity.EnrollmentDate );

            box.IfValidProperty( nameof( box.Entity.Location ),
                () => entity.LocationId = box.Entity.Location.GetEntityId<Location>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.PersonAlias ),
                () => entity.PersonAliasId = box.Entity.PersonAlias.GetEntityId<PersonAlias>( rockContext ).Value );


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
        /// <returns>The <see cref="Streak"/> to be viewed or edited on the page.</returns>
        private Streak GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<Streak, StreakService>( rockContext, PageParameterKey.StreakId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls( string streakTypeId )
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [NavigationUrlKey.CancelLink] = this.GetParentPageUrl( new Dictionary<string, string>
                {
                    { PageParameterKey.StreakTypeId, streakTypeId }
                } )
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
        private string GetSecurityGrantToken( Streak entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out Streak entity, out BlockActionResult error )
        {
            var entityService = new StreakService( rockContext );
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
                entity = new Streak();
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Streak.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Streak.FriendlyTypeName}." );
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<StreakBag, StreakDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<StreakBag, StreakDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new StreakService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var isNew = entity.Id == 0;
                string errorMessage = "";

                if ( isNew )
                {
                    var personId = new PersonAliasService( rockContext ).GetPersonId( box.Entity.PersonAlias.Value.AsGuid() ).ToIntSafe();
                    var streakTypeCache = StreakTypeCache.Get( box.Entity.StreakType.Value.AsGuid() );
                    var streakTypeService = new StreakTypeService( rockContext );
                    entity = streakTypeService.Enroll( streakTypeCache, personId, out errorMessage, entity.EnrollmentDate, entity.LocationId );

                    if ( !entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        entity.AllowPerson( Authorization.VIEW, RequestContext.CurrentPerson, rockContext );
                    }

                    if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                    {
                        entity.AllowPerson( Authorization.EDIT, RequestContext.CurrentPerson, rockContext );
                    }

                    if ( !entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                    {
                        entity.AllowPerson( Authorization.ADMINISTRATE, RequestContext.CurrentPerson, rockContext );
                    }
                }
                else
                {
                    // Update the entity instance from the information in the bag.
                    if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                    {
                        return ActionBadRequest( "Invalid data." );
                    }
                }

                // Ensure everything is valid before saving.
                string validationMessage = "";
                if ( entity == null )
                {
                    return ActionBadRequest( errorMessage );
                }
                if ( !ValidateStreak( entity, rockContext, out validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.StreakId] = entity.IdKey
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
                var entityService = new StreakService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, RequestContext.CurrentPerson, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                var streakTypeId = entity.StreakType?.Id ?? entity.StreakTypeId;

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl( new Dictionary<string, string> {
                    { PageParameterKey.StreakTypeId, streakTypeId.ToString() }
                } ) );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<StreakBag, StreakDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<StreakBag, StreakDetailOptionsBag>
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

        /// <summary>
        /// Gets the streak state string.
        /// </summary>
        /// <param name="streakCount">The streak count.</param>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        private string GetStreakStateString( int streakCount, DateTime? start, DateTime? end = null )
        {
            var dateString = GetStreakDateRangeString( start, end );

            if ( dateString.IsNullOrWhiteSpace() )
            {
                return streakCount.ToString();
            }

            return string.Format( "{0} <small>{1}</small>", streakCount, dateString );
        }

        /// <summary>
        /// Gets the streak date range string.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        private string GetStreakDateRangeString( DateTime? start, DateTime? end = null )
        {
            if ( !start.HasValue && !end.HasValue )
            {
                return string.Empty;
            }

            if ( !start.HasValue )
            {
                return string.Format( "Ended on {0}", end.ToShortDateString() );
            }

            if ( !end.HasValue )
            {
                return string.Format( "Started on {0}", start.ToShortDateString() );
            }

            return string.Format( "Ranging from {0} - {1}", start.ToShortDateString(), end.ToShortDateString() );
        }

        /// <summary>
        /// Gets the person HTML.
        /// </summary>
        /// <returns></returns>
        private string GetPersonHtml( RockContext rockContext, StreakBag entity )
        {
            var personImageStringBuilder = new StringBuilder();
            if ( entity.PersonAlias == null )
            {
                return "";
            }
            var person = new PersonAliasService( rockContext ).GetPerson( entity.PersonAlias.Value.AsGuid() );
            const string photoFormat = "<div class=\"photo-icon photo-round photo-round-sm pull-left margin-r-sm js-person-popover\" personid=\"{0}\" data-original=\"{1}&w=50\" style=\"background-image: url({1}); background-size: cover; background-repeat: no-repeat;\"></div>";
            const string nameLinkFormat = @"
    {0}
    <p><small><a href='/Person/{1}'>View Profile</a></small></p>
";
            if ( person == null )
            {
                return "";
            }

            personImageStringBuilder.AppendFormat( photoFormat, person.Id, person.PhotoUrl );
            personImageStringBuilder.AppendFormat( nameLinkFormat, person.FullName, person.Id );

            if ( person.TopSignalColor.IsNotNullOrWhiteSpace() )
            {
                personImageStringBuilder.Append( person.GetSignalMarkup() );
            }

            return personImageStringBuilder.ToString();
        }

        /// <summary>
        /// The Streak Chart in HTML
        /// </summary>
        private string StreakChartHTML( RockContext rockContext, StreakBag entity )
        {
            var occurrenceEngagement = GetOccurrenceEngagement( rockContext, entity );

            if ( occurrenceEngagement == null )
            {
                return "";
            }

            var stringBuilder = new StringBuilder();
            var bitItemFormat = @"<li class=""binary-state-graph-bit {2} {3}"" title=""{0}""><span style=""height: {1}%""></span></li>";

            for ( var i = 0; i < occurrenceEngagement.Length; i++ )
            {
                var occurrence = occurrenceEngagement[i];
                var hasEngagement = occurrence != null && occurrence.HasEngagement;
                var hasExclusion = occurrence != null && occurrence.HasExclusion;
                var title = occurrence != null ? occurrence.DateTime.ToShortDateString() : string.Empty;

                stringBuilder.AppendFormat( bitItemFormat,
                    title, // 0
                    hasEngagement ? 100 : 5, // 1
                    hasEngagement ? "has-engagement" : string.Empty, // 2
                    hasExclusion ? "has-exclusion" : string.Empty ); // 3
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Get the recent bits data for the chart
        /// </summary>
        /// <returns></returns>
        private OccurrenceEngagement[] GetOccurrenceEngagement( RockContext rockContext, StreakBag entity )
        {
            if ( entity.StreakType == null || entity.PersonAlias == null )
            {
                return null;
            }
            OccurrenceEngagement[] occurrenceEngagement = null;
            var streakTypeService = new StreakTypeService( rockContext );
            var streakTypeId = StreakTypeCache.Get( entity.StreakType.Value ).Id;
            var personId = new PersonAliasService( rockContext ).GetPerson( entity.PersonAlias.Value.AsGuid() ).Id;

            if ( personId > 0 && streakTypeId > 0 )
            {
                var errorMessage = string.Empty;
                occurrenceEngagement = streakTypeService.GetRecentEngagementBits( streakTypeId, personId, ChartBitsToShow, out errorMessage );
            }

            return occurrenceEngagement;
        }

        /// <summary>
        /// Performs the rebuild action on the Streak
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult Rebuild( string key )
        {
            if ( key.IsNullOrWhiteSpace() )
            {
                return ActionNotFound();
            }
            using ( RockContext rockContext = new RockContext() )
            {
                var streakService = new StreakService( rockContext );
                var streak = streakService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( !streak.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                {
                    return ActionUnauthorized( "You are not authorized to rebuild this item." );
                }

                var errorMessage = string.Empty;
                StreakTypeService.RebuildStreakFromAttendance( streak.StreakTypeId, streak.PersonAlias.PersonId, out errorMessage );
                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk( "The streak rebuild was successful!" );
            }
        }

        #endregion
    }
}
